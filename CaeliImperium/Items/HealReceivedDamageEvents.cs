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
        public static void Init(ItemDef itemDef)
        {
            Hooks.OnInventoryChanged += Events_OnInventoryChanged;
            CaeliImperiumPlugin.OnPluginDestroyed += OnPluginDestroyed;
        }
        private static void OnPluginDestroyed()
        {
            Hooks.OnInventoryChanged -= Events_OnInventoryChanged;
            CaeliImperiumPlugin.OnPluginDestroyed -= OnPluginDestroyed;
        }
        public static void Events_OnInventoryChanged(CharacterBody obj)
        {
            if (!NetworkServer.active) return;
            int stacks = obj.inventory ? obj.inventory.GetItemCountEffective(CaeliImperiumContent.Items.HealReceivedDamage) : 0;
            obj.AddItemBehavior<HealReceivedDamageBehaviour>(stacks);
        }
    }
}
