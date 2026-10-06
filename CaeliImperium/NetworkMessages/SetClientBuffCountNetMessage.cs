using BrynzaAPI;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Networking;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages;
public class SetClientBuffCountNetMessage : INetMessage
{
    public NetworkInstanceId networkInstanceId;
    public BuffIndex buffIndex;
    public int count;
    public void Deserialize(NetworkReader reader)
    {
        networkInstanceId = reader.ReadNetworkId();
        buffIndex = reader.ReadBuffIndex();
        count = reader.ReadInt32();
    }
    public void OnReceived()
    {
       CharacterBody characterBody = networkInstanceId.GetCharacterBody();
       if (!characterBody || characterBody.hasAuthority) return;
       characterBody.SetClientBuffCount(buffIndex, count);
    }
    public void Serialize(NetworkWriter writer)
    {
        writer.Write(networkInstanceId);
        writer.WriteBuffIndex(buffIndex);
        writer.Write(count);
    }
    public static void SendToClients(NetworkInstanceId networkInstanceId, BuffIndex buffIndex, int count) => new SetClientBuffCountNetMessage
    {
        networkInstanceId = networkInstanceId,
        buffIndex = buffIndex,
        count = count
    }.Send(R2API.Networking.NetworkDestination.Clients);
}
