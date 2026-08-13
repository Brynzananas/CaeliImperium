using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Rendering.VirtualTexturing;

namespace CaeliImperium.Configs;

public static class CaeliImperiumConfigs
{
    public const string sectionName = "Main";
    public static void Init()
    {
        Screaming = CaeliImperiumUtils.CreateConfig(sectionName, "Screaming", true, "", false);
        DrawSpeedPathConfigs.Init();
        HealReceivedDamageConfigs.Init();
        InfiniteSecondarySkillChargesConfigs.Init();
        BomberWispConfigs.Init();
        MonsterChestConfigs.Init();
    }
    public static ConfigEntry<bool> OverrideConfigValuesOnUpdate;
    public static ConfigEntry<bool> Screaming;
}
