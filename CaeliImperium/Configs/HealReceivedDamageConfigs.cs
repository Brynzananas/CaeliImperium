using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using CaeliImperium;
using static CaeliImperium.Utils;

namespace CaeliImperium.Configs
{
    public static class HealReceivedDamageConfigs
    {
        public static void Init()
        {
            HealReceivedDamageHealCoefficient = CreateConfig(CaeliImperiumContent.Items.HealReceivedDamage.configName, "Heal coefficient", 1f, "");
            HealReceivedDamageHealCoefficientPerStack = CreateConfig(CaeliImperiumContent.Items.HealReceivedDamage.configName, "Heal coefficient per stack", 0f, "");
            HealReceivedDamageTime = CreateConfig(CaeliImperiumContent.Items.HealReceivedDamage.configName, "Time to fully heal received damage", 15f, "");
            HealReceivedDamageStackTimeReduction = CreateConfig(CaeliImperiumContent.Items.HealReceivedDamage.configName, "Time percentage reduction per stack", 15f, "");
            HealReceivedDamageHealCoefficient.SettingChanged += SettingChanged;
            HealReceivedDamageHealCoefficientPerStack.SettingChanged += SettingChanged;
            HealReceivedDamageTime.SettingChanged += SettingChanged;
            HealReceivedDamageStackTimeReduction.SettingChanged += SettingChanged;
        }
        private static void SettingChanged(object sender, System.EventArgs e) => Language.InitHealReceivedDamage();

        public static ConfigEntry<float> HealReceivedDamageHealCoefficient;
        public static ConfigEntry<float> HealReceivedDamageHealCoefficientPerStack;
        public static ConfigEntry<float> HealReceivedDamageTime;
        public static ConfigEntry<float> HealReceivedDamageStackTimeReduction;
    }
}
