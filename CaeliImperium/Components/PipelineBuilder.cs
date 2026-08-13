using BrynzaAPI;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components;

public class PipelineBuilder : NetworkBehaviour
{
    public Interactor currentInteractor;
    public GameObject pointPrefab;
    public GameObject pipePrefab;
    public Material validMaterial;
    public Material invalidMaterial;
    public float firstRaycastAddDistance = 2f;
    public float secondRaycastAddDistance = 9f;
    public float minLength = 2.0f;
    public float maxLength = 15.0f;
    public float maxBendAngle = 60.0f;
    public float capsuleCheckRadius = 0.5f;
    public float handleLengthFactor = 0.4f;
    public int checkSamples = 10;
    public Transform startObject;
    private Transform _currentSourceNode;
    public Transform currentSourceNode
    {
        get => _currentSourceNode;
        set
        {
            _currentSourceNode = value;
            if (!_currentSourceNode) return;
            currentPipelineInteractor = _currentSourceNode.GetComponent<PipelineInteractor>();
        }
    }
    public Transform start => currentPipelineInteractor ? currentPipelineInteractor.start ?? currentPipelineInteractor.transform : currentSourceNode;
    public Vector3 endPositionOffset
    {
        get
        {
            if (start == currentSourceNode) return Vector3.zero;
            return start.localPosition;
        }
    }
    private GameObject previewPipeObj;
    private PipeMeshGenerator previewPipeMesh;
    private MeshRenderer previewPipeRenderer;
    private PipelineInteractor currentPipelineInteractor;

    public void Start()
    {
        startObject.SetParent(null, true);
        currentSourceNode = startObject;
        CreatePreviewPipe();
    }
    public void OnNetworkIdentityChanged(NetworkIdentity currentNetworkIdentity)
    {
        currentInteractor = currentNetworkIdentity.GetComponent<Interactor>();
    }
    public void OnInteract(Interactor interactor)
    {
        CallUpdateCurrentInteractor(interactor.netId);
    }
    public void CallUpdateCurrentInteractor(NetworkInstanceId networkInstanceId)
    {
        if (NetworkServer.active)
        {
            RpcUpdateCurrentInteractor(networkInstanceId);
        }
        else
        {
            CmdUpdateCurrentInteractor(networkInstanceId);
        }
    }
    public void CallRemoveCurrentInteractor()
    {
        if (NetworkServer.active)
        {
            RpcRemoveCurrentInteractor();
        }
        else
        {
            CmdRemoveCurrentInteractor();
        }
    }
    [Command]
    public void CmdUpdateCurrentInteractor(NetworkInstanceId networkInstanceId) => RpcUpdateCurrentInteractor(networkInstanceId);
    [ClientRpc]
    public void RpcUpdateCurrentInteractor(NetworkInstanceId networkInstanceId)
    {
        GameObject gameObject = Util.FindNetworkObject(networkInstanceId);
        if (!gameObject) return;
        currentInteractor = gameObject.GetComponent<Interactor>();
    }
    [Command]
    public void CmdRemoveCurrentInteractor() => RpcRemoveCurrentInteractor();
    [ClientRpc]
    public void RpcRemoveCurrentInteractor()
    {
        currentInteractor = null;
    }
    public void FixedUpdate()
    {
        if (!currentInteractor) return;
        CharacterBody characterBody = currentInteractor.GetCharacterBody();
        if (!characterBody) return;
        InputBankTest inputBankTest = characterBody.inputBank;
        if (!inputBankTest) return;
        if (!characterBody.isPlayerControlled || !characterBody.hasAuthority) return;
        Vector3 point;
        Ray ray1 = characterBody.GetAimRay();
        if (Physics.Raycast(ray1, out RaycastHit hitInfo, characterBody.bestFitActualRadius + firstRaycastAddDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
        {
            point = hitInfo.point;
        }
        else
        {
            point = ray1.origin + (ray1.direction * (characterBody.bestFitActualRadius + firstRaycastAddDistance));
        }
        Ray ray = new Ray(point, Physics.gravity.normalized);
        bool pressed = inputBankTest.interact.justPressed;
        if (Physics.Raycast(ray, out RaycastHit hit, secondRaycastAddDistance, LayerIndex.world.mask))
        {
            Vector3 targetPos = hit.point;
            Vector3 targetNormal = hit.normal;
            Vector3 aimDirection = inputBankTest.aimDirection;
            aimDirection.y = 0f;
            aimDirection.Normalize();
            if (aimDirection == Vector3.zero) aimDirection = characterBody.transform.forward;
            aimDirection *= -1f;
            CalculateControlPoints(start.position, start.forward, targetPos + endPositionOffset, aimDirection, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3);
            bool isValid = ValidateSegment(p0, p1, p2, p3);
            previewPipeObj.SetActive(true);
            previewPipeMesh.GeneratePipe(p0, p1, p2, p3);
            previewPipeRenderer.material = isValid ? validMaterial : invalidMaterial;
            if (pressed)
            {
                if (isValid) PlaceNode(targetPos, aimDirection * -1f, p0, p1, p2, p3);
                CallRemoveCurrentInteractor();
            }
        }
        else
        {
            previewPipeObj.SetActive(false);
            if (pressed) CallRemoveCurrentInteractor();
        }
    }
    public void CreatePreviewPipe()
    {
        previewPipeObj = Instantiate(pipePrefab);
        previewPipeMesh = previewPipeObj.GetComponent<PipeMeshGenerator>();
        previewPipeRenderer = previewPipeObj.GetComponent<MeshRenderer>();
    }
    public void CalculateControlPoints(Vector3 startPos, Vector3 startForward, Vector3 endPos, Vector3 endNormal, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3)
    {
        p0 = startPos;
        p3 = endPos;
        float distance = Vector3.Distance(p0, p3);
        float handleLength = distance * handleLengthFactor;
        p1 = p0 + (startForward * handleLength);
        p2 = p3 + (endNormal * handleLength);
    }
    public bool ValidateSegment(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 vector3 = p0 - p3;
        float sqrMagnitude = vector3.sqrMagnitude;
        if (sqrMagnitude < minLength * minLength || sqrMagnitude > maxLength * maxLength) return false;
        Vector3 lastDir = CaeliImperiumUtils.BezierGetFirstDerivative(p0, p1, p2, p3, 0f).normalized;
        for (int i = 1; i <= checkSamples; i++)
        {
            float t = (float)i / checkSamples;
            Vector3 currentDir = CaeliImperiumUtils.BezierGetFirstDerivative(p0, p1, p2, p3, t).normalized;
            float angle = Vector3.Angle(lastDir, currentDir);
            if (angle > maxBendAngle) return false;
            lastDir = currentDir;
        }
        for (int i = 0; i < checkSamples; i++)
        {
            float t1 = (float)i / checkSamples;
            float t2 = (float)(i + 1) / checkSamples;
            Vector3 pos1 = CaeliImperiumUtils.BezierGetPoint(p0, p1, p2, p3, t1);
            Vector3 pos2 = CaeliImperiumUtils.BezierGetPoint(p0, p1, p2, p3, t2);
            if (Physics.Linecast(pos1, pos2, LayerIndex.world.mask))  return false;
        }
        return true;
    }
    public void PlaceNode(Vector3 position, Vector3 normal, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        GameObject placedPipe = Instantiate(pipePrefab);
        PipeMeshGenerator pipeMeshGenerator = placedPipe.GetComponent<PipeMeshGenerator>();
        if (pipeMeshGenerator) pipeMeshGenerator.GeneratePipe(p0, p1, p2, p3);
        MeshRenderer meshRenderer = placedPipe.GetComponent<MeshRenderer>();
        if (meshRenderer) meshRenderer.material = validMaterial;
        Quaternion rotation = Quaternion.LookRotation(normal);
        GameObject newNode = Instantiate(pointPrefab, position, rotation);
        currentSourceNode = newNode.transform;
        PipelineInteractor pipelineInteractor = newNode.GetComponent<PipelineInteractor>();
        if (pipelineInteractor) pipelineInteractor.pipelineBuilder = this;
    }
}
