using BrynzaAPI;
using CaeliImperium.Bodies;
using CaeliImperium.Components.Doomguy;
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
public class DoomguySkillDef : SkillDef
{
    public bool cannotUseWhileSpearOrbiting;
    public bool rechargeOnlyOnGround;
    public bool restockOnParry;
    public float rechargePercentageOnMeleeParry;
    public DoomguyWeaponController rightHandWeapon;
    public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
    {
        BishopSkillInstanceData bishopSkillInstanceData = new BishopSkillInstanceData { genericSkill = skillSlot, restockOnParry = restockOnParry, rechargePercentageOnParry = rechargePercentageOnMeleeParry };
        DoomguyEvents.onParry += bishopSkillInstanceData.OnParry;
        return bishopSkillInstanceData;
    }
    public override void OnUnassigned([NotNull] GenericSkill skillSlot)
    {
        if (skillSlot.skillInstanceData != null && skillSlot.skillInstanceData is BishopSkillInstanceData bishopSkillInstanceData) DoomguyEvents.onParry -= bishopSkillInstanceData.OnParry;
        base.OnUnassigned(skillSlot);
    }
    public override bool CanExecute([NotNull] GenericSkill skillSlot)
    {
        bool flag = base.CanExecute(skillSlot);
        if (cannotUseWhileSpearOrbiting && skillSlot.characterBody && skillSlot.characterBody.GetClientBuffCount(RoR2Content.Buffs.Nullified) > 0) return false;
        return flag;
    }
    public override float GetRechargeRate([NotNull] GenericSkill skillSlot)
    {
        CharacterBody characterBody = skillSlot.characterBody;
        if (!characterBody) return base.GetRechargeRate(skillSlot);
        if (!skillSlot.skillDef || skillSlot.skillDef is not DoomguySkillDef bishopSkillDef || !bishopSkillDef.rechargeOnlyOnGround) return base.GetRechargeRate(skillSlot);
        CharacterMotor characterMotor = characterBody.characterMotor;
        if (!characterMotor || characterMotor.isGrounded) return base.GetRechargeRate(skillSlot);
        return 0f;
    }
    public class BishopSkillInstanceData : BaseSkillInstanceData
    {
        public GenericSkill genericSkill;
        public bool restockOnParry;
        public float rechargePercentageOnParry;
        public void OnParry(DoomguyEvents.OnParryInfo onParryInfo)
        {
            if (!genericSkill) return;
            if (restockOnParry) genericSkill.stock = genericSkill.maxStock;
            if (rechargePercentageOnParry > 0f && onParryInfo.damageInfo.damageType.HasModdedDamageType(DoomguyEvents.ParriableMeleeDamageType)) genericSkill.RunRecharge(genericSkill.finalRechargeInterval * rechargePercentageOnParry);
        }
    }
}
