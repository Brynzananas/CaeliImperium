using CaeliImperium.Configs;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.BomberWisp
{
    public class SpawnState : BaseSkillState
    {
        public static float baseDuration => BomberWispConfigs.SpawnStateDuration.Value;
        public float duration;
        private ChildLocator childLocator;
        private Transform fireTransform;
        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayAnimation("Body", "Spawn", "Spawn.playbackRate", SpawnState.baseDuration);
            duration = baseDuration;
            childLocator = GetModelChildLocator();
            if (childLocator)
            {
                fireTransform = childLocator.FindChild("Fire");
                if (fireTransform) fireTransform.gameObject.SetActive(false);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (fireTransform) fireTransform.gameObject.SetActive(true);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
