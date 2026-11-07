using HarmonyLib;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static CaeliImperium.Bodies.DoomguyEvents;

namespace CaeliImperium.ModCompatibilities;
public static class VarianceCompatability
{
    public const string GUID = "com.Nebby.VAPI";
    private static bool init;
    public static void DoomguyInit()
    {
        if (init) return;
        init = true;
        Harmony.CreateAndPatchAll(typeof(VariantSpawnManagerPatches));
    }
    [HarmonyPatch(typeof(VAPI.VariantSpawnManager))]
    public class VariantSpawnManagerPatches
    {
        [HarmonyPatch((nameof(VAPI.VariantSpawnManager.OnRulebookUpdated)), new[] { typeof(NetworkRuleBook) })]
        [HarmonyPrefix]
        public static void OnRulebookUpdated_Postfix(VAPI.VariantSpawnManager __instance, NetworkRuleBook book)
        {
            __instance.availableVariants.Clear();
        }
    }
}
