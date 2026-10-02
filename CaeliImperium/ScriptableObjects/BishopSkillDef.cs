using BrynzaAPI;
using CaeliImperium.Components.Bishop;
using JetBrains.Annotations;
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
    public BishopWeaponController rightHandWeapon;
    public override bool CanExecute([NotNull] GenericSkill skillSlot)
    {
        bool flag = base.CanExecute(skillSlot);
        if (cannotUseWhileSpearOrbiting && skillSlot.characterBody && skillSlot.characterBody.GetClientBuffCount(RoR2Content.Buffs.Nullified) > 0) return false;
        return flag;
    }
}
