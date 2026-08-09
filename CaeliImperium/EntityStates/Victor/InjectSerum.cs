using CaeliImperium.Bodies;
using CaeliImperium.ScriptableObjects;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates.Victor
{
    public class InjectSerum : BaseSkillState
    {
        public static float baseDuration = 0.3f;
        public const float audioDuration = 0.3f;
        public static float minBuffDuration = 10f;
        public static float maxBuffDuration = 100f;
        public float duration;
        private bool injected;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Util.PlayAttackSpeedSound("Play_Deadlock_Victor_Jumpstart", gameObject, attackSpeedStat / (baseDuration / audioDuration));
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge < duration) return;
            if (!injected)
            {
                injected = true;
                float duration = 500f;
                float maxCharge = 1000f;
                if (activatorSkillSlot)
                {
                    VictorSkillDef.InstanceData instanceData = activatorSkillSlot.skillInstanceData != null ? activatorSkillSlot.skillInstanceData as VictorSkillDef.InstanceData : null;
                    if (instanceData != null)
                    {
                        duration = instanceData.charge;
                        instanceData.charge = 0f;
                        maxCharge = instanceData.maxCharge;
                    }
                }
                if (NetworkServer.active)
                {
                    characterBody.AddTimedBuff(VictorEvents.Serum.buffIndex, Mathf.Lerp(minBuffDuration, maxBuffDuration, duration / maxCharge));
                }
                if (healthComponent)
                {
                    healthComponent.lastHealTime = Run.FixedTimeStamp.now;
                }
            }
            if (!isAuthority) return;
            outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
