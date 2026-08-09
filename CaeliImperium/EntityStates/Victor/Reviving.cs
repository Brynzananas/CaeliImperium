using CaeliImperium.Bodies;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates.Victor
{
    public class Reviving : BaseCaeliImperiumState
    {
        public static float timeToRevive = 3f;
        public const float audioTime = 3f;
        private uint soundId;
        public override void OnEnter()
        {
            base.OnEnter();
            soundId = Util.PlaySound("Play_Deadlock_Victor_Reanimation_Start", gameObject);
            float num2 = Util.CalculateAttackSpeedRtpcValue((timeToRevive / audioTime));
            AkSoundEngine.SetRTPCValueByPlayingID("attackSpeed", num2, soundId);
            if (!NetworkServer.active) return;
            //characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            CleanseSystem.CleanseBodyServer(characterBody, true, false, true, true, true, false);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!NetworkServer.active) return;
            if (fixedAge >= timeToRevive) outer.SetNextStateToMain();
            if (!healthComponent) return;
            float coefficient = fixedAge / timeToRevive;
            healthComponent.Networkhealth = Mathf.Max(characterBody.maxHealth * coefficient, 1f);
            healthComponent.Networkshield = characterBody.maxShield * coefficient;
        }
        public override void OnExit()
        {
            base.OnExit();
            if (!NetworkServer.active) return;
            //characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            if (healthComponent)
            {
                healthComponent.Networkhealth = characterBody.maxHealth;
                healthComponent.Networkshield = characterBody.maxShield;
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Death;
    }
}
