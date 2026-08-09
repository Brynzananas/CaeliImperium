using CaeliImperium.NetworkMessages;
using JetBrains.Annotations;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ScriptableObjects
{
    public class VictorSkillDef : SkillDef
    {
        public float damageToCharge;
        public float healToCharge;
        public float maxCharge = 1000f;
        public override BaseSkillInstanceData OnAssigned(GenericSkill skillSlot)
        {
            InstanceData instanceData = new InstanceData();
            instanceData.OnAssigned(skillSlot);
            instanceData.healToCooldownReduction = healToCharge;
            instanceData.damageToCooldownReduction = damageToCharge;
            instanceData.genericSkill = skillSlot;
            instanceData.maxCharge = maxCharge;
            return instanceData;
        }
        public override void OnUnassigned([NotNull] GenericSkill skillSlot)
        {
            InstanceData instanceData = skillSlot.skillInstanceData != null ? skillSlot.skillInstanceData as InstanceData : null;
            if (instanceData != null) instanceData.OnUnassigned(skillSlot);
            base.OnUnassigned(skillSlot);
        }
        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            InstanceData instanceData = skillSlot.skillInstanceData != null ? skillSlot.skillInstanceData as InstanceData : null;
            return (instanceData != null ? instanceData.charge > 0f : true) && base.CanExecute(skillSlot);
        }
        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            InstanceData instanceData = skillSlot.skillInstanceData != null ? skillSlot.skillInstanceData as InstanceData : null;
            return (instanceData != null ? instanceData.charge > 0f : true) && base.IsReady(skillSlot);
        }
        public class InstanceData : BaseSkillInstanceData
        {
            public static List<InstanceData> instances = [];
            public int id;
            public float damageToCooldownReduction;
            public float healToCooldownReduction;
            public float charge;
            public float maxCharge;
            public GenericSkill genericSkill;
            public void OnAssigned(GenericSkill skillSlot)
            {
                HealthComponent.onCharacterHealServer += HealthComponent_onCharacterHealServer;
                GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
                id = instances.Count;
                instances.Add(this);
            }
            public void OnUnassigned(GenericSkill skillSlot)
            {
                HealthComponent.onCharacterHealServer -= HealthComponent_onCharacterHealServer;
                GlobalEventManager.onServerDamageDealt -= GlobalEventManager_onServerDamageDealt;
                instances.Remove(this);
            }
            private void GlobalEventManager_onServerDamageDealt(DamageReport obj)
            {
                if (!obj.victim || obj.victim.gameObject != genericSkill.gameObject) return;
                float charge = damageToCooldownReduction * obj.damageDealt;
                if (this.charge + charge > maxCharge) charge = maxCharge - this.charge;
                new VictorChargeMessage(charge, id).Send(R2API.Networking.NetworkDestination.Clients);
            }
            private void HealthComponent_onCharacterHealServer(HealthComponent arg1, float arg2, ProcChainMask arg3)
            {
                if (!arg1 || arg1.gameObject != genericSkill.gameObject) return;
                float charge = arg2 * healToCooldownReduction;
                if (this.charge + charge > maxCharge) charge = maxCharge - this.charge;
                new VictorChargeMessage(charge, id).Send(R2API.Networking.NetworkDestination.Clients);
            }
        }
    }
}
