using CaeliImperium.Bodies;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Networking;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages;
public class AddTimeBuffAndResetTimerForAllStacksNetMessage : INetMessage
{
    private NetworkInstanceId networkInstanceId;
    private BuffIndex buffIndex;
    private float duration;
    public void Deserialize(NetworkReader reader)
    {
        networkInstanceId = reader.ReadNetworkId();
        buffIndex = (BuffIndex)reader.ReadInt32();
        duration = reader.ReadSingle();
    }
    public void OnReceived()
    {
        CharacterBody characterBody = networkInstanceId.GetCharacterBody();
        if (!characterBody) return;
        Send(characterBody, buffIndex, duration);
    }
    public void Serialize(NetworkWriter writer)
    {
        writer.Write(networkInstanceId);
        writer.Write((int)buffIndex);
        writer.Write(duration);
    }
    public static void Send(CharacterBody characterBody, BuffIndex buffIndex, float duration)
    {
        if (!NetworkServer.active)
        {
            new AddTimeBuffAndResetTimerForAllStacksNetMessage
            {
                networkInstanceId = characterBody.netId,
                buffIndex = buffIndex,
                duration = duration
            }.Send(R2API.Networking.NetworkDestination.Server);
            return;
        }
        foreach (CharacterBody.TimedBuff timedBuff in characterBody.timedBuffs)
        {
            if (timedBuff.buffIndex == buffIndex) timedBuff.timer = timedBuff.totalDuration;
        }
        characterBody.AddTimedBuff(buffIndex, duration);
    }
}
