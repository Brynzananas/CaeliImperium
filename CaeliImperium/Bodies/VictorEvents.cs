using BrynzaAPI;
using CaeliImperium.Components;
using CaeliImperium.NetworkMessages;
using CaeliImperium.ScriptableObjects;
using CaeliImperiumEntityStates.Victor;
using HarmonyLib;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RoR2.BuffDef;

namespace CaeliImperium.Bodies
{
    public static class VictorEvents
    {
        public static SurvivorDef Survivor;
        public static GameObject BodyPrefab;
        public static CharacterBody Body;
        public static GameObject MasterPrefab;
        public static SkillFamily Passive;
        public static PassiveItemSkillDef Immortal;
        public static SkillFamily Primary;
        public static SkillDef FireSyringes;
        public static SkillFamily Secondary;
        public static SkillDef AimSpear;
        public static SkillDef PrepareBlade;
        public static SkillFamily Sprint;
        public static SkillDef BloodTribute;
        public static SkillFamily Utility;
        public static VictorSkillDef InjectSerum;
        public static SkillFamily Special;
        public static float GutsGain = 8f;
        public static float GutsTake = 80f;
        public static DamageAPI.ModdedDamageType SummonDeathAuraDamageType;
        public static DamageAPI.ModdedDamageType GainGutsDamageType;
        public static ItemDef Guts;
        public static ItemDef ImmortalPassive;
        public static BuffDef Serum;
        public static float SerumRegen = 0.1f;
        public static float SerumMoveSpeed = 0.1f;
        public static float SerumAttackSpeed = 0.01f;
        public static float SerumDurationMultiplier = 0.1f;
        private static bool inited;
        public static void Init(GameObject gameObject)
        {
            CaeliImperiumPlugin.onPluginDestroyed += OnPluginDestroyed;
            IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            IL.RoR2.HealthComponent.Heal += HealthComponent_Heal;
            BrynzaAPI.BrynzaAPI.GetDynamicRegen += BrynzaAPI_GetDynamicRegen;
            BrynzaAPI.BrynzaAPI.GetDynamicAttackSpeed += BrynzaAPI_GetDynamicAttackSpeed;
            BrynzaAPI.BrynzaAPI.GetDynamicMoveSpeed += BrynzaAPI_GetDynamicMoveSpeed;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;
            On.RoR2.CharacterBody.GetBuffCount_BuffIndex += CharacterBody_GetBuffCount_BuffIndex;
            IL.RoR2.CharacterBody.UpdateBuffs += CharacterBody_UpdateBuffs;
            IL.RoR2.GenericPickupController.AttemptGrant += GenericPickupController_AttemptGrant;
            //IL.RoR2.UI.BuffIcon.UpdateIcon += BuffIcon_UpdateIcon;
            if (inited) return;
            BodyPrefab = gameObject;
            Survivor = CaeliImperiumAssets.assetBundle.LoadAsset<SurvivorDef>("Assets/CaeliImperium/Bodies/Victor/Character/CIVictor.asset").RegisterSurvivor();
            Body = CaeliImperiumUtils.HandleBody(BodyPrefab);
            Body.AddModdedBodyFlag(BrynzaAPI.Assets.SprintAllTime);
            Body.preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").WaitForCompletion();
            GenericSkill[] genericSkills = BodyPrefab.GetComponents<GenericSkill>();
            GenericSkill sprintSkill = null;
            foreach (GenericSkill skill in genericSkills)
            {
                if (skill.skillName == "Sprint") sprintSkill = skill;
            }
            BodyPrefab.GetComponent<SkillLocator>().SetSprintSkill(sprintSkill);
            //Body._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            Passive = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Victor/SkillFamilies/VictorPassive.asset").RegisterSkillFamily();
            Immortal = CaeliImperiumAssets.assetBundle.LoadAsset<PassiveItemSkillDef>("Assets/CaeliImperium/Bodies/Victor/Skills/Immortal.asset").RegisterSkillDef();
            Primary = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Victor/SkillFamilies/VictorPrimary.asset").RegisterSkillFamily();
            FireSyringes = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/Victor/Skills/FireSyringes.asset").RegisterSkillDef();
            Secondary = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Victor/SkillFamilies/VictorSecondary.asset").RegisterSkillFamily();
            PrepareBlade = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/Victor/Skills/PrepareBlade.asset").RegisterSkillDef();
            BloodTribute = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/Victor/Skills/BloodTribute.asset").RegisterSkillDef();
            AimSpear = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/Victor/Skills/AimSpear.asset").RegisterSkillDef();
            Sprint = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Victor/SkillFamilies/VictorSprint.asset").RegisterSkillFamily();
            Utility = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Victor/SkillFamilies/VictorUtility.asset").RegisterSkillFamily();
            InjectSerum = CaeliImperiumAssets.assetBundle.LoadAsset<VictorSkillDef>("Assets/CaeliImperium/Bodies/Victor/Skills/InjectSerum.asset").RegisterSkillDef();
            Special = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Victor/SkillFamilies/VictorSpecial.asset").RegisterSkillFamily();
            Guts = CaeliImperiumAssets.assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Bodies/Victor/Items/Guts.asset").RegisterItemDef();
            ImmortalPassive = CaeliImperiumAssets.assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Bodies/Victor/Items/ImmortalPassive.asset").RegisterItemDef();
            Serum = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Victor/Buffs/Serum.asset").RegisterBuffDef();
            SummonDeathAuraDamageType = DamageAPI.ReserveDamageType();
            GainGutsDamageType = DamageAPI.ReserveDamageType();
            BrynzaAPI.BrynzaAPI.OnPickupCreated += BrynzaAPI_GetCreatePickupDropletMessage;
            CaeliImperiumEntityStates.Victor.FireSyringes.tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/TracerToolbotNails.prefab").WaitForCompletion();
            CaeliImperiumEntityStates.Victor.FireSyringes.hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ImpactNailgun.prefab").WaitForCompletion();
            CaeliImperiumEntityStates.Victor.SlashBlade.tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/TracerToolbotRebar.prefab").WaitForCompletion();
            CaeliImperiumEntityStates.Victor.SlashBlade.hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ImpactSpear.prefab").WaitForCompletion();
            CaeliImperiumEntityStates.Victor.FireSpear.tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/TracerToolbotRebar.prefab").WaitForCompletion();
            CaeliImperiumEntityStates.Victor.FireSpear.hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ImpactSpear.prefab").WaitForCompletion();
            CaeliImperiumEntityStates.Victor.AimSpear.characterCameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/DLC1/Railgunner/ccpRailgunnerScopeLight.asset").WaitForCompletion();
            typeof(FireSyringes).RegisterEntityState();
            typeof(PrepareBlade).RegisterEntityState();
            typeof(SlashBlade).RegisterEntityState();
            typeof(InjectSerum).RegisterEntityState();
            typeof(AimSpear).RegisterEntityState();
            typeof(AimDownSpear).RegisterEntityState();
            typeof(AimSecondary).RegisterEntityState();
            typeof(FireSpear).RegisterEntityState();
            typeof(BloodTribute).RegisterEntityState();
            typeof(Reviving).RegisterEntityState();
            inited = true;
        }
        private static void GenericPickupController_AttemptGrant(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            if (
                !c.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(1),
                    x => x.MatchCallvirt(typeof(CharacterBody).GetPropertyGetter(nameof(CharacterBody.inventory))),
                    x => x.MatchCall<UnityEngine.Object>("op_Implicit"),
                    x => x.MatchBrfalse(out iLLabel),
                    x => x.MatchLdloc(out _),
                    x => x.MatchBrfalse(out _)
                ))
            {
                CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 1 failed!");
                return;
            }
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(CanPickupGuts);
            c.Emit(OpCodes.Brfalse_S, iLLabel);
        }
        private static bool CanPickupGuts(CharacterBody characterBody, GenericPickupController genericPickupController) => genericPickupController.pickup.pickupIndex == PickupCatalog.FindPickupIndex(Guts.itemIndex) ? characterBody.inventory.GetItemCountEffective(ImmortalPassive) > 0 : true;
        
        private static void BrynzaAPI_GetCreatePickupDropletMessage(GenericPickupController.CreatePickupInfo createPickupInfo, GameObject pickupDroplet)
        {
            if (createPickupInfo.pickup.pickupIndex == PickupCatalog.FindPickupIndex(Guts.itemIndex)) pickupDroplet.AddComponent<GutsTempPickupDecay>();
        }

        private static void CharacterBody_UpdateBuffs(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            if (
                !c.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(1)
                ))
            {
                CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 1 failed!");
                return;
            }
            c.Emit(OpCodes.Ldloc, 1);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(DurationBuffMultiplier);
            c.Emit(OpCodes.Mul);
        }
        private static float DurationBuffMultiplier(CharacterBody.TimedBuff timedBuff, CharacterBody characterBody)
        {
            if (timedBuff.buffIndex != Serum.buffIndex) return 1f;
            return characterBody.GetSerumCount() * SerumDurationMultiplier;
        }
        public static float GetSerumCount(this CharacterBody characterBody)
        {
            float duration = 0;
            foreach (CharacterBody.TimedBuff timedBuff in characterBody.timedBuffs)
            {
                if (timedBuff.buffIndex != Serum.buffIndex) continue;
                duration += timedBuff.timer;
            }
            return duration;
        }
        private static int CharacterBody_GetBuffCount_BuffIndex(On.RoR2.CharacterBody.orig_GetBuffCount_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            if (buffType == Serum.buffIndex)
            {
                float duration = 0;
                foreach (CharacterBody.TimedBuff timedBuff in self.timedBuffs)
                {
                    if (timedBuff.buffIndex != buffType) continue;
                    duration += timedBuff.timer;
                }
                return (int)Mathf.Floor(duration);
            }
            return orig(self, buffType);
        }

        private static void BuffIcon_UpdateIcon(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            if (
                !c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<BuffIcon>(nameof(BuffIcon.buffDef)),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<BuffIcon>(nameof(BuffIcon.lastBuffDef)),
                    x => x.MatchCall<UnityEngine.Object>("op_Equality"),
                    x => x.MatchBrfalse(out iLLabel)
                ))
            {
                CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 1 failed!");
                return;
            }
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(BuffIcon), nameof(BuffIcon.buffDef)));
            c.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(VictorEvents), nameof(VictorEvents.Serum)));
            c.Emit(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Object), "op_Equality"));
            c.Emit(OpCodes.Brtrue_S, iLLabel);
        }
        private static void CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            if (buffDef == Serum)
            {
                if (!self.HasBuff(buffDef))
                {
                    self.timedBuffs.Add(new CharacterBody.TimedBuff { buffIndex = buffDef.buffIndex, timer = duration, totalDuration = duration});
                }
                else
                {
                    foreach (CharacterBody.TimedBuff timedBuff in self.timedBuffs)
                    {
                        if (timedBuff.buffIndex != buffDef.buffIndex) continue;
                        timedBuff.totalDuration += duration;
                        timedBuff.timer += duration;
                        break;
                    }
                }
                return;
            }
            orig(self, buffDef, duration);
        }
        private static void BrynzaAPI_GetDynamicMoveSpeed(CharacterBody characterBody, ref float moveSpeed)
        {
            if (!characterBody.HasBuff(Serum)) return;
            float multiplier = moveSpeed / (characterBody.isSprinting ? characterBody.sprintingSpeedMultiplier : 1f) / (characterBody.baseMoveSpeed + characterBody.levelMoveSpeed * (characterBody.level - 1f));
            moveSpeed += characterBody.GetSerumCount() * SerumMoveSpeed * multiplier;
        }
        private static void BrynzaAPI_GetDynamicAttackSpeed(CharacterBody characterBody, ref float attackSpeed)
        {
            if (!characterBody.HasBuff(Serum)) return;
            float multiplier = attackSpeed / (characterBody.baseAttackSpeed + characterBody.levelAttackSpeed * (characterBody.level - 1f));
            attackSpeed += characterBody.GetSerumCount() * SerumAttackSpeed * multiplier;
        }
        private static void BrynzaAPI_GetDynamicRegen(CharacterBody characterBody, ref float regen)
        {
            if (!characterBody.HasBuff(Serum)) return;
            float multiplier = regen / (characterBody.baseRegen + characterBody.levelRegen * (characterBody.level - 1f));
            regen += characterBody.GetSerumCount() * SerumRegen * multiplier;
        }
        private static void HealthComponent_Heal(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.Goto(il.Instrs[il.Instrs.Count - 1]);
            if (
                !c.TryGotoPrev(MoveType.Before,
                    x => x.MatchLdarg(3),
                    x => x.MatchBrfalse(out _)
                ))
            {
                CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 1 failed!");
                return;
            }
            ILLabel iLLabel = null;
            Instruction instruction1 = c.Next;
            //c = new ILCursor(il);
            if (
                !c.TryGotoPrev(MoveType.Before,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.health)),
                    x => x.MatchLdarg(0),
                    x => x.MatchCall(typeof(HealthComponent).GetPropertyGetter(nameof(HealthComponent.fullHealth))),
                    x => x.MatchBgeUn(out iLLabel)
                ))
            {
                CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 2 failed!");
                return;
            }
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(HealthComponent), nameof(HealthComponent.body)));
            c.Emit(OpCodes.Ldarg_3);
            c.EmitDelegate(CantHeal);
            c.Emit(OpCodes.Brtrue_S, instruction1);
        }
        private static bool CantHeal(CharacterBody characterBody, bool nonRegen)
        {
            if (!nonRegen) return false;
            Inventory inventory = characterBody.inventory;
            if (!inventory || inventory.GetItemCountEffective(ImmortalPassive.itemIndex) == 0) return false;
            return true;
        }
        public static Vector3 GutsVelocity = new Vector3(0f, 9f, 0f);
        public static float ImmortalCooldownReduceOnHit = 10f;
        private static void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            DamageInfo damageInfo = obj.damageInfo;
            if (!damageInfo.attacker) return;
            if (damageInfo.HasModdedDamageType(GainGutsDamageType))
                {
                SkillLocator skillLocator = damageInfo.attacker.GetComponent<SkillLocator>();
                if (!skillLocator) return;
                GenericSkill immortalSkill = null;
                int i = 0;
                for (; i < skillLocator.allSkills.Length; i++)
                {
                    GenericSkill genericSkill = skillLocator.allSkills[i];
                    if (!genericSkill || !genericSkill.skillDef || !genericSkill.skillDef.HasRequiredStockAndDelay(genericSkill) || !(genericSkill.skillDef is PassiveItemSkillDef) || (genericSkill.skillDef as PassiveItemSkillDef).passiveItem != ImmortalPassive) continue;
                    immortalSkill = genericSkill;
                    break;
                }
                if (!immortalSkill) return;
                new VictorDamageDealtMessage(skillLocator.netIdentity, ImmortalCooldownReduceOnHit, i).Send(R2API.Networking.NetworkDestination.Clients);
            }
        }
        private static void HealthComponent_TakeDamageProcess(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            Instruction instruction1 = il.Instrs[il.Instrs.Count - 1];
            ILLabel iLLabel = null;
            if (
                !c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                    x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.SoulSurge)),
                    x => x.MatchCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff)),
                    x => x.MatchBrfalse(out iLLabel)
                ))
            {
                CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook failed!");
                return;
            }
            c.GotoLabel(iLLabel, MoveType.Before);
            Instruction instruction2 = c.Next;
            Instruction instruction = c.Emit(OpCodes.Ldarg_0).Prev;
            iLLabel.Target = instruction;
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(HealthComponent), nameof(HealthComponent.body)));
            c.EmitDelegate(CanRevive);
            //c.Emit(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Object), "op_Implicit"));
            c.Emit(OpCodes.Brfalse_S, instruction2);
            //c.Emit(OpCodes.Ldarg_0);
            //c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(HealthComponent), nameof(HealthComponent.body)));
            //c.EmitDelegate(OnRevive);
            c.Emit(OpCodes.Br_S, instruction1);
        }
        private static bool CanRevive(CharacterBody characterBody)
        {
            Inventory inventory = characterBody.inventory;
            if (!inventory) return false;
            SkillLocator skillLocator = characterBody.skillLocator;
            if (!skillLocator) return false;
            GenericSkill immortalSkill = null;
            foreach (GenericSkill genericSkill in skillLocator.allSkills)
            {
                if (!genericSkill || !genericSkill.skillDef || !genericSkill.skillDef.HasRequiredStockAndDelay(genericSkill) || !(genericSkill.skillDef is PassiveItemSkillDef) || (genericSkill.skillDef as PassiveItemSkillDef).passiveItem != ImmortalPassive) continue;
                immortalSkill = genericSkill;
                break;
            }
            if (immortalSkill) OnRevive(characterBody, immortalSkill);
            return immortalSkill;
        }
        private static void OnRevive(CharacterBody characterBody, GenericSkill genericSkill)
        {
            characterBody.healthComponent.Networkhealth = 1f;
            characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, Reviving.timeToRevive);
            genericSkill.OnExecute();
        }
        private static void OnPluginDestroyed()
        {
            CaeliImperiumPlugin.onPluginDestroyed -= OnPluginDestroyed;
            IL.RoR2.HealthComponent.TakeDamageProcess -= HealthComponent_TakeDamageProcess;
            GlobalEventManager.onServerDamageDealt -= GlobalEventManager_onServerDamageDealt;
            IL.RoR2.HealthComponent.Heal -= HealthComponent_Heal;
        }
    }
}
