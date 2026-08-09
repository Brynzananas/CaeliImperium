using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace CaeliImperium
{
    public class VictorBlastAttack : BlastAttack, R2API.ICustomDamageInfo<VictorDamageInfo>
    {
        public float reduceReviveCooldown;
        public void ModifyDamageInfo(VictorDamageInfo damageInfo)
        {
            damageInfo.reduceReviveCooldown = reduceReviveCooldown;
        }

        public void Read(VictorDamageInfo damageInfo, NetworkReader networkReader)
        {
            damageInfo.reduceReviveCooldown = networkReader.ReadSingle();
        }

        public void Write(VictorDamageInfo damageInfo, NetworkWriter networkWriter)
        {
            networkWriter.Write(damageInfo.reduceReviveCooldown);
        }
    }
}
