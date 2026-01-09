using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaeliImperium.ItemBehaviours
{
    public class CopyNearbyCharactersSkillsOnDeathBehaviour : CharacterBody.ItemBehavior
    {
        public Dictionary<int, List<GenericSkill>> keyValuePairs = new Dictionary<int, List<GenericSkill>>();
        public List<GenericSkill> skillList = new List<GenericSkill>();
        public void OnEnable()
        {
            if (!keyValuePairs.ContainsKey(0))
                keyValuePairs.Add(0, new List<GenericSkill>());
            if (!keyValuePairs.ContainsKey(1))
                keyValuePairs.Add(1, new List<GenericSkill>());
            if (!keyValuePairs.ContainsKey(2))
                keyValuePairs.Add(2, new List<GenericSkill>());
            if (!keyValuePairs.ContainsKey(3))
                keyValuePairs.Add(3, new List<GenericSkill>());
            if (!keyValuePairs.ContainsKey(4))
                keyValuePairs.Add(4, new List<GenericSkill>());
        }
        public void FixedUpdate()
        {
            if (!body || !body.hasEffectiveAuthority) return;
            //if (keyValuePairs[0] != null) foreach (GenericSkill skill in keyValuePairs[0]) skill.ExecuteIfReady();
            if ((!body.inputBank || body.inputBank.skill1.down) && keyValuePairs[1] != null) foreach (GenericSkill skill in keyValuePairs[1]) if (skill) skill.ExecuteIfReady();
            if ((!body.inputBank || body.inputBank.skill2.down) && keyValuePairs[2] != null) foreach (GenericSkill skill in keyValuePairs[2]) if (skill) skill.ExecuteIfReady();
            if ((!body.inputBank || body.inputBank.skill3.down) && keyValuePairs[3] != null) foreach (GenericSkill skill in keyValuePairs[3]) if (skill) skill.ExecuteIfReady();
            if ((!body.inputBank || body.inputBank.skill4.down) && keyValuePairs[4] != null) foreach (GenericSkill skill in keyValuePairs[4]) if (skill) skill.ExecuteIfReady();
        }
    }
}
