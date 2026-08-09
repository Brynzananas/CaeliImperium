using CaeliImperium.Components;
using CaeliImperiumEntityStates;
using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaeliImperiumEntityStates.Victor
{
    public class AimSecondary : BaseCaeliImperiumState
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            if (activatorSkillSlot && activatorSkillSlot is ExtraInputGenericSkill extraInputGenericSkill) extraInputGenericSkill.HandleExtraSkills(this);
            if (IsKeyDownAuthority()) return;
            outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
