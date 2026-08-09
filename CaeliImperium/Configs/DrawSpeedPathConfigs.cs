using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using static CaeliImperium.CaeliImperiumUtils;

namespace CaeliImperium.Configs
{
    public static class DrawSpeedPathConfigs
    {
        public static void Init()
        {
            SpeedPathSpeedBonusCoefficient = CreateConfig(CaeliImperiumContent.Items.DrawSpeedPath.configName, "Movement speed increase", 0.5f, "");
            SpeedPathSpeedBonusCoefficient.SettingChanged += SettingChanged;
            SpeedPathSpeedBonusStackCoefficient = CreateConfig( CaeliImperiumContent.Items.DrawSpeedPath.configName, "Movement speed increase per stack", 0.5f, "");
            SpeedPathSpeedBonusStackCoefficient.SettingChanged += SettingChanged;
            SpeedPathMaxPathLength = CreateConfig(CaeliImperiumContent.Items.DrawSpeedPath.configName, "Max speed path length", 240f, "");
            SpeedPathMaxPathLength.SettingChanged += SettingChanged;
            SpeedPathMaxPathLengthStack = CreateConfig(CaeliImperiumContent.Items.DrawSpeedPath.configName, "Max speed path length increase per stack", 120f, "");
            SpeedPathMaxPathLengthStack.SettingChanged += SettingChanged;
            SpeedPathRenderDistance = CreateConfig(CaeliImperiumContent.Items.DrawSpeedPath.configName, "Speed path render distance", 60f, "");
        }
        private static void SettingChanged(object sender, System.EventArgs e) => CaeliImperiumLanguage.InitDrawSpeedPath();
        public static ConfigEntry<float> SpeedPathSpeedBonusCoefficient;
        public static ConfigEntry<float> SpeedPathSpeedBonusStackCoefficient;
        public static ConfigEntry<float> SpeedPathMaxPathLength;
        public static ConfigEntry<float> SpeedPathMaxPathLengthStack;
        public static ConfigEntry<float> SpeedPathRenderDistance;
    }
}
