using CaeliImperium.Components.Refinery;
using R2API.Networking.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages;
public class PipelineBuilderRemoveCurrentInteractorNetMessage : INetMessage
{
    public NetworkInstanceId networkInstanceId;
    public void Deserialize(NetworkReader reader)
    {
        networkInstanceId = reader.ReadNetworkId();
    }
    public void OnReceived()
    {
        PipelineBuilder pipelineBuilder = networkInstanceId.GetPipelineBuilder();
        if (!pipelineBuilder) return;
        pipelineBuilder.currentInteractor = null;
        pipelineBuilder.RpcRemoveCurrentInteractor();
    }
    public void Serialize(NetworkWriter writer)
    {
        writer.Write(networkInstanceId);
    }
    public static void SendToServer(NetworkInstanceId networkInstanceId) => new PipelineBuilderRemoveCurrentInteractorNetMessage
    {
        networkInstanceId = networkInstanceId
    }.Send(R2API.Networking.NetworkDestination.Server);
}
