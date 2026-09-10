using BrynzaAPI;
using CaeliImperium.NetworkMessages;
using RoR2;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components.Refinery;

public class PipelineBuilder : NetworkBehaviour
{
    public static FixedConditionalWeakTable<Interactor, InteractionDriver> keyValuePairs = [];
    [SyncVar] public NetworkInstanceId pipelineRefineryControllerNetworkInstanceId;
    private PipelineRefineryController _pipelineRefineryController;
    public PipelineRefineryController pipelineRefineryController
    {
        get
        {
            if (!_pipelineRefineryController)
            {
                GameObject gameObject = Util.FindNetworkObject(pipelineRefineryControllerNetworkInstanceId);
                if (gameObject)
                {
                    _pipelineRefineryController = gameObject.GetComponent<PipelineRefineryController>();
                }
            }
            return _pipelineRefineryController;
        }
    }
    public ZiprailController ziprailController;
    public BasicBezierSpline basicBezierSpline;
    public bool initialized;
    [SyncVar] public bool canBeInteracted = true;
    [SyncVar] public bool completed;
    public Interactor currentInteractor;
    public GameObject pointPrefab;
    public GameObject pipePrefab;
    public Material validMaterial;
    public Material invalidMaterial;
    public Material buildMaterial;
    public Vector3 addPositionTobezierSegment = new Vector3(0f, 1f, 0f);
    public float endFindRaycastAddDistance = 15f;
    public float firstRaycastAddDistance = 2f;
    public float secondRaycastAddDistance = 9f;
    public float minLength = 2f;
    public float maxLength = 15f;
    public float maxBendAngle = 90f;
    public float capsuleCheckRadius = 0.5f;
    public float handleLengthFactor = 1f;
    public int checkSamples = 10;
    public Transform startObject;
    public List<PipelineInteractor> pipelineInteractors = [];
    public List<PipeMeshGenerator> pipeMeshGenerators = [];
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
            if (!currentPipelineInteractor) return Vector3.zero;
            return currentPipelineInteractor.startOffset;
        }
    }
    private GameObject previewPipeObj;
    private PipeMeshGenerator previewPipeMeshGenerator;
    private MeshRenderer previewPipeRenderer;
    private PipelineInteractor currentPipelineInteractor;
    private List<GameObject> generatedControlPoints;
    public void Awake()
    {
        if (!ziprailController) ziprailController = GetComponent<ZiprailController>();
        if (!basicBezierSpline) basicBezierSpline = GetComponent<BasicBezierSpline>();
    }
    public void Start()
    {
        Init();
    }
    public void Init()
    {
        if (initialized) return;
        initialized = true;
        CreatePreviewPipe();
        if (currentSourceNode || !NetworkServer.active) return;
        GameObject newNode = Instantiate(pointPrefab, startObject);
        PipelineInteractor pipelineInteractor = newNode.GetComponent<PipelineInteractor>();
        pipelineInteractor.pipelineBuilderNetworkInstanceId = netId;
        NetworkServer.Spawn(newNode);
    }
    public void OnNetworkIdentityChanged(NetworkIdentity currentNetworkIdentity)
    {
        currentInteractor = currentNetworkIdentity.GetComponent<Interactor>();
    }
    public void OnInteract(Interactor interactor)
    {
        if (!canBeInteracted) return;
        CharacterBody characterBody = interactor.GetCharacterBody();
        if (!characterBody || !characterBody.isPlayerControlled || !characterBody.inputBank) return;
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
        if (!currentInteractor || !Util.HasEffectiveAuthority(currentInteractor.netIdentity)) return;
        Camera camera = Camera.main;
        if (!camera) return;
        Util.PlaySound("Play_DRG_Hologram_Up", camera.gameObject);
    }
    [Command]
    public void CmdRemoveCurrentInteractor() => RpcRemoveCurrentInteractor();
    [ClientRpc]
    public void RpcRemoveCurrentInteractor()
    {
        if (NetworkServer.active) return;
        currentInteractor = null;
    }
    public void CallPlaceNode(Vector3 position, Vector3 normal, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, bool end)
    {
        if (NetworkServer.active)
        {
            PlaceNode(position, normal, p0, p1, p2, p3, end);
            RpcPlaceNode(position, normal, p0, p1, p2, p3, end);
        }
        else
        {
            PipelineBuilderPlaceNodeNetMessage.SendToServer(netId, position, normal, p0, p1, p2, p3, end);
        }
    }
    [Command]
    public void CmdPlaceNode(Vector3 position, Vector3 normal, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, bool end) => RpcPlaceNode(position, normal, p0, p1, p2, p3, end);
    [ClientRpc]
    public void RpcPlaceNode(Vector3 position, Vector3 normal, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, bool end)
    {
        if (NetworkServer.active) return;
        PlaceNode(position, normal, p0, p1, p2, p3, end);
    }
    public void CallPlacePreview(Vector3 position, Vector3 normal, bool valid)
    {
        PlacePreview(position, normal, valid);
        if (NetworkServer.active)
        {
            RpcPlacePreview(position, normal, valid);
        }
        else
        {
            CmdPlacePreview(position, normal, valid);
        }
    }
    [ClientRpc]
    public void RpcPlacePreview(Vector3 position, Vector3 normal, bool valid)
    {
        if (NetworkServer.active) return;
        if (currentInteractor && currentInteractor.hasAuthority) return;
        PlacePreview(position, normal, valid);
    }
    [Command]
    public void CmdPlacePreview(Vector3 position, Vector3 normal, bool valid) => RpcPlacePreview(position, normal, valid);
    public void CtsmPlacePreview(ClientToServerMessenger clientToServerMessenger, Vector3 position, Vector3 normal, bool valid) => clientToServerMessenger.CmdPipelineBuilderPlacePreview(netId, position, normal, valid);
    public void PlacePreview(Vector3 position, Vector3 normal, bool valid)
    {
        CalculateControlPoints(start.position, start.forward, position, normal, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3);
        if (!previewPipeObj.activeSelf) previewPipeObj.SetActive(true);
        previewPipeMeshGenerator.GeneratePipe(p0, p1, p2, p3);
        previewPipeRenderer.material = valid ? validMaterial : invalidMaterial;
    }
    public void CallDisablePreview()
    {
        DisablePreview();
        if (NetworkServer.active)
        {
            RpcDisablePreview();
        }
        else
        {
            CmdDisablePreview();
        }
    }
    [Command]
    public void CmdDisablePreview() => RpcDisablePreview();
    [ClientRpc]
    public void RpcDisablePreview()
    {
        if (currentInteractor && currentInteractor.hasAuthority) return;
        DisablePreview();
    }
    public void DisablePreview()
    {
        if (previewPipeObj.activeSelf) previewPipeObj.SetActive(false);
    }
    [ClientRpc]
    public void RpcSetCurrentSourceNode(NetworkInstanceId networkInstanceId)
    {
        if (NetworkServer.active) return;
        GameObject gameObject = Util.FindNetworkObject(networkInstanceId);
        if (!gameObject) return;
        currentSourceNode = gameObject.transform;
        PipelineInteractor pipelineInteractor = gameObject.GetComponent<PipelineInteractor>();
        if (!pipelineInteractor) return;
        pipelineInteractor.pipelineBuilderNetworkInstanceId = netId;
    }
    [ClientRpc]
    public void RpcRemoveCurrentSourceNode()
    {
        if (NetworkServer.active) return;
        currentSourceNode = null;
    }
    public void FixedUpdate()
    {
        if (!currentInteractor) return;
        CharacterBody characterBody = currentInteractor.GetCharacterBody();
        if (!characterBody)
        {
            //if (NetworkServer.active) CallRemoveCurrentInteractor();
            return;
        }
        InputBankTest inputBankTest = characterBody.inputBank;
        if (!inputBankTest)
        {
            //if (NetworkServer.active) CallRemoveCurrentInteractor();
            return;
        }
        if (!characterBody.hasEffectiveAuthority) return;
        if (ziprailController && ziprailController is ZiprailControllerWithInactivityDuration ziprailControllerWithInactivityDuration) ziprailControllerWithInactivityDuration.SetInactivityDuration();
        Vector3 point;
        Ray ray1 = characterBody.GetAimRay();
        Vector3 endPoistion;
        Vector3 endNormal;
        bool canPlace;
        bool end = false;
        bool pressed = inputBankTest.interact.justPressed;
        WellExtractorController wellExtractor = null;
        if (Physics.Raycast(ray1, out RaycastHit hitInfo1, characterBody.bestFitActualRadius + endFindRaycastAddDistance, LayerIndex.CommonMasks.interactable, QueryTriggerInteraction.Collide))
        {
            Collider collider = hitInfo1.collider;
            if (collider)
            {
                EntityLocator entityLocator = collider.GetComponent<EntityLocator>();
                if (entityLocator && entityLocator.entity)
                {
                    wellExtractor = entityLocator.entity.GetComponent<WellExtractorController>();
                    if (wellExtractor && !wellExtractor.connected)
                    {
                        endPoistion = wellExtractor.pipeEndTransform ? wellExtractor.pipeEndTransform.position : wellExtractor.transform.position;
                        Vector3 vector3 = ray1.origin - endPoistion;
                        vector3.y = 0f;
                        vector3.Normalize();
                        if (vector3 == Vector3.zero) vector3 = (ray1.origin - endPoistion).normalized;
                        endNormal = vector3;
                        canPlace = true;
                        end = true;
                        goto skipNormalPlacement;
                    }
                }
            }
        }
        if (Physics.Raycast(ray1, out RaycastHit hitInfo, characterBody.bestFitActualRadius + firstRaycastAddDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
        {
            point = hitInfo.point;
        }
        else
        {
            point = ray1.origin + ray1.direction * (characterBody.bestFitActualRadius + firstRaycastAddDistance);
        }
        Ray ray = new Ray(point, Physics.gravity.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, secondRaycastAddDistance, LayerIndex.world.mask))
        {
            canPlace = true;
            Vector3 targetPos = hit.point;
            Vector3 aimDirection = inputBankTest.aimDirection;
            aimDirection.y = 0f;
            aimDirection.Normalize();
            if (aimDirection == Vector3.zero) aimDirection = characterBody.transform.forward;
            aimDirection *= -1f;
            endPoistion = targetPos + endPositionOffset;
            endNormal = aimDirection;
        }
        else
        {
            endPoistion = Vector3.zero;
            endNormal = Vector3.zero;
            canPlace = false;
        }
        skipNormalPlacement:
        if (canPlace)
        {
            CalculateControlPoints(start.position, start.forward, endPoistion, endNormal, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3);
            bool isValid = ValidateSegment(p0, p1, p2, p3);
            PlacePreview(endPoistion, endNormal, isValid);
            if (NetworkServer.active)
            {
                RpcPlacePreview(endPoistion, endNormal, isValid);
            }
            else
            {
                PipelineBuilderPlacePreviewNetMessage.SendToClients(netId, endPoistion, endNormal, isValid);
            }
            if (pressed)
            {
                if (isValid)
                {
                    if (NetworkServer.active)
                    {
                        PlaceNode(endPoistion, endNormal * -1f, p0, p1, p2, p3, end);
                        RpcPlaceNode(endPoistion, endNormal * -1f, p0, p1, p2, p3, end);
                    }
                    else
                    {
                        PipelineBuilderPlaceNodeNetMessage.SendToServer(netId, endPoistion, endNormal * -1f, p0, p1, p2, p3, end);
                    }
                    if (wellExtractor)
                    {
                        if (NetworkServer.active)
                        {
                            wellExtractor.OnPipeConnected(endNormal, netId);
                            wellExtractor.RpcOnPipeConnected(endNormal, netId);
                        }
                        else
                        {
                            WellExtractorControllerOnPipeConnectedNetMessage.SendToServer(wellExtractor.netId, endNormal, netId);
                        }
                    }
                }
                else
                {
                    DisablePreview();
                    currentInteractor = null;
                    if (NetworkServer.active)
                    {
                        RpcDisablePreview();
                        RpcRemoveCurrentInteractor();
                    }
                    else
                    {
                        PipelineBuilderDisablePreviewNetMessage.SendToServer(netId);
                        PipelineBuilderRemoveCurrentInteractorNetMessage.SendToServer(netId);
                    }
                }
            }
        }
        else
        {
            bool disable = previewPipeObj.activeSelf;
            if (disable) DisablePreview();
            if (NetworkServer.active)
            {
                if (disable) RpcDisablePreview();
                if (pressed) RpcRemoveCurrentInteractor();
            }
            else
            {
                if (disable) PipelineBuilderDisablePreviewNetMessage.SendToServer(netId);
                if (pressed) PipelineBuilderRemoveCurrentInteractorNetMessage.SendToServer(netId);
            }
                
        }
    }
    public void CreatePreviewPipe()
    {
        previewPipeObj = Instantiate(pipePrefab);
        previewPipeMeshGenerator = previewPipeObj.GetComponent<PipeMeshGenerator>();
        previewPipeRenderer = previewPipeObj.GetComponent<MeshRenderer>();
        if (previewPipeMeshGenerator)
        {
            if (previewPipeMeshGenerator.meshCollider) previewPipeMeshGenerator.meshCollider.enabled = false;
            previewPipeMeshGenerator.setCollider = false;
        }
        
    }
    public void CalculateControlPoints(Vector3 startPos, Vector3 startForward, Vector3 endPos, Vector3 endNormal, out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3)
    {
        p0 = startPos;
        p3 = endPos;
        float distance = Vector3.Distance(p0, p3);
        float handleLength = distance * handleLengthFactor;
        p1 = p0 + startForward * handleLength;
        p2 = p3 + endNormal * handleLength;
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
    public void PlaceNode(Vector3 position, Vector3 normal, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, bool end)
    {
        currentInteractor = null;
        DisablePreview();
        GameObject placedPipe = Instantiate(pipePrefab);
        PipeMeshGenerator pipeMeshGenerator = placedPipe.GetComponent<PipeMeshGenerator>();
        if (pipeMeshGenerator)
        {
            pipeMeshGenerator.GeneratePipe(p0, p1, p2, p3);
            pipeMeshGenerators.Add(pipeMeshGenerator);
        }
        MeshRenderer meshRenderer = placedPipe.GetComponent<MeshRenderer>();
        if (meshRenderer) meshRenderer.material = buildMaterial;
        EntityLocator entityLocator = placedPipe.GetComponent<EntityLocator>();
        if (entityLocator) entityLocator.entity = gameObject;
        Quaternion rotation = Quaternion.LookRotation(normal);
        if (ziprailController && ziprailController is ZiprailControllerWithInactivityDuration ziprailControllerWithInactivityDuration) ziprailControllerWithInactivityDuration.SetInactivityDuration();
        if (!NetworkServer.active) return;
        GameObject newNode = Instantiate(pointPrefab, position - endPositionOffset, rotation);
        PipelineInteractor pipelineInteractor = newNode.GetComponent<PipelineInteractor>();
        if (pipelineInteractor)
        {
            pipelineInteractor.pipelineBuilderNetworkInstanceId = netId;
            pipelineInteractor.bezierSegmentData = new BezierSegmentData(p0 + addPositionTobezierSegment, p1 + addPositionTobezierSegment, p2 + addPositionTobezierSegment, p3 + addPositionTobezierSegment);
        }
        NetworkServer.Spawn(newNode);
    }
    public void CompleteBuilder()
    {
        if (!NetworkServer.active) return;
        canBeInteracted = false;
        completed = true;
        if (pipelineRefineryController) pipelineRefineryController.currentlyCompletedBuilders++;
    }
    public void RebuildZiprail()
    {
        if (generatedControlPoints != null)
        {
            for (int i = 0; i < generatedControlPoints.Count; i++)
            {
                GameObject gameObject = generatedControlPoints[i];
                if (!gameObject) continue;
                Destroy(gameObject);
            }
            generatedControlPoints.Clear();
        }
        List<BezierSegmentData> bezierSegmentDatas = [];
        foreach (PipelineInteractor pipelineInteractor in pipelineInteractors)
        {
            if (!pipelineInteractor || pipelineInteractor.bezierSegmentData.Equals(BezierSegmentData.Empty)) continue;
            bezierSegmentDatas.Add(pipelineInteractor.bezierSegmentData);
        }
        basicBezierSpline = CaeliImperiumUtils.CreateSplineFromSegments(gameObject, [.. bezierSegmentDatas], out generatedControlPoints);
    }
}
