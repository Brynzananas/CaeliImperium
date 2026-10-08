using CaeliImperium.Bodies;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages;
public class BishopOnParryNetMessage : INetMessage
{
    public NetworkInstanceId networkInstanceId;
    public DamageInfo damageInfo;
    public void Deserialize(NetworkReader reader)
    {
        networkInstanceId = reader.ReadNetworkId();
        damageInfo = reader.ReadDamageInfo();
    }

    public void OnReceived()
    {
        CharacterBody characterBody = networkInstanceId.GetCharacterBody();
        if (!characterBody || characterBody.hasEffectiveAuthority) return;
        BishopEvents.OnParry(characterBody, damageInfo);
    }

    public void Serialize(NetworkWriter writer)
    {
        writer.Write(networkInstanceId);
        writer.Write(damageInfo);
    }
    public static void SendToClients(NetworkInstanceId networkInstanceId, DamageInfo damageInfo) => new BishopOnParryNetMessage
    {
        networkInstanceId = networkInstanceId,
        damageInfo = damageInfo
    }.Send(R2API.Networking.NetworkDestination.Clients);
}
