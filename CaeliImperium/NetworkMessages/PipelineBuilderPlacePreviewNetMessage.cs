using CaeliImperium.Components.Refinery;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages;
public class PipelineBuilderPlacePreviewNetMessage : INetMessage
{
    public NetworkInstanceId networkInstanceId;
    public Vector3 position;
    public Vector3 normal;
    public bool valid;
    public void Deserialize(NetworkReader reader)
    {
        networkInstanceId = reader.ReadNetworkId();
        position = reader.ReadVector3();
        normal = reader.ReadVector3();
        valid = reader.ReadBoolean();
    }
    public void OnReceived()
    {
        PipelineBuilder pipelineBuilder = networkInstanceId.GetPipelineBuilder();
        if (!pipelineBuilder) return;
        if (pipelineBuilder.currentInteractor && Util.HasEffectiveAuthority(pipelineBuilder.currentInteractor.netIdentity)) return;
        pipelineBuilder.PlacePreview(position, normal, valid);
    }
    public void Serialize(NetworkWriter writer)
    {
        writer.Write(networkInstanceId);
        writer.Write(position);
        writer.Write(normal);
        writer.Write(valid);
    }
    public static void SendToClients(NetworkInstanceId networkInstanceId, Vector3 position, Vector3 normal, bool valid) => new PipelineBuilderPlacePreviewNetMessage
    {
        networkInstanceId = networkInstanceId,
        position = position,
        normal = normal,
        valid = valid
    }.Send(R2API.Networking.NetworkDestination.Clients);
}



