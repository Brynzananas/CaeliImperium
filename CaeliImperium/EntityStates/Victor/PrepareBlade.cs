using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.Victor
{
    public class PrepareBlade : BaseSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            if (!IsKeyDownAuthority())
            {
                activatorSkillSlot.DeductStock(1);
                outer.SetNextState(new SlashBlade { activatorSkillSlot = activatorSkillSlot});
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
