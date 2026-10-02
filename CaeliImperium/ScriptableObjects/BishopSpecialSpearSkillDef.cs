using BrynzaAPI;
using JetBrains.Annotations;
using RoR2;
using RoR2.Skills;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ScriptableObjects;
public class BishopSpecialSpearSkillDef : SkillDef
{
    public static FixedConditionalWeakTable<CharacterBody, HashSet<GenericSkill>> characterBodyToSkillsWithSpecialSpear = [];
    public int rechargeStocksOnParry;
    public override bool CanExecute([NotNull] GenericSkill skillSlot)
    {
        bool flag = base.CanExecute(skillSlot);
        if (skillSlot.characterBody && skillSlot.characterBody.GetClientBuffCount(RoR2Content.Buffs.Nullified) > 0) return false;
        return flag;
    }
    public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
    {
        BaseSkillInstanceData baseSkillInstanceData = base.OnAssigned(skillSlot);
        CharacterBody characterBody = skillSlot.characterBody;
        if (characterBody)
        {
            if (characterBodyToSkillsWithSpecialSpear.TryGetValue(characterBody, out HashSet<GenericSkill> genericSkills))
            {
                if (!genericSkills.Contains(skillSlot)) genericSkills.Add(skillSlot);
            }
            else
            {
                characterBodyToSkillsWithSpecialSpear.Add(characterBody, [skillSlot]);
            }
        }
        return baseSkillInstanceData;
    }
    public override void OnUnassigned([NotNull] GenericSkill skillSlot)
    {
        base.OnUnassigned(skillSlot);
        CharacterBody characterBody = skillSlot.characterBody;
        if (characterBody) if (characterBodyToSkillsWithSpecialSpear.TryGetValue(characterBody, out HashSet<GenericSkill> genericSkills)) if (genericSkills.Contains(skillSlot)) genericSkills.Remove(skillSlot);
    }
    public static HashSet<GenericSkill> GetGenericSkills(CharacterBody characterBody) => characterBodyToSkillsWithSpecialSpear.TryGetValue(characterBody, out HashSet<GenericSkill> genericSkills) ? genericSkills : null;
    public static void RechargeStocksForSpecialSpearSkills(CharacterBody characterBody)
    {
        HashSet<GenericSkill> genericSkills = GetGenericSkills(characterBody);
        if (genericSkills == null) return;
        foreach (GenericSkill skill in genericSkills)
        {
            if (!skill) continue;
            SkillDef skillDef = skill.skillDef;
            if (!skillDef || skillDef is not BishopSpecialSpearSkillDef bishopSpecialSpearSkill) continue;
            skill.stock = Mathf.Min(skill.stock + bishopSpecialSpearSkill.rechargeStocksOnParry, skill.maxStock);
        }
    }
    public static void RechargeStocksForSpecialSpearSkills(CharacterBody characterBody, int amount)
    {
        HashSet<GenericSkill> genericSkills = GetGenericSkills(characterBody);
        if (genericSkills == null) return;
        foreach (GenericSkill skill in genericSkills)
        {
            if (!skill) continue;
            SkillDef skillDef = skill.skillDef;
            if (!skillDef) continue;
            skill.stock = Mathf.Min(skill.stock + amount, skill.maxStock);
        }
    }
}
