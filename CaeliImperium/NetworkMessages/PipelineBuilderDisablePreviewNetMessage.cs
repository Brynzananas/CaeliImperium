using CaeliImperium.Components.Refinery;
using R2API.Networking.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages;
public class PipelineBuilderDisablePreviewNetMessage : INetMessage
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
        pipelineBuilder.DisablePreview();
        pipelineBuilder.RpcDisablePreview();
    }
    public void Serialize(NetworkWriter writer)
    {
        writer.Write(networkInstanceId);
    }
    public static void SendToServer(NetworkInstanceId networkInstanceId) => new PipelineBuilderDisablePreviewNetMessage
    {
        networkInstanceId = networkInstanceId
    }.Send(R2API.Networking.NetworkDestination.Server);
}