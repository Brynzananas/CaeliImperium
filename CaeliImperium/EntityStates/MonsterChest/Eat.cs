using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.MonsterChest
{
    public class Eat : MonsterChestBaseState
    {
        public static float duration = 1f;
        private static int EatStateHash = Animator.StringToHash("Eat");
        private static int EatParamHash = Animator.StringToHash("Eat.playbackRate");
        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Base", EatStateHash, EatParamHash, duration);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > duration)
            {
                this.outer.SetNextState(new Idle());
            }
        }
    }
}
