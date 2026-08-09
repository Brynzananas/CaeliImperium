using CaeliImperium.ScriptableObjects;
using R2API.Networking.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages
{
    public class VictorChargeMessage : INetMessage
    {
        public float charge;
        public int id;
        public VictorChargeMessage()
        {

        }
        public VictorChargeMessage(float charge, int id)
        {
            this.charge = charge;
            this.id = id;
        }
        public void Deserialize(NetworkReader reader)
        {
            charge = reader.ReadSingle();
            id = reader.ReadInt32();
        }
        public void OnReceived()
        {
            VictorSkillDef.InstanceData instanceData = VictorSkillDef.InstanceData.instances[id];
            instanceData.charge += charge;
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(charge);
            writer.Write(id);
        }
    }
}
