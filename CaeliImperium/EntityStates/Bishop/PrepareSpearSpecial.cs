using CaeliImperium;
using CaeliImperium.Bodies;
using EntityStates;
using RoR2;
using RoR2.HudOverlay;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates.Bishop;
public class PrepareSpearSpecial : BishopState
{
    public Animator animator;
    public OverlayController overlayController;
    public override void OnEnter()
    {
        base.OnEnter();
        Util.PlaySound("Play_DoomTDA_Spear_Throw_Enter", gameObject);
        animator = (this as IBishopState).GetFirstPersonAnimator();
        if (animator) animator.AddInteger("spearPrepare");
        OverlayCreationParams overlayCreationParams = new OverlayCreationParams
        {
            prefab = BishopEvents.SpearCrosshair,
            childLocatorEntry = "CrosshairExtras"
        };
        overlayController = HudOverlayManager.AddOverlay(gameObject, overlayCreationParams);
        /*overlayController.onInstanceAdded += this.OnOverlayInstanceAdded;
        overlayController.onInstanceRemove += this.OnOverlayInstanceRemoved;*/
    }
    /*private void OnOverlayInstanceRemoved(OverlayController controller, GameObject @object)
    {
    }

    private void OnOverlayInstanceAdded(OverlayController controller, GameObject @object)
    {
    }*/
    public override void OnExit()
    {
        base.OnExit();
        Util.PlaySound("Play_DoomTDA_Spear_Throw_Exit", gameObject);
        if (animator) animator.SubstractInteger("spearPrepare");
        if (overlayController != null)
        {
            HudOverlayManager.RemoveOverlay(overlayController);
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        StartAimMode();
        if (!isAuthority) return;
        if (!inputBank)
        {
            outer.SetNextStateToMain();
            return;
        }
        if (!activatorSkillSlot || !activatorSkillSlot.skillDef || activatorSkillSlot.skillDef.HasRequiredStockAndDelay(activatorSkillSlot))
        {
            if (inputBank.skill1.justPressed)
            {
                DeductStock();
                outer.SetNextState(new SpearThrow { activatorSkillSlot = activatorSkillSlot });
            }
            if (inputBank.skill3.justPressed)
            {
                DeductStock();
                outer.SetNextState(new SpearSlam { activatorSkillSlot = activatorSkillSlot });
            }
            if (inputBank.skill2.justPressed)
            {
                DeductStock();
                outer.SetNextState(new SpearOrbitThrow { activatorSkillSlot = activatorSkillSlot });
            }
        }
        if (!IsKeyDownAuthority()) outer.SetNextStateToMain();
    }
    private void DeductStock()
    {
        if (activatorSkillSlot) activatorSkillSlot.DeductStock(activatorSkillSlot.skillDef.requiredStock);
    }
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
}
