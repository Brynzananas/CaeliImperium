using CaeliImperium.Configs;
using CaeliImperium.ItemBehaviours;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Items
{
    public static class HealReceivedDamageEvents
    {
        public static float HealReceivedHealCoefficient => HealReceivedDamageConfigs.HealReceivedDamageHealCoefficient.Value;
        public static float HealReceivedHealCoefficientPerStack => HealReceivedDamageConfigs.HealReceivedDamageHealCoefficientPerStack.Value;
        public static float HealReceivedDamageTime => HealReceivedDamageConfigs.HealReceivedDamageTime.Value;
        public static float HealReceivedDamageStackTimeReduction => HealReceivedDamageConfigs.HealReceivedDamageStackTimeReduction.Value;
        public static float neededHealRateToTypeBeat = 0.25f;
        public static Material HealMaterial;
        public static EffectDef TypeBeatEffect;
        private static bool inited;
        public static void Init(ItemDef itemDef)
        {
            CaeliImperiumHooks.OnInventoryChanged += Events_OnInventoryChanged;
            CaeliImperiumAssets.onMaterialFound += OnMaterialFound;
            CaeliImperiumPlugin.onPluginDestroyed += OnPluginDestroyed;
            if (inited) return;
            inited = true;
            TypeBeatEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Effects/EmergencyMedicalTreatmentTypeBeatEffect.prefab").RegisterEffect();
        }
        private static void OnMaterialFound(Material material)
        {
            if (material.name == "matEmergencyMedicalTreatmentHealing") HealMaterial = material;
        }
        private static void OnPluginDestroyed()
        {
            CaeliImperiumHooks.OnInventoryChanged -= Events_OnInventoryChanged;
            CaeliImperiumAssets.onMaterialFound -= OnMaterialFound;
            CaeliImperiumPlugin.onPluginDestroyed -= OnPluginDestroyed;
        }
        public static void Events_OnInventoryChanged(CharacterBody obj)
        {
            if (!NetworkServer.active) return;
            int stacks = obj.inventory ? obj.inventory.GetItemCountEffective(CaeliImperiumContent.Items.HealReceivedDamage) : 0;
            obj.AddItemBehavior<HealReceivedDamageBehaviour>(stacks);
        }
    }
}
