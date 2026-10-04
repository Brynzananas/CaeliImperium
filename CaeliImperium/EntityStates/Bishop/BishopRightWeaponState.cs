using CaeliImperiumEntityStates.Bishop;
using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.Bishop;
public abstract class BishopRightWeaponState : BishopState
{
    public abstract float durationForStateModification { get; }
    public abstract float fixedAgeForStateModification { get; }
    public override void ModifyNextState(EntityState nextState)
    {
        base.ModifyNextState(nextState);
        if (nextState is not BishopMeleeState bishopMeleeState) return;
        bishopMeleeState.addDuration = Mathf.Max(durationForStateModification - fixedAgeForStateModification, 0f);
    }
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
}
