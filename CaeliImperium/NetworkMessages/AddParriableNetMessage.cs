using EntityStates;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages;
public class AddParriableNetMessage : INetMessage
{
    public NetworkInstanceId networkInstanceId;
    public int esmIndex;
    public void Deserialize(NetworkReader reader)
    {
        throw new NotImplementedException();
    }

    public void OnReceived()
    {
        CharacterBody characterBody = networkInstanceId.GetCharacterBody();
        if (!characterBody || characterBody.hasEffectiveAuthority) return;
        NetworkStateMachine networkStateMachine = characterBody.GetComponent<NetworkStateMachine>();
        if (!networkStateMachine) return;
        EntityStateMachine entityStateMachine = HG.ArrayUtils.GetSafe<EntityStateMachine>(networkStateMachine.stateMachines, esmIndex);
    }

    public void Serialize(NetworkWriter writer)
    {
        throw new NotImplementedException();
    }
}
