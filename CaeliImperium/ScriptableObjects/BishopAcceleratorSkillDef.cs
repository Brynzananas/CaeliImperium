using CaeliImperium.ScriptableObjects;
using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumScriptableObjects;
public class BishopAcceleratorSkillDef : BishopSkillDef
{
    public float maxCharge = 100f;
    public float chargeCoefficient = 1f;
    public float chargeLossPerSecond = 1f;
    public float chargeGraceDuration = 2f;
    public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
    {
        return new AcceleratorSkillInstanceData { chargeCoefficient = chargeCoefficient, maxCharge = maxCharge };
    }
    public override void OnFixedUpdate([NotNull] GenericSkill skillSlot, float deltaTime)
    {
        base.OnFixedUpdate(skillSlot, deltaTime);
        if (skillSlot.skillInstanceData == null || skillSlot.skillInstanceData is not AcceleratorSkillInstanceData acceleratorSkillInstanceData) return;
        if (acceleratorSkillInstanceData.chargeLossTimer >= chargeGraceDuration)
        {
            if (acceleratorSkillInstanceData.charge > 0f)
            {
                acceleratorSkillInstanceData.charge -= chargeLossPerSecond * Time.fixedDeltaTime;
                if (acceleratorSkillInstanceData.charge < 0f) acceleratorSkillInstanceData.charge = 0f;
            }
        }
        else
        {
            acceleratorSkillInstanceData.chargeLossTimer += Time.fixedDeltaTime;
        }
    }
    public class AcceleratorSkillInstanceData : BaseSkillInstanceData
    {
        public float maxCharge;
        public float chargeCoefficient;
        public float charge;
        public float chargeLossTimer;
    }
    public static float GetTotalCharge(SkillLocator skillLocator)
    {
        float charge = 0f;
        if (skillLocator.allSkills == null) return charge;
        foreach (GenericSkill genericSkill in skillLocator.allSkills)
        {
            if (!genericSkill) continue;
            if (genericSkill.skillInstanceData == null || genericSkill.skillInstanceData is not AcceleratorSkillInstanceData acceleratorSkillInstanceData) continue;
            charge += acceleratorSkillInstanceData.charge;
        }
        return charge;
    }
    public static void AddCharge(GenericSkill genericSkill, float charge)
    {
        if (genericSkill.skillInstanceData == null || genericSkill.skillInstanceData is not AcceleratorSkillInstanceData acceleratorSkillInstanceData) return;
        acceleratorSkillInstanceData.charge += charge * acceleratorSkillInstanceData.chargeCoefficient;
        acceleratorSkillInstanceData.charge = Mathf.Clamp(acceleratorSkillInstanceData.charge, 0f, acceleratorSkillInstanceData.maxCharge);
        acceleratorSkillInstanceData.chargeLossTimer = 0f;
    }
    public static void RemoveAllCharge(GenericSkill genericSkill)
    {
        if (genericSkill.skillInstanceData == null || genericSkill.skillInstanceData is not AcceleratorSkillInstanceData acceleratorSkillInstanceData) return;
        acceleratorSkillInstanceData.charge = 0f;
    }
}
