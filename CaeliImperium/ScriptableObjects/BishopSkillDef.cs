using BrynzaAPI;
using CaeliImperium.Bodies;
using CaeliImperium.Components.Bishop;
using JetBrains.Annotations;
using R2API;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ScriptableObjects;
public class BishopSkillDef : SkillDef
{
    public bool cannotUseWhileSpearOrbiting;
    public bool rechargeOnlyOnGround;
    public bool restockOnParry;
    public float rechargePercentageOnMeleeParry;
    public BishopWeaponController rightHandWeapon;
    public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
    {
        BishopSkillInstanceData bishopSkillInstanceData = new BishopSkillInstanceData { genericSkill = skillSlot, restockOnParry = restockOnParry, rechargePercentageOnParry = rechargePercentageOnMeleeParry };
        BishopEvents.onParry += bishopSkillInstanceData.OnParry;
        return bishopSkillInstanceData;
    }
    public override void OnUnassigned([NotNull] GenericSkill skillSlot)
    {
        if (skillSlot.skillInstanceData != null && skillSlot.skillInstanceData is BishopSkillInstanceData bishopSkillInstanceData) BishopEvents.onParry -= bishopSkillInstanceData.OnParry;
        base.OnUnassigned(skillSlot);
    }
    public override bool CanExecute([NotNull] GenericSkill skillSlot)
    {
        bool flag = base.CanExecute(skillSlot);
        if (cannotUseWhileSpearOrbiting && skillSlot.characterBody && skillSlot.characterBody.GetClientBuffCount(RoR2Content.Buffs.Nullified) > 0) return false;
        return flag;
    }
    public class BishopSkillInstanceData : BaseSkillInstanceData
    {
        public GenericSkill genericSkill;
        public bool restockOnParry;
        public float rechargePercentageOnParry;
        public void OnParry(BishopEvents.OnParryInfo onParryInfo)
        {
            if (!genericSkill) return;
            if (restockOnParry) genericSkill.stock = genericSkill.maxStock;
            if (rechargePercentageOnParry > 0f && onParryInfo.damageInfo.damageType.HasModdedDamageType(BishopEvents.ParriableMeleeDamageType)) genericSkill.RunRecharge(genericSkill.finalRechargeInterval * rechargePercentageOnParry);
        }
    }
}
