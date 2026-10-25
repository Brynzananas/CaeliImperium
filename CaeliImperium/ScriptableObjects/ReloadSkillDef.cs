using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ScriptableObjects;
public class ReloadSkillDef : SkillDef
{
    [Header("Reload")]
    [Tooltip("How long it takes for this skill to reload after all stocks being used?")]
    public float baseReloadInterval;
    public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
    {
        ReloadSkillInstanceData relaodSkillInstanceData = new ReloadSkillInstanceData
        {
            baseReloadInterval = baseReloadInterval,
            genericSkill = skillSlot
        };
        relaodSkillInstanceData.RecalculateFinalReloadInterval();
        return relaodSkillInstanceData;
    }
    public override bool CanExecute([NotNull] GenericSkill skillSlot)
    {
        bool flag = base.CanExecute(skillSlot);
        if (skillSlot.skillInstanceData == null || skillSlot.skillInstanceData is not ReloadSkillInstanceData reloadSkillInstanceData) return flag;
        if (reloadSkillInstanceData.reloadStopwatch > 0f)
        {
            return false;
        }
        else
        {
            return flag;
        }
    }
    public override void OnExecute([NotNull] GenericSkill skillSlot)
    {
        base.OnExecute(skillSlot);
        if (skillSlot.skillInstanceData == null || skillSlot.skillInstanceData is not ReloadSkillInstanceData reloadSkillInstanceData) return;
        reloadSkillInstanceData.reloadStopwatch = reloadSkillInstanceData.finalReloadInterval;
    }
    public override void OnFixedUpdate([NotNull] GenericSkill skillSlot, float deltaTime)
    {
        base.OnFixedUpdate(skillSlot, deltaTime);
        if (skillSlot.skillInstanceData == null || skillSlot.skillInstanceData is not ReloadSkillInstanceData reloadSkillInstanceData) return;
        if (reloadSkillInstanceData.reloadStopwatch <= 0f) return;
        if ((beginSkillCooldownOnSkillEnd && skillSlot.stateMachine && (skillSlot.stateMachine.state.GetType() == activationState.stateType)) || skillSlot.isCooldownBlocked) return;
        reloadSkillInstanceData.reloadStopwatch = Mathf.Max(reloadSkillInstanceData.reloadStopwatch - Time.fixedDeltaTime, 0f);
    }
    public override float GetRechargeRate([NotNull] GenericSkill skillSlot)
    {
        return skillSlot.stock == 0 ? base.GetRechargeRate(skillSlot) : 0f;
    }
    public class ReloadSkillInstanceData : BaseSkillInstanceData
    {
        public GenericSkill genericSkill;
        public float reloadStopwatch;
        public float baseReloadInterval;
        public float finalReloadInterval;
        public float CalculateFinalReloadInterval()
        {
            if (!genericSkill) return baseReloadInterval;
            return baseReloadInterval * genericSkill.cooldownScale;
        }
        public void RecalculateFinalReloadInterval() => finalReloadInterval = CalculateFinalReloadInterval();
    }
}
