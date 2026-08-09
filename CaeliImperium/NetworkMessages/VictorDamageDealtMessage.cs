using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages
{
    public class VictorDamageDealtMessage : INetMessage
    {
        public NetworkIdentity networkIdentity;
        public float cooldownReduction;
        public int genericSkillId;
        public VictorDamageDealtMessage()
        {

        }
        public VictorDamageDealtMessage(NetworkIdentity networkIdentity, float cooldownReduction, int genericSkillId)
        {
            this.networkIdentity = networkIdentity;
            this.cooldownReduction = cooldownReduction;
            this.genericSkillId = genericSkillId;
        }
        public void Deserialize(NetworkReader reader)
        {
            networkIdentity = reader.ReadNetworkIdentity();
            cooldownReduction = reader.ReadSingle();
            genericSkillId = reader.ReadInt32();
        }

        public void OnReceived()
        {
            if (!networkIdentity) return;
            SkillLocator skillLocator = networkIdentity.GetComponent<SkillLocator>();
            if (!skillLocator) return;
            GenericSkill genericSkill = skillLocator.GetSkillAtIndex(genericSkillId);
            if (!genericSkill) return;
            genericSkill.RunRecharge(cooldownReduction);
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(networkIdentity);
            writer.Write(cooldownReduction);
            writer.Write(genericSkillId);
        }
    }
}
