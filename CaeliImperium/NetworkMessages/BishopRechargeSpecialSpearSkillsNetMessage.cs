using CaeliImperium.ScriptableObjects;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages;
public class BishopRechargeSpecialSpearSkillsNetMessage : INetMessage
{
    public int rechargeStocks;
    public NetworkInstanceId networkInstanceId;
    public void Deserialize(NetworkReader reader)
    {
        rechargeStocks = reader.ReadInt32();
        networkInstanceId = reader.ReadNetworkId();
    }
    public void OnReceived()
    {
        CharacterBody characterBody = networkInstanceId.GetCharacterBody();
        if (!characterBody || characterBody.hasEffectiveAuthority) return;
        if (rechargeStocks <= 0)
        {
            BishopSpecialSpearSkillDef.RechargeStocksForSpecialSpearSkills(characterBody);
        }
        else
        {
            BishopSpecialSpearSkillDef.RechargeStocksForSpecialSpearSkills(characterBody, rechargeStocks);
        }
    }
    public void Serialize(NetworkWriter writer)
    {
        writer.Write(rechargeStocks);
        writer.Write(networkInstanceId);
    }
    public static void SendToClients(int rechargeStocks, NetworkInstanceId networkInstanceId) => new BishopRechargeSpecialSpearSkillsNetMessage
    {
        rechargeStocks = rechargeStocks,
        networkInstanceId = networkInstanceId
    }.Send(R2API.Networking.NetworkDestination.Clients);
}
