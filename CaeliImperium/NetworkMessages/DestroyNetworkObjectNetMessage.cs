using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages;
public class DestroyNetworkObjectNetMessage : INetMessage
{
    public NetworkInstanceId networkInstanceId;
    public void Deserialize(NetworkReader reader)
    {
        networkInstanceId = reader.ReadNetworkId();
    }
    public void OnReceived()
    {
        GameObject gameObject = Util.FindNetworkObject(networkInstanceId);
        if (!gameObject) return;
        GameObject.Destroy(gameObject);
    }
    public void Serialize(NetworkWriter writer)
    {
        writer.Write(networkInstanceId);
    }
    public static void SendToServer(NetworkInstanceId networkInstanceId) => new DestroyNetworkObjectNetMessage
    {
        networkInstanceId = networkInstanceId,
    }.Send(R2API.Networking.NetworkDestination.Server);
}
