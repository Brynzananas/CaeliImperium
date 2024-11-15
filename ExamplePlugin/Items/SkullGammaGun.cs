﻿using BepInEx.Configuration;
using KitchenSanFiero.Buffs;
using R2API;
using RoR2;
using RoR2.Items;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RiskOfOptions.Options;
using RiskOfOptions;
using static KitchenSanFieroPlugin.KitchenSanFiero;
using BepInEx;
using RiskOfOptions.OptionConfigs;
using UnityEngine.Networking;

namespace KitchenSanFiero.Items
{
    public static class SkullGammaGun
    {

        
        internal static GameObject MicrowavePrefab;
        internal static Sprite MicrowaveIcon;
        public static ItemDef MicrowaveItemDef;
        public static ConfigEntry<bool> SkullGammaGunEnable;
        public static ConfigEntry<bool> SkullGammaGunAIBlacklist;
        public static ConfigEntry<float> SkullGammaGunTier;
        public static ConfigEntry<float> SkullGammaGunTimer;
        public static ConfigEntry<float> SkullGammaGunAngle;
        public static ConfigEntry<float> SkullGammaGunAngleStack;
        public static ConfigEntry<float> SkullGammaGunRange;
        public static ConfigEntry<float> SkullGammaGunRangeStack;
        public static ConfigEntry<float> SkullGammaGunBuffDamageMultiplier;
        public static ConfigEntry<float> SkullGammaGunDamageMultiplier;
        public static ConfigEntry<float> SkullGammaGunDuration;
        public static ConfigEntry<int> SkullGammaGunMaxBuffCount;
        public static ConfigEntry<int> SkullGammaGunMaxBuffCountStack;
        public static ConfigEntry<float> SkullGammaGunBuffDamage;

        internal static void Init()
        {
            AddConfigs();
            string tier = "Assets/Icons/SkullGammaGunIcon.png";
            switch (SkullGammaGunTier.Value)
            {
                case 1:
                    tier = "Assets/Icons/SkullGammaGunIconTier1.png";
                    break;
                case 2:
                    tier = "Assets/Icons/SkullGammaGunIcon.png";
                    break;
                case 3:
                    tier = "Assets/Icons/SkullGammaGunIconTier3.png";
                    break;

            }
            MicrowavePrefab = MainAssets.LoadAsset<GameObject>("Assets/Models/Prefabs/SkullGammaGun.prefab");
            MicrowaveIcon = MainAssets.LoadAsset<Sprite>(tier);
            Item();
            //FixedUpdate();
            AddLanguageTokens();
        }
        public static void AddConfigs()
        {
            SkullGammaGunEnable = Config.Bind<bool>("Item : Skull Gamma Gun",
                                         "Activation",
                                         true,
                                         "Enable Skull Gamma Gun item?");
            SkullGammaGunAIBlacklist = Config.Bind<bool>("Item : Skull Gamma Gun",
                             "AI Blacklist",
                             true,
                             "Blacklist this item from enemies?");
            SkullGammaGunTier = Config.Bind<float>("Item : Skull Gamma Gun",
                                         "Item tier",
                                         2f,
                                         "1: Common/White\n2: Rare/Green\n3: Legendary/Red");
            SkullGammaGunTimer = Config.Bind<float>("Item : Skull Gamma Gun",
                                         "Interval",
                                         0.5f,
                                         "Control the interval this item applies the debuff in seconds");
            SkullGammaGunAngle = Config.Bind<float>("Item : Skull Gamma Gun",
                                         "Base angle",
                                         30f,
                                         "Control the base angle value");
            SkullGammaGunAngleStack = Config.Bind<float>("Item : Skull Gamma Gun",
                                         "Angle per stack",
                                         15f,
                                         "Control the angle increase per item stack");
            SkullGammaGunRange = Config.Bind<float>("Item : Skull Gamma Gun",
                                         "Base range",
                                         6f,
                                         "Control the base range value");
            SkullGammaGunRangeStack = Config.Bind<float>("Item : Skull Gamma Gun",
                                         "Range per stack",
                                         4f,
                                         "Control the range increase per item stack");
            SkullGammaGunBuffDamageMultiplier = Config.Bind<float>("Item : Skull Gamma Gun",
                                         "Damage multiplier",
                                         1f,
                                         "Control the damage multiplier of the debuff");
            SkullGammaGunMaxBuffCount = Config.Bind<int>("Item : Skull Gamma Gun",
                                         "Base max debuff stack",
                                         3,
                                         "Control the base max debuff stack");
            SkullGammaGunMaxBuffCountStack = Config.Bind<int>("Item : Skull Gamma Gun",
                                         "Max debuff stack per stack",
                                         1,
                                         "Control the max debuff stack increase per item stack");
            SkullGammaGunBuffDamage = Config.Bind<float>("Item : Skull Gamma Gun",
                             "Debuff stack damage",
                             0.5f,
                             "Control how much debuff stack increases its damage multiplicatively?");
            ModSettingsManager.AddOption(new CheckBoxOption(SkullGammaGunEnable, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new CheckBoxOption(SkullGammaGunAIBlacklist, new CheckBoxConfig() { restartRequired = true }));
            ModSettingsManager.AddOption(new StepSliderOption(SkullGammaGunTier, new StepSliderConfig() { min = 1, max = 3, increment = 1f, restartRequired = true }));
            ModSettingsManager.AddOption(new FloatFieldOption(SkullGammaGunTimer));
            ModSettingsManager.AddOption(new FloatFieldOption(SkullGammaGunAngle));
            ModSettingsManager.AddOption(new FloatFieldOption(SkullGammaGunAngleStack));
            ModSettingsManager.AddOption(new FloatFieldOption(SkullGammaGunRange));
            ModSettingsManager.AddOption(new FloatFieldOption(SkullGammaGunRangeStack));
            ModSettingsManager.AddOption(new FloatFieldOption(SkullGammaGunBuffDamageMultiplier));
            ModSettingsManager.AddOption(new IntFieldOption(SkullGammaGunMaxBuffCount));
            ModSettingsManager.AddOption(new IntFieldOption(SkullGammaGunMaxBuffCountStack));
            ModSettingsManager.AddOption(new FloatFieldOption(SkullGammaGunBuffDamage));
        }
        private static void Item()
        {
            MicrowaveItemDef = ScriptableObject.CreateInstance<ItemDef>();
            MicrowaveItemDef.name = "SkullGammaGun";
            MicrowaveItemDef.nameToken = "SKULLGAMMAGUN_NAME";
            MicrowaveItemDef.pickupToken = "SKULLGAMMAGUN_PICKUP";
            MicrowaveItemDef.descriptionToken = "SKULLGAMMAGUN_DESC";
            MicrowaveItemDef.loreToken = "SKULLGAMMAGUN_LORE";
            switch (SkullGammaGunTier.Value)
            {
                case 1:
                    MicrowaveItemDef.deprecatedTier = ItemTier.Tier1;
                    break;
                case 2:
                    MicrowaveItemDef.deprecatedTier = ItemTier.Tier2;
                    break;
                case 3:
                    MicrowaveItemDef.deprecatedTier = ItemTier.Tier3;
                    break;

            }
            MicrowaveItemDef.pickupIconSprite = MicrowaveIcon;
            MicrowaveItemDef.pickupModelPrefab = MicrowavePrefab;
            MicrowaveItemDef.canRemove = true;
            MicrowaveItemDef.hidden = false;
            var tags = new List<ItemTag>() { ItemTag.Damage };
            if (SkullGammaGunAIBlacklist.Value)
            {
                tags.Add(ItemTag.AIBlacklist);
            }
            MicrowaveItemDef.tags = tags.ToArray();
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Head",
localPos = new Vector3(-0.15534F, 0.28751F, -0.00002F),
localAngles = new Vector3(0F, 180F, 313.8421F),
localScale = new Vector3(0.09184F, 0.09184F, 0.09184F)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Head",
localPos = new Vector3(-0.08506F, 0.28505F, -0.04497F),
localAngles = new Vector3(0F, 180F, 325.181F),
localScale = new Vector3(0.06837F, 0.06837F, 0.06837F)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Head",
localPos = new Vector3(-0.09128F, 0.00883F, 0.01715F),
localAngles = new Vector3(0F, 180F, 313.2246F),
localScale = new Vector3(0.05074F, 0.05074F, 0.05074F)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "HeadCenter",
localPos = new Vector3(1.642F, 1.26774F, 0.67249F),
localAngles = new Vector3(56.20844F, 0.00001F, 314.3897F),
localScale = new Vector3(1F, 1F, 1F)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Chest",
localPos = new Vector3(0F, 0.73419F, 0F),
localAngles = new Vector3(350.008F, 180F, 0F),
localScale = new Vector3(0.12937F, 0.12937F, 0.12937F)
                }
            });/*
            rules.Add("mdlEngiTurrety", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });*/
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Head",
localPos = new Vector3(-0.07704F, 0.14855F, -0.04397F),
localAngles = new Vector3(358.2678F, 180.0091F, 328.8737F),
localScale = new Vector3(0.06909F, 0.06909F, 0.06909F)
                }
            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Head",
localPos = new Vector3(-0.09267F, 0.18578F, 0.04265F),
localAngles = new Vector3(0F, 180F, 315.4285F),
localScale = new Vector3(0.07905F, 0.07905F, 0.07905F)
                }
            });/*
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlSeeker", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlChef", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlFalseSon", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });
            rules.Add("mdlScav", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = MicrowavePrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });*/
            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(MicrowaveItemDef, displayRules));
            On.RoR2.CharacterBody.OnInventoryChanged += ActivateBehaviour;
            //On.RoR2.CharacterBody.FixedUpdate += BUUURNN;
        }

        private static void ActivateBehaviour(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            if (NetworkServer.active)
            {
                    self.AddItemBehavior<SkullGammaGunBehaviour>(self.inventory.GetItemCount(MicrowaveItemDef));
            }
            orig(self);
        }

        public class SkullGammaGunBehaviour : RoR2.CharacterBody.ItemBehavior
        {
            //[BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
            private float timer1;
            private bool isWorking;

            private static ItemDef GetItemDef()
            {
                return MicrowaveItemDef;
            }
            private void Awake()
            {
                base.enabled = false;
            }

            private void OnEnable()
            {
                //base.enabled = true;
            }

            private void FixedUpdate()
            {
                //int itemCount = body.inventory ? body.inventory.GetItemCount(MicrowaveItemDef) : 0;
                if (!NetworkServer.active)
                {
                    return;
                }
                int stack = this.stack;
                if (stack > 0)
                {
                    //Vector3 aim = self.inputBank.aimDirection;
                    //var owner = self.transform;
                    timer1 += Time.fixedDeltaTime;
                    if (timer1 > SkullGammaGunTimer.Value / body.attackSpeed)
                    {
                        foreach (var characterBody in CharacterBody.readOnlyInstancesList)
                        {



                            Vector3 targetDir = characterBody.corePosition - body.corePosition;
                            float angle = Vector3.Angle(targetDir, body.inputBank.aimDirection);
                            float dist = Vector3.Distance(characterBody.corePosition, body.corePosition);
                            if (angle < (SkullGammaGunAngle.Value / 2) + (SkullGammaGunAngleStack.Value / 2 * stack) && body.teamComponent.teamIndex != characterBody.teamComponent.teamIndex && dist < SkullGammaGunRange.Value + (stack * SkullGammaGunRangeStack.Value))
                            {
                                float damage = body.damage;
                                Vector3 position2 = characterBody.transform.position;
                                DamageInfo damageInfo2 = new DamageInfo
                                {
                                    damage = damage * stack * (SkullGammaGunRange.Value + (stack * SkullGammaGunRangeStack.Value)) / dist / ((SkullGammaGunRange.Value + (stack * SkullGammaGunRangeStack.Value)) / 2),
                                    damageColorIndex = DamageColorIndex.Item,
                                    damageType = DamageType.Generic,
                                    attacker = body.gameObject,
                                    crit = Util.CheckRoll(body.crit),
                                    force = Vector3.zero,
                                    inflictor = null,
                                    position = position2,
                                    procChainMask = default,
                                    procCoefficient = 1f
                                };
                                characterBody.healthComponent.TakeDamage(damageInfo2);
                                if (!characterBody.gameObject.GetComponent<IrradiatedBuff.IrradiateComponent>())
                                {
                                    IrradiatedBuff.IrradiateComponent component = characterBody.gameObject.AddComponent<IrradiatedBuff.IrradiateComponent>();
                                    component.body = characterBody;
                                }
                                InflictDotInfo dotInfo = new InflictDotInfo()
                                {
                                    attackerObject = body.gameObject,
                                    victimObject = characterBody.gameObject,
                                    totalDamage = body.damage * SkullGammaGunBuffDamageMultiplier.Value, //* PackOfCiggaretesDuration.Value,
                                    damageMultiplier = stack * (SkullGammaGunRange.Value + (stack * SkullGammaGunRangeStack.Value)) / dist / ((SkullGammaGunRange.Value + (stack * SkullGammaGunRangeStack.Value)) / 2) * ((characterBody.GetBuffCount(IrradiatedBuff.IrradiatedBuffDef) + 1) * SkullGammaGunBuffDamage.Value),
                                    duration = 5f,
                                    dotIndex = Buffs.IrradiatedBuff.IrradiatedDOTDef,
                                    maxStacksFromAttacker = (uint?)(SkullGammaGunMaxBuffCount.Value + (stack - 1 * SkullGammaGunMaxBuffCountStack.Value))

                                };
                                //StrengthenBurnUtils.CheckDotForUpgrade(self.inventory, ref dotInfo);
                                DotController.InflictDot(ref dotInfo);
                            }
                            timer1 = 0;

                        }
                    }
                }


            }

            private void OnDisable()
            {
                //base.enabled = false;
            }
        }

        /*
         private void BUUURNN(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
         {
             orig(self);
             int itemCount = self.inventory ? self.inventory.GetItemCount(MicrowaveItemDef) : 0;
             if (itemCount > 0)
             {
                 //Vector3 aim = self.inputBank.aimDirection;
                 //var owner = self.transform;
                 foreach (var characterBody in CharacterBody.readOnlyInstancesList)
                 {

                     timer += Time.fixedDeltaTime;
                     if (timer > SkullGammaGunTimer.Value * 2)
                     {
                         Vector3 targetDir = characterBody.corePosition - self.corePosition;
                         float angle = Vector3.Angle(targetDir, self.inputBank.aimDirection);
                         float dist = Vector3.Distance(characterBody.corePosition, self.corePosition);
                         if (angle < SkullGammaGunAngle.Value + (SkullGammaGunAngleStack.Value * itemCount) && self.teamComponent.teamIndex != characterBody.teamComponent.teamIndex && dist < SkullGammaGunRange.Value * 10 + (itemCount * SkullGammaGunRangeStack.Value * 10))
                         {
                             float damage = self.damage;
                             Vector3 position2 = characterBody.transform.position;
                             DamageInfo damageInfo2 = new DamageInfo
                             {
                                 damage = damage * itemCount,
                                 damageColorIndex = DamageColorIndex.Item,
                                 damageType = DamageType.Generic,
                                 attacker = self.gameObject,
                                 crit = Util.CheckRoll(self.crit),
                                 force = Vector3.zero,
                                 inflictor = null,
                                 position = position2,
                                 procChainMask = default,
                                 procCoefficient = 1f
                             };
                             characterBody.healthComponent.TakeDamage(damageInfo2);
                             InflictDotInfo dotInfo = new InflictDotInfo()
                             {
                                 attackerObject = self.gameObject,
                                 victimObject = characterBody.gameObject,
                                 totalDamage = self.damage * SkullGammaGunBuffDamageMultiplier.Value, //* PackOfCiggaretesDuration.Value,
                                 damageMultiplier = itemCount * ((SkullGammaGunRange.Value * 10 + (itemCount * SkullGammaGunRangeStack.Value * 10) - dist) / 10) * ((characterBody.GetBuffCount(IrradiatedBuff.IrradiatedBuffDef) + 1) * SkullGammaGunBuffDamage.Value),
                                 duration = 5f,
                                 dotIndex = Buffs.IrradiatedBuff.IrradiatedDOTDef,
                                 maxStacksFromAttacker = (uint?)(SkullGammaGunMaxBuffCount.Value + (itemCount - 1 * SkullGammaGunMaxBuffCountStack.Value))

                             };
                             //StrengthenBurnUtils.CheckDotForUpgrade(self.inventory, ref dotInfo);
                             DotController.InflictDot(ref dotInfo);
                         }
                         timer = 0;

                     }
                 }
             }


         }
         */
        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("SKULLGAMMAGUN_NAME", "Skull Gamma Gun");
            LanguageAPI.Add("SKULLGAMMAGUN_PICKUP", "In " + SkullGammaGunAngle.Value + " (+" + SkullGammaGunAngleStack.Value + " per item stack) degree radius and " + SkullGammaGunRange.Value + " (+ " + SkullGammaGunRangeStack.Value + " per item stack) meter distance irradiate enemies for 100% base damage");
            LanguageAPI.Add("SKULLGAMMAGUN_DESC", "In " + SkullGammaGunAngle.Value + " (+" + SkullGammaGunAngleStack.Value + " per item stack) degree radius and " + SkullGammaGunRange.Value + " (+ " + SkullGammaGunRangeStack.Value + " per item stack) meter distance irradiate enemies for 100% base damage");
            LanguageAPI.Add("SKULLGAMMAGUN_LORE", "Laputan Machine");
        }
    }
}
