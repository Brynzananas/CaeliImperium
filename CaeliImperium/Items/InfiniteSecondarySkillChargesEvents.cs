using CaeliImperium.Configs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Networking;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;
using static CaeliImperium.Hooks;
namespace CaeliImperium.Items
{
    public static class InfiniteSecondarySkillChargesEvents
    {
        public static float buffDamage => InfiniteSecondarySkillChargesConfigs.InfiniteSecondarySkillChargesDamagePerCharge.Value;
        public static float itemDamage => InfiniteSecondarySkillChargesConfigs.InfiniteSecondarySkillChargesDamage.Value;
        public static float itemDamagePerStack => InfiniteSecondarySkillChargesConfigs.InfiniteSecondarySkillChargesDamagePerStack.Value;
        private static List<SkillDef> blacklistedSkillDefs = [];
        public static void SkillDef_OnExecute(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            if (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdfld<SkillDef>(nameof(SkillDef.stockToConsume))
                ))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate(HandleInfiniteSecondarySkillCharges);
            }
            else
            {
                CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook failed!");
            }
        }
        public static void Patch(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            while (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdfld<SkillDef>(nameof(SkillDef.stockToConsume))
                ))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate(HandleInfiniteSecondarySkillCharges);
            }
        }
        private static int HandleInfiniteSecondarySkillCharges(int stockToConsume, GenericSkill genericSkill)
        {
            if (!Check(genericSkill)) return stockToConsume;
            CharacterBody characterBody = genericSkill.characterBody;
            if (Util.HasEffectiveAuthority(characterBody.networkIdentity))
            {
                int buffCount = genericSkill.stock / (genericSkill.rechargeStock <= 0 ? 1 : genericSkill.rechargeStock);
                int oldBuffCount = characterBody.GetBuffCount(CaeliImperiumContent.Buffs.IncreaseSecondarySkillDamage);
                if (buffCount > oldBuffCount) characterBody.ApplyBuff(CaeliImperiumContent.Buffs.IncreaseSecondarySkillDamage.buffIndex, buffCount);
            }
            return blacklistedSkillDefs.Contains(genericSkill.skillDef) ? stockToConsume : genericSkill.stock;
        }
        public static bool SkillDef_HasRequiredStockAndDelay(On.RoR2.Skills.SkillDef.orig_HasRequiredStockAndDelay orig, SkillDef self, GenericSkill skillSlot)
        {
            bool flag = orig(self, skillSlot);
            if (!flag) return Check(skillSlot);
            return flag;
        }
        private static bool Check(GenericSkill genericSkill)
        {
            if (!genericSkill) return false;
            CharacterBody characterBody = genericSkill.characterBody;
            if (!characterBody) return false;
            SkillLocator skillLocator = characterBody.skillLocator;
            if (!skillLocator) return false;
            if (skillLocator.secondary != genericSkill) return false;
            Inventory inventory = characterBody.inventory;
            if (!inventory) return false;
            int itemCount = inventory.GetItemCountEffective(CaeliImperiumContent.Items.InfiniteSecondarySkillCharges);
            if (itemCount <= 0) return false;
            return true;
        }
        public static void Events_OnTakeDamageProcess(HealthComponent healthComponent, DamageInfo damageInfo, CharacterBody characterBody, ref float damage)
        {
            if (damageInfo.damageType.damageSource.HasFlag(DamageSource.Secondary))
            {
                int buffCount = characterBody.GetBuffCount(CaeliImperiumContent.Buffs.IncreaseSecondarySkillDamage);
                float multiplier = 1f;
                if (buffCount > 0)
                {
                    multiplier += buffCount * buffDamage;
                    characterBody.SetBuffCount(CaeliImperiumContent.Buffs.IncreaseSecondarySkillDamage.buffIndex, 0);
                }
                if (characterBody.inventory)
                {
                    int itemCount = characterBody.inventory.GetItemCountEffective(CaeliImperiumContent.Items.InfiniteSecondarySkillCharges);
                    multiplier += itemCount.Stack(itemDamage, itemDamagePerStack);
                }
                damage *= multiplier;
            }
        }
        public static void Init(ItemDef itemDef)
        {
            CaeliImperiumContent.Buffs.IncreaseSecondarySkillDamage = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/IncreaseSecondarySkillDamage.asset").RegisterBuffDef();
            BlacklistSkillDef(Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyScopeHeavy.asset").WaitForCompletion());
            BlacklistSkillDef(Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyScopeLight.asset").WaitForCompletion());
            On.RoR2.Skills.SkillDef.HasRequiredStockAndDelay += SkillDef_HasRequiredStockAndDelay;
            IL.RoR2.Skills.SkillDef.OnExecute += Patch;
            IL.RoR2.Skills.DrifterSkillDef.OnExecute += Patch;
            OnTakeDamageProcess += Events_OnTakeDamageProcess;
            CaeliImperiumPlugin.OnPluginDestroyed += OnPluginDestroyed;
        }

        public static void OnPluginDestroyed()
        {
            CaeliImperiumPlugin.OnPluginDestroyed -= OnPluginDestroyed;
            IL.RoR2.Skills.SkillDef.OnExecute -= Patch;
            IL.RoR2.Skills.DrifterSkillDef.OnExecute -= Patch;
            On.RoR2.Skills.SkillDef.HasRequiredStockAndDelay += SkillDef_HasRequiredStockAndDelay;
            OnTakeDamageProcess -= Events_OnTakeDamageProcess;
        }
        public static void BlacklistSkillDef(SkillDef skillDef) => blacklistedSkillDefs.Add(skillDef);
    }
}
