using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaeliImperium.Configs
{
    public static class InfiniteSecondarySkillChargesConfigs
    {
        public static void Init()
        {
            InfiniteSecondarySkillChargesDamage = Utils.CreateConfig(CaeliImperiumContent.Items.InfiniteSecondarySkillCharges.configName, "Secondary damage increase", 1f, "");
            InfiniteSecondarySkillChargesDamagePerStack = Utils.CreateConfig(CaeliImperiumContent.Items.InfiniteSecondarySkillCharges.configName, "Secondary damage increase per stack", 1f, "");
            InfiniteSecondarySkillChargesDamagePerCharge = Utils.CreateConfig(CaeliImperiumContent.Items.InfiniteSecondarySkillCharges.configName, "Secondary damage increase per charge consumed", 1f, "");
            InfiniteSecondarySkillChargesDamage.SettingChanged += SettingChanged;
            InfiniteSecondarySkillChargesDamagePerStack.SettingChanged += SettingChanged;
            InfiniteSecondarySkillChargesDamagePerCharge.SettingChanged += SettingChanged;
        }
        private static void SettingChanged(object sender, System.EventArgs e) => Language.InitInfiniteSecondarySkillCharges();

        public static ConfigEntry<float> InfiniteSecondarySkillChargesDamage;
        public static ConfigEntry<float> InfiniteSecondarySkillChargesDamagePerStack;
        public static ConfigEntry<float> InfiniteSecondarySkillChargesDamagePerCharge;
    }
}
