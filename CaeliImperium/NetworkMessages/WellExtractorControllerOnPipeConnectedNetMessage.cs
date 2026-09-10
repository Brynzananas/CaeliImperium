using CaeliImperium.Components.Refinery;
using R2API.Networking.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages;
public class WellExtractorControllerOnPipeConnectedNetMessage : INetMessage
{
    public NetworkInstanceId networkInstanceId;
    public Vector3 rotateTo;
    public NetworkInstanceId networkInstanceIdPipelineBuilder;
    public void Deserialize(NetworkReader reader)
    {
        networkInstanceId = reader.ReadNetworkId();
        rotateTo = reader.ReadVector3();
        networkInstanceIdPipelineBuilder = reader.ReadNetworkId();
    }
    public void OnReceived()
    {
        WellExtractorController wellExtractorController = networkInstanceId.GetWellExtractorController();
        if (!wellExtractorController) return;
        wellExtractorController.OnPipeConnected(rotateTo, networkInstanceIdPipelineBuilder);
        wellExtractorController.RpcOnPipeConnected(rotateTo, networkInstanceIdPipelineBuilder);
    }
    public void Serialize(NetworkWriter writer)
    {
        writer.Write(networkInstanceId);
        writer.Write(rotateTo);
        writer.Write(networkInstanceIdPipelineBuilder);
    }
    public static void SendToServer(NetworkInstanceId networkInstanceId, Vector3 rotateTo, NetworkInstanceId networkInstanceIdPipelineBuilder) => new WellExtractorControllerOnPipeConnectedNetMessage
    {
        networkInstanceId = networkInstanceId,
        rotateTo = rotateTo,
        networkInstanceIdPipelineBuilder = networkInstanceIdPipelineBuilder
    }.Send(R2API.Networking.NetworkDestination.Server);
}
