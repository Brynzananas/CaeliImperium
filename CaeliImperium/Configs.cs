using BepInEx.Configuration;
using static CaeliImperium.Utils;
using static CaeliImperium.Keywords;

namespace CaeliImperium
{
    public static class Configs
    {
        public static void Init()
        {
            HealReceivedDamageConfigs.Init();
            DrawSpeedPathConfigs.Init();
        }
    }
    public static class HealReceivedDamageConfigs
    {
        public static void Init()
        {
            HealReceivedDamageTime = CreateConfig(ItemName + Items.HealReceivedDamage.configName, "Time to fully heal received damage", 15f, "");
            HealReceivedDamageStackTimeReduction = CreateConfig(ItemName + Items.HealReceivedDamage.configName, "Time percentage reduction per stack", 15f, "");
            HealReceivedDamageTime.SettingChanged += SettingChanged;
            HealReceivedDamageStackTimeReduction.SettingChanged += SettingChanged;
        }

        private static void SettingChanged(object sender, System.EventArgs e) => Language.InitHealReceivedDamage();

        public static ConfigEntry<float> HealReceivedDamageTime;
        public static ConfigEntry<float> HealReceivedDamageStackTimeReduction;
    }
    public static class DrawSpeedPathConfigs
    {
        public static void Init()
        {
            SpeedPathSpeedBonusCoefficient = CreateConfig(ItemName + Items.DrawSpeedPath.configName, "Movement speed increase", 0.5f, "");
            SpeedPathSpeedBonusStackCoefficient = CreateConfig(ItemName + Items.DrawSpeedPath.configName, "Movement speed increase per stack", 0.5f, "");
            SpeedPathSpeedBonusCoefficient.SettingChanged += SettingChanged;
            SpeedPathSpeedBonusStackCoefficient.SettingChanged += SettingChanged;
        }
        private static void SettingChanged(object sender, System.EventArgs e) => Language.InitDrawSpeedPath();
        public static ConfigEntry<float> SpeedPathSpeedBonusCoefficient;
        public static ConfigEntry<float> SpeedPathSpeedBonusStackCoefficient;
    }
}
