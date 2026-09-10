using CaeliImperium.Components.Refinery;
using R2API.Networking.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages;
public class PipelineBuilderPlaceNodeNetMessage : INetMessage
{
    public NetworkInstanceId networkInstanceId;
    public Vector3 position;
    public Vector3 normal;
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;
    public bool end;
    public void Deserialize(NetworkReader reader)
    {
        networkInstanceId = reader.ReadNetworkId();
        position = reader.ReadVector3();
        normal = reader.ReadVector3();
        p0 = reader.ReadVector3();
        p1 = reader.ReadVector3();
        p2 = reader.ReadVector3();
        p3 = reader.ReadVector3();
        end = reader.ReadBoolean();
    }
    public void OnReceived()
    {
        PipelineBuilder pipelineBuilder = networkInstanceId.GetPipelineBuilder();
        if (!pipelineBuilder) return;
        pipelineBuilder.PlaceNode(position, normal, p0, p1, p2, p3, end);
        pipelineBuilder.RpcPlaceNode(position, normal, p0, p1, p2, p3, end);
    }
    public void Serialize(NetworkWriter writer)
    {
        writer.Write(networkInstanceId);
        writer.Write(position);
        writer.Write(normal);
        writer.Write(p0);
        writer.Write(p1);
        writer.Write(p2);
        writer.Write(p3);
        writer.Write(end);
    }
    public static void SendToServer(NetworkInstanceId networkInstanceId, Vector3 position, Vector3 normal, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, bool end) => new PipelineBuilderPlaceNodeNetMessage
    {
        networkInstanceId = networkInstanceId,
        position = position,
        normal = normal,
        p0 = p0,
        p1 = p1,
        p2 = p2,
        p3 = p3,
        end = end
    }.Send(R2API.Networking.NetworkDestination.Server);
}
