using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CaeliImperium.ItemBehaviours
{
    public class DuplicateMainSkillsBehaviour : CharacterBody.ItemBehavior
    {
        public static float chancePerStack = 10f;
        public List<GenericSkill> genericSkills = new List<GenericSkill>();
        public List<Action<GenericSkill>> onSkillActivatedEvents = new List<Action<GenericSkill>>();
        public SkillLocator skillLocator;
        public void OnEnable()
        {
            skillLocator = GetComponent<SkillLocator>();
            if (skillLocator == null) return;
            foreach (var skill in skillLocator.allSkills)
            {
                if (skill == null) continue;
                CharacterBody body = skill.characterBody;
                if (body == null) continue;
                GenericSkill genericSkill = Utils.CopyGenericSkill(skill, body, "DragonStyle");
                genericSkills.Add(genericSkill);
                body.onSkillActivatedServer += Body_onSkillActivatedServer;
                onSkillActivatedEvents.Add(Body_onSkillActivatedServer);
                void Body_onSkillActivatedServer(GenericSkill obj)
                {
                    if (obj == skill)
                    {
                        //int itemCount = body && body.inventory ? body.inventory.GetItemCount(DuplicateMainSkills) : 0;
                        float chance = stack * chancePerStack;//Utils.ConvertAmplificationPercentageIntoReductionPercentage(itemCount * 10, 100);
                        if (Util.CheckRoll(chance))
                            genericSkill.Invoke("OnExecute", UnityEngine.Random.Range(0.2f, 0.3f));
                    }
                }
            }
        }

        public void OnDisable()
        {
            for (int i = 0; i < onSkillActivatedEvents.Count; i++)
            {
                Action<GenericSkill> skill = onSkillActivatedEvents[i];
                body.onSkillActivatedServer -= skill;
            }
            for (int i = 0; i < genericSkills.Count; i++)
            {
                GenericSkill skill = genericSkills[i];
                if (skill != null)
                {
                    //if (skill.characterBody.skillLocator != null) skill.characterBody.skillLocator.RemoveBonusSkill(skill);
                    Destroy(skill.stateMachine);
                    Destroy(skill);
                }
            }
            genericSkills.Clear();
        }
    }
}
