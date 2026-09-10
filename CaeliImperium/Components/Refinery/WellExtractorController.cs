using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components.Refinery;
public class WellExtractorController : NetworkBehaviour
{
    public bool connected;
    public Animator animator;
    public Transform pipeEndTransform;
    public Transform transformToRotate;
    public PositionIndicator positionIndicator;
    public float shaveRunTimer = 60f;
    public void CallOnPipeConnected(Vector3 rotateTo, NetworkInstanceId networkInstanceId)
    {
        if (connected) return;
        if (NetworkServer.active)
        {
            OnPipeConnected(rotateTo, networkInstanceId);
            RpcOnPipeConnected(rotateTo, networkInstanceId);
        }
        else
        {
            CmdOnPipeConnected(rotateTo, networkInstanceId);
        }
    }
    [Command]
    public void CmdOnPipeConnected(Vector3 rotateTo, NetworkInstanceId networkInstanceId) => CallOnPipeConnected(rotateTo, networkInstanceId);
    [ClientRpc]
    public void RpcOnPipeConnected(Vector3 rotateTo, NetworkInstanceId networkInstanceId)
    {
        if (NetworkServer.active) return;
        OnPipeConnected(rotateTo, networkInstanceId);
    }
    public void OnPipeConnected(Vector3 rotateTo, NetworkInstanceId networkInstanceId)
    {
        if (transformToRotate)
        {
            float z = transformToRotate.localEulerAngles.z;
            transformToRotate.forward = rotateTo;
            transformToRotate.localEulerAngles = new Vector3(transformToRotate.localEulerAngles.x, transformToRotate.localEulerAngles.y, z);
        }
        if (NetworkServer.active && Run.instance)
        {
            float newRunTimer = Mathf.Max(Run.instance.GetRunStopwatch() - shaveRunTimer, 1f);
            Run.instance.SetRunStopwatch(newRunTimer);
        }
        if (positionIndicator) positionIndicator.gameObject.SetActive(false);
        connected = true;
        GameObject gameObject = Util.FindNetworkObject(networkInstanceId);
        if (!gameObject) return;
        PipelineBuilder pipelineBuilder = gameObject.GetComponent<PipelineBuilder>();
        pipelineBuilder.CompleteBuilder();
        if (pipelineBuilder.pipelineRefineryController) pipelineBuilder.pipelineRefineryController.wellExtractorControllers.Add(this);
    }
}
