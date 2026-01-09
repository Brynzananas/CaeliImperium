using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using static CaeliImperium.Utils;

namespace CaeliImperium.Configs
{
    public static class DrawSpeedPathConfigs
    {
        public static void Init()
        {
            SpeedPathSpeedBonusCoefficient = CreateConfig(CaeliImperiumContent.Items.DrawSpeedPath.configName, "Movement speed increase", 0.5f, "");
            SpeedPathSpeedBonusStackCoefficient = CreateConfig( CaeliImperiumContent.Items.DrawSpeedPath.configName, "Movement speed increase per stack", 0.5f, "");
            SpeedPathSpeedBonusCoefficient.SettingChanged += SettingChanged;
            SpeedPathSpeedBonusStackCoefficient.SettingChanged += SettingChanged;
        }
        private static void SettingChanged(object sender, System.EventArgs e) => Language.InitDrawSpeedPath();
        public static ConfigEntry<float> SpeedPathSpeedBonusCoefficient;
        public static ConfigEntry<float> SpeedPathSpeedBonusStackCoefficient;
    }
}
