using BrynzaAPI;
using CaeliImperium;
using CaeliImperium.Bodies;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.BomberWisp
{
    public class ChargeBomb : BaseSkillState
    {
        public static float baseDuration = 2f;
        public static float effectScale = 1f;
        public float duration;
        private Animator animator;
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_Silksong_Wisp_FlyLoop", gameObject);
            duration = baseDuration / attackSpeedStat;
            animator = GetModelAnimator();
            if (animator)
            {
                animator.AddInteger("chargeCount");
            }
            Transform eye = FindModelChild("Eye");
            if (eye)
            {
                EffectData effectData = new EffectData
                {
                    rootObject = eye.gameObject,
                    rotation = eye.rotation,
                    scale = effectScale,
                    genericFloat = duration
                };
                EffectManager.SpawnEffect(BomberWisp2Events.Charging.index, effectData, false);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            Util.PlaySound("Stop_Silksong_Wisp_FlyLoop", gameObject);
            if (animator)
            {
                animator.SubstractInteger("chargeCount");
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            if (fixedAge >= duration) outer.SetNextState(new FireBomb { activatorSkillSlot = activatorSkillSlot });
        }
    }
}
