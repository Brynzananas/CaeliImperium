using CaeliImperium.Items;
using RoR2;
using RoR2.ContentManagement;
using RoR2.ExpansionManagement;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaeliImperium
{
    public class CaeliImperiumContent : IContentPackProvider
    {
        internal ContentPack contentPack = new ContentPack();
        public string identifier => CaeliImperiumPlugin.ModGuid + ".ContentProvider";
        public static List<GameObject> bodies = new List<GameObject>();
        public static List<BuffDef> buffs = new List<BuffDef>();
        public static List<SkillDef> skills = new List<SkillDef>();
        public static List<SkillFamily> skillFamilies = new List<SkillFamily>();
        public static List<GameObject> projectiles = new List<GameObject>();
        public static List<GameObject> networkPrefabs = new List<GameObject>();
        public static List<SurvivorDef> survivors = new List<SurvivorDef>();
        public static List<Type> states = new List<Type>();
        public static List<NetworkSoundEventDef> sounds = new List<NetworkSoundEventDef>();
        public static List<UnlockableDef> unlockableDefs = new List<UnlockableDef>();
        public static List<GameObject> masters = new List<GameObject>();
        public static List<ItemDef> items = new List<ItemDef>();
        public static List<EquipmentDef> equipments = new List<EquipmentDef>();
        public static List<EliteDef> elites = new List<EliteDef>();
        public static List<ExpansionDef> expansions = new List<ExpansionDef>();
        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            this.contentPack.identifier = this.identifier;
            contentPack.skillDefs.Add([.. skills]);
            contentPack.skillFamilies.Add([.. skillFamilies]);
            contentPack.bodyPrefabs.Add([.. bodies]);
            contentPack.buffDefs.Add([.. buffs]);
            contentPack.projectilePrefabs.Add([.. projectiles]);
            contentPack.survivorDefs.Add([.. survivors]);
            contentPack.entityStateTypes.Add([.. states]);
            contentPack.networkSoundEventDefs.Add([.. sounds]);
            contentPack.networkedObjectPrefabs.Add([.. networkPrefabs]);
            contentPack.unlockableDefs.Add([.. unlockableDefs]);
            contentPack.masterPrefabs.Add([.. masters]);
            contentPack.itemDefs.Add([.. items]);
            contentPack.equipmentDefs.Add([.. equipments]);
            contentPack.eliteDefs.Add([.. elites]);
            contentPack.expansionDefs.Add([.. expansions]);
            yield break;
        }
        public static class Items
        {
            public static CIItemDef CritUpgradeOnKill;
            public static CIItemDef ExtraEquipmentSlot;
            public static CIItemDef FireBulletOnPrimarySkill;
            public static CIItemDef PeriodicDamageIncrease;
            public static CIItemDef StunEnemyOnItsAttack;
            public static CIItemDef ChargeAtomicBeamOnSpecialSkill;
            public static CIItemDef CopyNearbyCharactersSkillsOnDeath;
            public static CIItemDef ImproveHealingAndRegen;
            public static CIItemDef SummonMercenary;
            public static CIItemDef ShareDamageToAll;
            public static CIItemDef DuplicateMainSkills;
            public static CIItemDef WoundEnemyOnContiniousHits;
            public static CIItemDef DropHealOrbsOnContiniousHits;
            public static CIItemDef TeleportAroundOpenedChests;
            public static CIItemDef DrawSpeedPath;
            public static CIItemDef HealReceivedDamage;
            public static CIItemDef InfiniteSecondarySkillCharges;
            public static CIItemDef TransferDamageOwnership;
        }
        public static class Buffs
        {
            public static BuffDef IncreaseCritChanceAndDamage;
            public static BuffDef IncreaseDamagePereodically;
            public static BuffDef TaoPunchReady;
            public static BuffDef TaoPunchCooldown;
            public static BuffDef SpeedPathSpeedBonus;
            public static BuffDef IncreaseSecondarySkillDamage;
            public static BuffDef AffixSpeedster;
            public static BuffDef IrradiatedBuff;
            public static DotController.DotDef IrradiatedDot;
            public static DotController.DotIndex IrradiatedDotIndex;
        }
        public static class Elites
        {
            public static EliteDef HastingElite;
        }
        public static class Equipments
        {
            public static EquipmentDef Necronomicon;
            public static EquipmentDef SpeedsterEquipment;
            public static EquipmentDef DamageAllEnemies;
        }
    }
}
