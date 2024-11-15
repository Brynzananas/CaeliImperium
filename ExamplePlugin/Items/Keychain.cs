﻿using BepInEx.Configuration;
using R2API;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfOptions;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static KitchenSanFieroPlugin.KitchenSanFiero;
using RoR2.Orbs;
using KitchenSanFiero.Buffs;
using static R2API.RecalculateStatsAPI;
using Rewired;

namespace KitchenSanFiero.Items
{
    internal static class Keychain
    {
        internal static GameObject KeychainPrefab;
        internal static Sprite KeychainIcon;
        public static ItemDef KeychainItemDef;
        public static ConfigEntry<bool> KeychainAIBlacklist;
        public static ConfigEntry<float> keychainTier;
        public static ConfigEntry<float> keychainInitialCritIncrease;
        public static ConfigEntry<float> keychainChancePerItemStack;
        public static ConfigEntry<bool> KeychainDoElite;
        public static ConfigEntry<float> keychainCritChancePerBuff;
        public static ConfigEntry<float> keychainCritDamagePerBuff;
        public static string name = "Keychain";

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/BrassBellIcon.png";
            switch (keychainTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/BrassBellIconTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/BrassBellIcon.png";
                    break;
                case 3:
                    tier = "Assets/Icons/BrassBellIconTier3.png";
                    break;

            }
            KeychainPrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/Keychains.prefab");
            KeychainIcon = MainAssets.LoadAsset<Sprite>(tier);

            Item();

            AddLanguageTokens();
        }

        private static void AddConfigs()
        {
            KeychainAIBlacklist = Config.Bind<bool>("Item : " + name,
                             "AI Blacklist",
                             false,
                             "Blacklist this item from enemies?");
            keychainTier = Config.Bind<float>("Item : " + name,
                                         "Item tier",
                                         2f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            keychainInitialCritIncrease = Config.Bind<float>("Item : " + name,
                                         "Crit chance addition",
                                         5f,
                                         "Control how much this item gives crit chance addition on all stacks");
            keychainChancePerItemStack = Config.Bind<float>("Item : " + name,
                                         "On kill buff chance",
                                         5f,
                                         "Control the chance of getting a buff on enemy kill per item stack");
            KeychainAIBlacklist = Config.Bind<bool>("Item : " + name,
                             "On elite kill",
                             true,
                             "Always get buff on elite kill?");
            keychainCritChancePerBuff = Config.Bind<float>("Item : " + name,
                                         "Buff crit chance increase",
                                         2.5f,
                                         "Control the crit chance increase per every buff stack");
            keychainCritDamagePerBuff = Config.Bind<float>("Item : " + name,
                                         "Buff crit damage increase",
                                         5f,
                                         "Control the crit damage percentage increase per every buff stack");
            ModSettingsManager.AddOption(new CheckBoxOption(KeychainAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(keychainTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
        }

        private static void Item()
        {
            KeychainItemDef = ScriptableObject.CreateInstance<ItemDef>();
            KeychainItemDef.name = name.Replace(" ", "");
            KeychainItemDef.nameToken = name.ToUpper().Replace(" ", "") + "_NAME";
            KeychainItemDef.pickupToken = name.ToUpper().Replace(" ", "") + "_PICKUP";
            KeychainItemDef.descriptionToken = name.ToUpper().Replace(" ", "") + "_DESC";
            KeychainItemDef.loreToken = name.ToUpper().Replace(" ", "") + "_LORE";
            switch (keychainTier.Value)
            {
                case 1:
                    KeychainItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    KeychainItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    KeychainItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            KeychainItemDef.pickupIconSprite = KeychainIcon;
            KeychainItemDef.pickupModelPrefab = KeychainPrefab;
            KeychainItemDef.canRemove = true;
            KeychainItemDef.hidden = false;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (KeychainAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            KeychainItemDef.tags = tags.ToArray();
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(KeychainItemDef, displayRules));
            On.RoR2.CharacterBody.HandleOnKillEffectsServer += OnKillElite;
            GetStatCoefficients += Stats;
        }

        private static void Stats(CharacterBody sender, StatHookEventArgs args)
        {
            int itemCount = sender.inventory ? sender.inventory.GetItemCount(KeychainItemDef) : 0;
            if (itemCount > 0)
            {
                args.critAdd += keychainInitialCritIncrease.Value;
            }
        }

        private static void OnKillElite(On.RoR2.CharacterBody.orig_HandleOnKillEffectsServer orig, CharacterBody self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if (self && damageReport.victim)
            {
                int itemCount = self.inventory ? self.inventory.GetItemCount(KeychainItemDef) : 0;

                if (itemCount > 0)
                {
                    if (Util.CheckRoll(itemCount * keychainChancePerItemStack.Value, self.master) || (damageReport.victimBody.isElite && KeychainDoElite.Value))
                    {
                        self.AddBuff(KeyBuff.KeyBuffDef);
                    }
                }


            }
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_NAME", name.Replace(" ", ""));
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_PICKUP", "On kill 5% (+5% per item stack) gain a buff, that increases crit chance by 2.5% (+2.5% per buff stack) and crit damage by 5% (+5% per buff stack)");
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_DESC", "On kill 5% (+5% per item stack) gain a buff, that increases crit chance by 2.5% (+2.5% per buff stack) and crit damage by 5% (+5% per buff stack)");
            LanguageAPI.Add(name.ToUpper().Replace(" ", "") + "_LORE", "mmmm yummy");
        }
    }
}
