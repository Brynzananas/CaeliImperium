using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static CaeliImperium.Components.ExtraInputGenericSkill;

namespace CaeliImperium.Components
{
    public class ExtraInputGenericSkill : GenericSkill
    {
        public List<ExtraGenericSkill> extraGenericSkills = [];
        public bool HandleExtraSkills(BaseSkillState baseSkillState)
        {
            InputBankTest inputBankTest = baseSkillState.inputBank;
            if (!inputBankTest) return false;
            ref InputBankTest.ButtonState primaryButtonState = ref inputBankTest.skill1;
            ref InputBankTest.ButtonState secondaryButtonState = ref inputBankTest.skill2;
            ref InputBankTest.ButtonState utilityButtonState = ref inputBankTest.skill3;
            ref InputBankTest.ButtonState specialButtonState = ref inputBankTest.skill4;
            bool keyPressed = false;
            foreach (ExtraGenericSkill extraGenericSkill in extraGenericSkills)
            {
                if (!extraGenericSkill.genericSkill) continue;
                GenericSkill genericSkill = baseSkillState.activatorSkillSlot;
                if (!genericSkill) continue;
                if (extraGenericSkill.inputSource == DamageSource.Primary) keyPressed = HandleSkill(extraGenericSkill.genericSkill, ref primaryButtonState);
                if (extraGenericSkill.inputSource == DamageSource.Secondary) keyPressed = HandleSkill(extraGenericSkill.genericSkill, ref secondaryButtonState);
                if (extraGenericSkill.inputSource == DamageSource.Utility) keyPressed = HandleSkill(extraGenericSkill.genericSkill, ref utilityButtonState);
                if (extraGenericSkill.inputSource == DamageSource.Special) keyPressed = HandleSkill(extraGenericSkill.genericSkill, ref specialButtonState);
            }
            return keyPressed;
        }
        private bool HandleSkill(GenericSkill skillSlot, ref InputBankTest.ButtonState buttonState)
        {
            if (!buttonState.down || !skillSlot) return false;
            if (skillSlot.mustKeyPress && buttonState.hasPressBeenClaimed) return false;
            PerformSkill(skillSlot, ref buttonState);
            return true;
        }
        protected virtual void PerformSkill(GenericSkill skillSlot, ref InputBankTest.ButtonState buttonState)
        {
            if (skillSlot.ExecuteIfReady())
            {
                buttonState.hasPressBeenClaimed = true;
            }
        }
        [Serializable]
        public struct ExtraGenericSkill
        {
            public GenericSkill genericSkill;
            public DamageSource inputSource;
        }
    }
}
