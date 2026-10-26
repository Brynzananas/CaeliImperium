using BepInEx;
using BrynzaAPI;
using CaeliImperium.Components;
using CaeliImperium.Components.Doomguy;
using CaeliImperium.Components.FirstPerson;
using CaeliImperium.ModCompatibilities;
using CaeliImperium.NetworkMessages;
using CaeliImperium.ScriptableObjects;
using CaeliImperiumEntityStates.Doomguy;
using EnemiesReturns.Components;
using EntityStates;
using HarmonyLib;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using R2API;
using R2API.Networking;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using RoR2.Networking;
using RoR2.Projectile;
using RoR2.Skills;
using RoR2.UI;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static CaeliImperium.Bodies.DoomguyEvents;
using static MonoMod.InlineRT.MonoModRule;
using static UnityEngine.ParticleSystem.PlaybackState;
using static UnityEngine.UI.GridLayoutGroup;

namespace CaeliImperium.Bodies;
public static class DoomguyEvents
{
    public static GameObject BodyPrefab;
    public static CharacterBody Body;
    public static SurvivorDef Doomguy;
    public static SkillFamily Primary;
    public static SkillFamily Secondary;
    public static SkillFamily Utility;
    public static SkillFamily Special;
    public static SkillFamily Sprint;
    public static DoomguySkillDef ShotgunFire;
    public static DoomguySkillDef AcceleratorFire;
    public static DoomguySkillDef SpearSwing;
    public static DoomguySkillDef Punch;
    public static DoomguySkillDef Dash;
    public static DoomguySpecialSpearSkillDef SpearSpecial;
    public static BuffDef PrepareParriableAttackCount;
    public static BuffDef Parry;
    public static BuffDef Dodge;
    public static BuffDef Stagger;
    public static BuffDef StaggerInvincibility;
    public static BuffDef WasStaggered;
    public static BuffDef MeleeCombo;
    public static BuffDef SpearOrbitLockOn;
    public static GameObject SpearThrowProjectile;
    public static GameObject AcceleratorPlasmaProjectile;
    public static EffectDef AcceleratorPlasmaImpactEffect;
    public static EffectDef SpearThrowHitEffect;
    public static EffectDef SpearThrowHeadshotExplosionEffect;
    public static EffectDef SpearSlamExplosionEffect;
    public static EffectDef SpearSlashHitEffect;
    public static EffectDef SpearParryEffect;
    public static EffectDef AboutToAttackParryEffect;
    public static GameObject SpearCrosshair;
    public static BulletPatternDef ShotgunBulletPattern;
    public static BulletPatternDef AcceleratorBulletPattern;
    public static CIMusicTrackDef BerserkMusicTrackDef;
    public static CIMusicTrackDef TheHighCouncilMusicTrackDef;
    public static GameObject BerserkEffect;
    public static DifficultyDef HurtMePlentyDifficulty;
    public static DifficultyIndex HurtMePlentyDifficultyIndex;
    public static ModdedProcType IgnoreBishopComponentHealRestrictionProcType;
    public static DamageAPI.ModdedDamageType ParriableDamageType;
    public static DamageAPI.ModdedDamageType ParryDamageType;
    public static DamageAPI.ModdedDamageType ParriableMeleeDamageType;
    public static DamageAPI.ModdedDamageType GloryKillDamageType;
    public static DamageAPI.ModdedDamageType SuperStunDamageType;
    public static DamageAPI.ModdedDamageType BerserkedDamageType;
    public static CharacterBodyAPI.ModdedBodyFlag CanBeBerserkedBodyFlag;
    public static CharacterBodyAPI.ModdedBodyFlag DoomguyBodyFlag;
    public static CharacterBodyAPI.ModdedBodyFlag HeavyMonsterBodyFlag;
    public static CharacterBodyAPI.ModdedBodyFlag FodderMonsterBodyFlag;
    public static DamageColorIndex ParriableDamageColor;
    public static float HealFromGloryChampionKillCoeffecient =  1f / 4f;
    public static float HealFromGloryKillCoeffecient = 1f / 32f;
    public static float HealFromGloryHeavyKillCoeffecient = 1f / 8f;
    public static float HealFromWhiffedGloryKillCoeffecient = 0f;
    public static float ReserveHealFromWhiffedGloryKillCoeffecient = 0f;
    public static float ReserveHealFromGloryKillCoeffecient = 0.1f;
    public static float ReserveHealFromGloryHeavyKillCoeffecient = 0.25f;
    public static float ReserveHealFromGloryChampionKillCoeffecient = 0.5f;
    public static float DefaultOnParryStunDuration = 0.6f;  
    public static int StaggerAmount = 3;
    public static int StaggerAmountForFinalBosses = 5;
    public static float StaggerDuration = 6f;
    public static float StaggerInvincibilityDuration = 0.5f;
    public static float WasStaggeredDuration = 80f;
    public static float MeleeComboDuration = 1f;
    public static Gradient ParriableGradient = GetParriableGradient();
    public static Gradient NonParriableGradient = GetNonParriableGradient();
    public static float GlobalProjectileSpeedMultiplier = 0.65f;
    public static float SuperStunDuration = 2f;
    public static float ParriableOverlapAttackSizeMultiplier = 2f;
    public static float ParriableBlastAttackSizeMultiplier = 2f;
    public static float GloryKillForce = 48f;
    public static float StageTextureSaturationMultiplier = 0.3f;
    public static float StageTextureValueMultiplier = 0.85f;
    public static float StageTextureSizeMultiplier = 2f;
    public static float StageLightSaturationMultiplier = 0.3f;
    public static float StageLightValueMultiplier = 0.9f;
    public static float StageRampFogSaturationMultiplier = 1f;
    public static float StageRampFogValueMultiplier = 0.3f;
    public static float StageAmbientSaturationMultiplier = 0.2f;
    public static float StageAmbientValueMultiplier = 0.8f;
    public static float StageSkyboxSaturationMultiplier = 0.2f;
    public static float StageSkyboxValueMultiplier = 0.8f;
    public static float BerserkDurationOnActivatingHalcyoniteShrine = 81f * 2f - 13.4f;
    public static int BeginBerserkOnActivatingHalcyoniteShrineAfterStage = 3;
    public static float FlyingEnemiesAboveGround = 8f;
    public static float FlyingEnemiesAboveGroundRaycastLength = 64f;
    public static float FlyingEnemiesAboveGroundOnFail = 6f;
    public static float FlyingEnemiesBelowCeilingRaycastLength = 6f;
    public static float FlyingEnemiesBelowCeiling = 4f;
    public static float FlyingEnemiesUpdateInterval = 0.1f;
    public static float FlyingEnemiesMaxMoveSpeed = 24f;
    public static float HeavyHealthMultiplier = 1.6f;
    public static float BossHealthMultiplier = 2.2f;
    public static float ChampRushDurationMultiplier = 3f;
    public static float OverrideAimAssist = 10f;
    public static event Action<OnParryInfo> onParry;
    private static HashSet<GameObject> _parriableProjectileGhosts = [];
    internal static bool init;
    private static List<ILHook> _hooks = new List<ILHook>();
    private static Dictionary<MethodBase, int> keyValuePairs = [];
    private static FixedConditionalWeakTable<CharacterModel, TemporaryOverlayInstance> keyValuePairs2 = [];
    private static FixedConditionalWeakTable<CharacterModel, TemporaryOverlayInstance> keyValuePairs6 = [];
    private static FixedConditionalWeakTable<CharacterBody, TemporaryVisualEffect> keyValuePairs7 = [];
    private static FixedConditionalWeakTable<CharacterBody, HashSet<BaseState>> keyValuePairs3 = [];
    public delegate void OnBodyParried(DamageReport damageReport, BaseState baseState);
    private static Dictionary<Type, OnBodyParried> keyValuePairs4 = [];
    private static Dictionary<Type, ParriableStateInfo> parriableStatesTypes = [];
    private static HashSet<BaseState> keyValuePairs5 = [];
    private static Dictionary<Type, ParriableFireProjectileInfo> types = [];
    private static HashSet<Type> patchedProjectileStates = [];
    private static HashSet<Type> patchedOverlapStates = [];
    private static HashSet<Type> patchedBulletStates = [];
    private static HashSet<Type> patchedBlastStates = [];
    private static HashSet<Type> types3 = [];
    private static Dictionary<ProjectileController, float> customProjectileSpeedMultiplier = [];
    private static Dictionary<int, float> runtimeCustomProjectileSpeedMultiplier = [];
    private static bool projectilesCatalogInit;
    private static HashSet<string> purgeMonstersAndInteractables = new HashSet<string> { "BeetleMaster", "WispMaster" ,"GipMaster", "GeepMaster", "GupMaster", "JellyfishMaster", "AcidLarvaMaster", "MegaConstructMaster", "MagmaWormMaster", "ElectricWormMaster" , "HermitCrabMaster", "PipelineRefinery", "VoidChest", "ShrineRebirth", "Shrine of Ruin", "ProvShrine", "Duplicator", "DuplicatorLarge", "DuplicatorMilitary", "DuplicatorWild", "Scrapper", "repairStation", "ShrinePlanet", "Barrel1", "DroneScrapper" };
    private static HashSet<string> purgeItems = new HashSet<string> { "Clover", "StunChanceOnHit", "Mushroom", "SecondarySkillMagazine", "FlatHealth", "Firework", "HealingPotion", "GoldOnHurt", "WardOnLevel", "Missile", "DronesDropDynamite", "BonusGoldPackOnKill", "ExecuteLowHealthElite", "Phasing", "ExtraStatsOnLevelUp", "StrengthenBurn", "LowerPricedChests", "FreeChest", "Thorns", "SprintOutOfCombat", "RegeneratingScrap", "Squid", "ExtraShrineItem", "ChainLightning", "RandomEquipmentTrigger", "StunAndPierce", "GhostOnKill", "PhysicsProjectile", "MoreMissile", "MeteorAttackOnHighDamage", "ItemDropChanceOnKill", "DroneWeapons", "ShockNearby", "Icicle", "HeadHunter", "BarrageOnBoss", "CritGlassesVoid", "ChainLightningVoid", "ArmedBackpack", "Diary", "Malice", "UniversalCharger", "X4", "UraniumHorseshoe", "CrypticSource", "LowQualitySpeakers", "LightningOnKill", "JetBoots", "WatchMetronome", "DroidHead", "ErraticGadget", "GalvanicCore", "NkotasHeritage", "PortableReactor", "SwiftSkateboard", "IceTool", "BlastKnuckles", "PoisonousGland", "InfiniteSecondarySkillCharges", "ITEM_SANDSWEPT_AMBER_KNIFE", "ITEM_SANDSWEPT_TEMPORAL_TRANSISTOR", "ITEM_SANDSWEPT_FRACTURED_TIMEPIECE", "ITEM_SANDSWEPT_CROWNS_DIAMOND", "ITEM_SANDSWEPT_SUN_FRAGMENT", "ITEM_SANDSWEPT_DRIFTING_PERCEPTION", "ITEM_SANDSWEPT_NUCLEAR_SALVO", "ITEM_SANDSWEPT_UNIVERSAL_VIP_PASS", "ITEM_SANDSWEPT_TORN_FEATHER", "ITEM_SANDSWEPT_GLACIAL_PLASMA", "HermitsShawl", "SecondHand", "SpaciousUmbrella", "TargetingMatrix", "RadiantTimepiece", "ITEM_CALENDAR", "ITEM_ARSON", "ITEM_BloodMask", "ITEM_BLUELEMURIAN", "ITEM_INFINITYCODEX", "ProcBoost", "GlassShield", "FlatHealingIncrease", "HealOnCooldown", "DebuffBoss", "GoldStar", "UpgradeChests", "DropYellowItemOnKill", "HealOnLevelUp", "RuneChance", "RevengeDamageBonus", "Placebo", "FrenzyOnBossKill", "ITEM_AOEONCRIT", "ITEM_CLOUDBOTTLE", "ITEM_FARTBOTTLE", "ITEM_DISCOUNTCARDNEW", "ITEM_BOSSITEMCONSUMABLE", "ITEM_BORBOFUSE", "ITEM_GAMMAKNIF", "ITEM_BORBODISCOUNT", "ITEM_KALEIDOSKULL", "ITEM_LIGHTNINGATTRACTOR", "ITEM_MAGICQUIVER", "ITEM_GOODEXECUTIONITEM", "ITEM_PHOTOGRAPH", "BlastBoot", "itembarriercrit", "itemenigmatickeycard", "EXPEDITION_PAPERDOG", "EXPEDITION_NITROGEN", "EXPEDITION_MRE", "EXPEDITION_TRASHBOX", "EXPEDITION_GEOLOCATOR", "EXPEDITION_SMILINGPLANT", "EXPEDITION_REDGUITAR", "EXPEDITION_COMPASS" };
    private static HashSet<string> keepItems = new HashSet<string> { "BossDamageBonus", "AttackSpeedPerNearbyAllyOrEnemy", "HealWhileSafe", "IncreaseDamageOnMultiKill", "BarrierOnCooldown", "SpeedBoostPickup", "NearbyDamageBonus", "Crowbar", "IgniteOnKill", "CritAtLowerElevation", "CritGlasses", "Medkit", "AttackSpeedAndMoveSpeed", "Tooth", "OutOfCombatArmor", "PersonalShield", "ArmorPlate", "Syringe", "StickyBomb", "BarrierOnKill", "Bear", "BleedOnHit", "DelayedDamage", "Bandolier", "WarCryOnMultiKill", "KnockBackHitEnemies", "SlowOnHit", "SpeedOnPickup", "DeathMark", "Feather", "Infusion", "ShieldBooster", "FireRing", "IncreasePrimaryDamage", "DrawSpeedPath", "HealReceivedDamage", "TriggerEnemyDebuffs", "AttackSpeedOnCrit", "SprintArmor", "IceRing", "TeleportOnLowHealth", "EnergizedOnEquipmentUse", "JumpBoost", "ExplodeOnDeath", "AlienHead", "KillEliteFrenzy", "Behemoth", "BoostAllStats", "UtilitySkillMagazine", "Plant", "CritDamage", "NovaOnHeal", "SharedSuffering", "IncreaseHealing", "LaserTurbine", "BounceNearby", "ArmorReductionOnHit", "Talisman", "PermanentDebuffOnHit", "BloodTester", "CoffeeBag", "DetritiveTrematode", "Fork", "GuardingAmulet", "MoltenCoin", "Needles", "HuntersSigil", "StrangeCan", "GreenChocolate", "Insecticide", "ITEM_SANDSWEPT_RED_SPRING_WATER", "ITEM_SANDSWEPT_CEREMONIAL_JAR", "ITEM_SANDSWEPT_POCKET_PLUTONIUM", "ITEM_SANDSWEPT_SACRIFICIAL_BAND", "ITEM_SANDSWEPT_SMOULDERING_DOCUMENT", "EXPEDITION_FEATHER", "EXPEDITION_SHITTYCAPE", "EXPEDITION_BIODUDE", "SlipperyGround", "LowerHealing", "HexHaunting", "RoamingItemDisable", "RoamingItemDisable", "Mimicry", "HexCurse", "OutOfTPRadius", "IncreasedDifficulty", "HexMountain", "DreamDemon", "SanguineChalice", "GhostBlades", "VengefulSpirit", "VileCenser", "PageClarity", "PagePurify", "PageDie", "PageTeamInvisibility", "PageWarpGate" };
    private static HashSet<string> purgeEquipments = new HashSet<string> { "MIDAS", "Magnet", "WhiteFlag", "Parry", "CommandMissile", "MultiShopCard", "GummyClone", "Blackhole", "Recycle", "DroneBackup", "GoldGat", "BossHunter", "GALVANIC_CELL_SHIELD", "ImpulseFrostShield", "DeathImmunity", "SlowTime", "FireBurstOnFreeze", "EQUIPMENT_NINJAGEAR", "EQUIPMENT_QUICKSTARTEQUIPMENT", "EQUIPMENT_THEMACHINE", "EQUIPMENT_BROKENZAPINATOR" };
    private static HashSet<string> keepEquipments = ["Cleanse", "Gateway", "Fruit", "PassiveHealing", "TeamWarCry", "GainArmor", "Jetpack", "Molotov", "CritOnUse", "BFG", "VendingMachine", "Lightning", "Saw", "LifestealOnHit", "FireBallDash", "BackThruster", "CloakingHeadband", "GreaterWarbanner", "EQUIPMENT_SANDSWEPT_THE_SAND_SWEEPER", "EXPEDITION_TERRAFORMER"];
    private static HashSet<string> purgeClankers = new HashSet<string> { "Drone1", "FlameDrone", "MissileDrone", "HaulerDrone", "JunkDrone", "MegaDrone", "Turret1", "CopycatDrone", "EquipmentDrone", "BombardmentDrone", "ddVoltaicDroneBody", "ddInfernoDroneBody", "ddCloneDrone", "ddDroidDrone", "ddShockDrone", "ER_EnemiesReturnsMechanicalSpiderDrone" };
    private static HashSet<string> keepClankers = ["RechargeDrone", "CleanupDrone", "Drone2", "JailerDrone", "EmergencyDrone"];
    private static HashSet<ItemTier> bannedItemTiers = new HashSet<ItemTier> { ItemTier.VoidTier1, ItemTier.Lunar, ItemTier.VoidTier2, ItemTier.VoidTier3, ItemTier.VoidBoss };
    private static HashSet<ItemTierDef> bannedItemTierDefs = [CaeliImperiumAssets.VoidTier1, CaeliImperiumAssets.VoidTier2, CaeliImperiumAssets.VoidTier3, CaeliImperiumAssets.Lunar];
    public static void Init(GameObject gameObject)
    {
        On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
        GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
        On.EntityStates.EntityState.OnEnter += EntityState_OnEnter;
        On.EntityStates.EntityState.OnExit += EntityState_OnExit;
        On.EntityStates.EntityState.FixedUpdate += EntityState_FixedUpdate;
        On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
        On.EntityStates.Bell.BellWeapon.ChargeTrioBomb.OnEnter += ChargeTrioBomb_OnEnter;
        /*On.EntityStates.LemurianBruiserMonster.FireMegaFireball.OnEnter += FireMegaFireball_OnEnter;
        On.EntityStates.LemurianBruiserMonster.FireMegaFireball.OnExit += FireMegaFireball_OnExit;*/
        IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess1;
        On.RoR2.Projectile.ProjectileManager.InitializeProjectile += ProjectileManager_InitializeProjectile;
        IL.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
        On.RoR2.DirectorCard.IsAvailable += DirectorCard_IsAvailable;
        IL.RoR2.BlastAttack.CollectHits += BlastAttack_CollectHits;
        On.RoR2.BodyCatalog.SetBodyPrefabs += BodyCatalog_SetBodyPrefabs;
        On.RoR2.SetStateOnHurt.GetShouldHitStun += SetStateOnHurt_GetShouldHitStun;
        IL.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += BaseAI_FindEnemyHurtBox;
        On.RoR2.Run.Start += Run_Start;
        On.RoR2.CharacterAI.BaseAI.OnBodyDamaged += BaseAI_OnBodyDamaged;
        On.RoR2.CharacterAI.BaseAI.OnServerDamageDealt += BaseAI_OnServerDamageDealt;
        Run.onRunSetRuleBookGlobal += Run_onRunSetRuleBookGlobal;
        CaeliImperiumHooks.OnSetProjectilePrefabsIndividualPrefab += CaeliImperiumHooks_OnSetProjectilePrefabsIndividualPrefab;
        On.RoR2.HealthComponent.GetHealthBarValues += HealthComponent_GetHealthBarValues;
        On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += CharacterBody_UpdateAllTemporaryVisualEffects;
        On.EntityStates.ShrineHalcyonite.ShrineHalcyoniteActivatedState.OnEnter += ShrineHalcyoniteActivatedState_OnEnter;
        On.EntityStates.ShrineHalcyonite.ShrineHalcyoniteFinished.OnEnter += ShrineHalcyoniteFinished_OnEnter;
        //SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        On.RoR2.Inventory.GetItemCountEffective_ItemIndex += Inventory_GetItemCountEffective_ItemIndex;
        Stage.onStageStartGlobal += Stage_onStageStartGlobal;
        //On.RoR2.SpawnCard.DoSpawn += SpawnCard_DoSpawn;
        On.RoR2.SceneInfo.Awake += SceneInfo_Awake;
        On.RoR2.HealthComponent.TakeDamageForce_PhysForceInfo += HealthComponent_TakeDamageForce_PhysForceInfo;
        On.RoR2.SetStateOnHurt.OnTakeDamageServer += SetStateOnHurt_OnTakeDamageServer;
        On.RoR2.SetStateOnHurt.SetFrozen += SetStateOnHurt_SetFrozen;
        On.RoR2.SetStateOnHurt.SetShock += SetStateOnHurt_SetShock;
        On.RoR2.SetStateOnHurt.SetStun += SetStateOnHurt_SetStun;
        On.RoR2.SetStateOnHurt.SetTaunt += SetStateOnHurt_SetTaunt;
        On.RoR2.SetStateOnHurt.SetPain += SetStateOnHurt_SetPain;
        On.RoR2.SetStateOnHurt.SetImmobilize += SetStateOnHurt_SetImmobilize;
        On.RoR2.CombatDirector.EliteTierDef.CanSelect += EliteTierDef_CanSelect;
        On.RoR2.PickupDropletController.CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 += PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3;
        MusicController.pickTrackHook += MusicController_pickTrackHook;
        R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        IL.EntityStates.ChampRush.SpawnOrb.Ascending.OnEnter += Ascending_OnEnter;
        IL.EntityStates.Missions.ChampRush.QuadrantPhase.OnQuadrantComplete += QuadrantPhase_OnQuadrantComplete;
        On.RoR2.TrajectoryAimAssist.GetAimAssistRadius += TrajectoryAimAssist_GetAimAssistRadius;
        if (init) return;
        init = true;
        Body = gameObject.GetComponent<CharacterBody>();
        Body.ApplyFootstepDustPrefab(BaseFootstepDustType.Generic);
        Body.AddModdedBodyFlag(CharacterBodyAPI.AlwaysSprint);
        SkillLocator skillLocator = gameObject.GetComponent<SkillLocator>();
        DoomguyComponent bishopComponent = gameObject.GetComponent<DoomguyComponent>();
        skillLocator.SetSprintSkill(bishopComponent.dashSkill);
        FirstPersonCameraTargetParams firstPersonCameraTargetParams = gameObject.GetComponent<FirstPersonCameraTargetParams>();
        FirstPersonCharacterCameraParams firstPersonCharacterCameraParams = firstPersonCameraTargetParams.firstPersonCharacterCameraParams;
        FirstPersonCameraController firstPersonCameraController = firstPersonCharacterCameraParams.firstPersonCameraController;
        Transform camera = firstPersonCameraController.transform.Find("Camera");
        PostProcessLayer postProcessLayer = camera.GetComponent<PostProcessLayer>();
        PostProcessLayer postProcessLayer2 = CaeliImperiumAssets.MainCamera.GetComponentInChildren<PostProcessLayer>();
        postProcessLayer.m_Resources = postProcessLayer2.m_Resources;
        postProcessLayer.enabled = false;
        HurtMePlentyDifficulty = CaeliImperiumUtils.CreateDifficulty(1.5f, "CI_DIFFICULTY_HURTMEPLENTY_NAME", "CI_DIFFICULTY_HURTMEPLENTY_DESC", CaeliImperiumAssets.assetBundle.LoadAsset<Sprite>("Assets/CaeliImperium/Textures/hurtmeplentydifficon.png"), ColorCatalog.GetColor(ColorCatalog.ColorIndex.NormalDifficulty), "cihmp", false, out HurtMePlentyDifficultyIndex);
        Doomguy = CaeliImperiumAssets.assetBundle.LoadAsset<SurvivorDef>("Assets/CaeliImperium/Bodies/Doomguy/CIDoomguy.asset").RegisterSurvivor();
        Primary = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Doomguy/CIsfDoomguyPrimary.asset").RegisterSkillFamily();
        Secondary = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Doomguy/CIsfDoomguyUtility.asset").RegisterSkillFamily();
        Utility = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Doomguy/CIsfDoomguySecondary.asset").RegisterSkillFamily();
        Special = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Doomguy/CIsfDoomguySpecial.asset").RegisterSkillFamily();
        Sprint = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Doomguy/CIsfDoomguySprint.asset").RegisterSkillFamily();
        ShotgunFire = CaeliImperiumAssets.assetBundle.LoadAsset<DoomguySkillDef>("Assets/CaeliImperium/Bodies/Doomguy/CIsdDoomguyShotgunFire.asset").RegisterSkillDef();
        AcceleratorFire = CaeliImperiumAssets.assetBundle.LoadAsset<DoomguySkillDef>("Assets/CaeliImperium/Bodies/Doomguy/CIsdDoomguyAcceleratorFire.asset").RegisterSkillDef();
        SpearSwing = CaeliImperiumAssets.assetBundle.LoadAsset<DoomguySkillDef>("Assets/CaeliImperium/Bodies/Doomguy/CIsdDoomguySpearSwing.asset").RegisterSkillDef();
        Punch = CaeliImperiumAssets.assetBundle.LoadAsset<DoomguySkillDef>("Assets/CaeliImperium/Bodies/Doomguy/CIsdDoomguyPunch.asset").RegisterSkillDef();
        Dash = CaeliImperiumAssets.assetBundle.LoadAsset<DoomguySkillDef>("Assets/CaeliImperium/Bodies/Doomguy/CIsdDoomguyDash.asset").RegisterSkillDef();
        SpearSpecial = CaeliImperiumAssets.assetBundle.LoadAsset<DoomguySpecialSpearSkillDef>("Assets/CaeliImperium/Bodies/Doomguy/CIsdDoomguySpearSpecial.asset").RegisterSkillDef();
        PrepareParriableAttackCount = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Doomguy/CIbdPrepareParriableAttackCount.asset").RegisterBuffDef();
        Parry = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Doomguy/CIbdParry.asset").RegisterBuffDef();
        Dodge = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Doomguy/CIbdDodge.asset").RegisterBuffDef();
        SpearOrbitLockOn = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Doomguy/CIbdSpearOrbitLockOn.asset").RegisterBuffDef();
        Stagger = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Doomguy/CIbdStagger.asset").RegisterBuffDef();
        StaggerInvincibility = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Doomguy/CIbdStaggerInvincibility.asset").RegisterBuffDef();
        WasStaggered = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Doomguy/CIbdWasStaggered.asset").RegisterBuffDef();
        MeleeCombo = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Doomguy/CIbdMeleeCombo.asset").RegisterBuffDef();
        SpearThrowProjectile = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Doomguy/CIDoomguySpearThrowProjectile.prefab").RegisterProjectile();
        AcceleratorPlasmaProjectile = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Doomguy/Weapons/Accelerator/CIAcceleratorPlasmaProjectile.prefab").RegisterProjectile();
        SpearThrowHeadshotExplosionEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Doomguy/Effects/SpearThrowHeadshotHitVFX.prefab").RegisterEffect();
        SpearThrowHitEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Doomguy/Effects/SpearThrowHitVFX.prefab").RegisterEffect();
        AcceleratorPlasmaImpactEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Doomguy/Effects/AcceleratorPlasmaImpact.prefab").RegisterEffect();
        SpearSlashHitEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Doomguy/Effects/SpearSlashHitVFX.prefab").RegisterEffect();
        SpearSlamExplosionEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Doomguy/Effects/SpearSlamVFX.prefab").RegisterEffect();
        SpearParryEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Doomguy/Effects/SpearParryVFX.prefab").RegisterEffect();
        AboutToAttackParryEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Doomguy/Effects/AboutToAttackParriable.prefab").RegisterEffect();
        SpearCrosshair = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Doomguy/DoomguySpearInputsCrosshair.prefab");
        ShotgunBulletPattern = CaeliImperiumAssets.assetBundle.LoadAsset<BulletPatternDef>("Assets/CaeliImperium/Bodies/Doomguy/Weapons/Shotgun/bpdShotgun.asset");
        AcceleratorBulletPattern = CaeliImperiumAssets.assetBundle.LoadAsset<BulletPatternDef>("Assets/CaeliImperium/Bodies/Doomguy/Weapons/Accelerator/bpdAccelerator.asset");
        BerserkMusicTrackDef = CaeliImperiumAssets.assetBundle.LoadAsset<CIMusicTrackDef>("Assets/CaeliImperium/Bodies/Doomguy/cmtdCIBerserk.asset");
        TheHighCouncilMusicTrackDef = CaeliImperiumAssets.assetBundle.LoadAsset<CIMusicTrackDef>("Assets/CaeliImperium/Bodies/Doomguy/cmtdCITheHighCouncil.asset");
        BerserkEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Doomguy/BerserkEffect.prefab");
        SniperTargetViewer sniperTargetViewer = SpearCrosshair.AddComponent<SniperTargetViewer>();
        sniperTargetViewer.visualizerPrefab = CaeliImperiumAssets.LightSniperTargetVisualizer;
        IgnoreBishopComponentHealRestrictionProcType = ProcTypeAPI.ReserveProcType();
        ParriableDamageType = DamageAPI.ReserveDamageType();
        ParryDamageType = DamageAPI.ReserveDamageType();
        GloryKillDamageType = DamageAPI.ReserveDamageType();
        SuperStunDamageType = DamageAPI.ReserveDamageType();
        ParriableMeleeDamageType = DamageAPI.ReserveDamageType();
        BerserkedDamageType = DamageAPI.ReserveDamageType();
        HeavyMonsterBodyFlag = CharacterBodyAPI.ReserveBodyFlag();
        FodderMonsterBodyFlag = CharacterBodyAPI.ReserveBodyFlag();
        CanBeBerserkedBodyFlag = CharacterBodyAPI.ReserveBodyFlag();
        Body.AddModdedBodyFlag(CanBeBerserkedBodyFlag);
        DoomguyBodyFlag = CharacterBodyAPI.ReserveBodyFlag();
        Body.AddModdedBodyFlag(DoomguyBodyFlag);
        ParriableDamageColor = ColorsAPI.RegisterDamageColor(new Color(0f, 1f, 0.4f));
        R2API.Networking.NetworkingAPI.RegisterMessageType<DoomguyRechargeSpecialSpearSkillsNetMessage>();
        typeof(Dash).RegisterEntityState();
        typeof(Punch).RegisterEntityState();
        typeof(ShootShotgun).RegisterEntityState();
        typeof(SwingSpear).RegisterEntityState();
        typeof(PrepareSpearSpecial).RegisterEntityState();
        typeof(SpearOrbitThrow).RegisterEntityState();
        typeof(SpearSlam).RegisterEntityState();
        typeof(SpearThrow).RegisterEntityState();
        typeof(SpearOrbit).RegisterEntityState();
        typeof(ShootAccelerator).RegisterEntityState();
        typeof(ExitAccelerator).RegisterEntityState();
        AddParriableProjectileGhost(CaeliImperiumAssets.LemurianFireballGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.LemurianBruiserMegaFireballGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.VultureWindbladeProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.MiniMushroomSporeGrenadeGhost);
        SetCustomProjectileSpeedMultiplier(CaeliImperiumAssets.MiniMushroomSporeGrenadeProjectile, 1f);
        AddParriableProjectileGhost(CaeliImperiumAssets.FlyingVerminSpitProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.MinorConstructProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.BellBallProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.BellBallSmallProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.GreaterWispProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.HermitCrabBombGhost);
        SetCustomProjectileSpeedMultiplier(CaeliImperiumAssets.HermitCrabBombProjectile, 1f);
        AddParriableProjectileGhost(CaeliImperiumAssets.HermitCrabBombOptGhost);
        SetCustomProjectileSpeedMultiplier(CaeliImperiumAssets.HermitCrabBombOptProjectile, 1f);
        AddParriableProjectileGhost(CaeliImperiumAssets.BeetleQueenSpitProejctileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.ClayBossTarBallProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.GrandparentBossBoulderProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.GrandparentBossMiniBoulderProjectileGhost);
        PatchProjectileStateToParriable(typeof(EntityStates.LemurianMonster.FireFireball), nameof(EntityStates.LemurianMonster.FireFireball.OnEnter), 3);
        AddParriableStateVisuals<EntityStates.LemurianMonster.ChargeFireball>(3);
        PatchProjectileStateToParriable(typeof(EntityStates.Vulture.Weapon.FireWindblade), nameof(EntityStates.Vulture.Weapon.FireWindblade.OnEnter), 3);
        AddParriableStateVisuals<EntityStates.Vulture.Weapon.ChargeWindblade>(3);
        PatchProjectileStateToParriable(typeof(EntityStates.MiniMushroom.SporeGrenade), nameof(EntityStates.MiniMushroom.SporeGrenade.FireGrenade), 0);
        //AddParriableStateVisuals<EntityStates.MiniMushroom.SporeGrenade>();
        PatchProjectileStateToParriable(typeof(EntityStates.GenericProjectileBaseState), nameof(EntityStates.GenericProjectileBaseState.FireProjectile), typeof(EntityStates.MinorConstruct.Weapon.FireConstructBeam), 3);
        AddParriableStateVisuals<EntityStates.MinorConstruct.Weapon.ChargeConstructBeam>(3);
        PatchProjectileStateToParriable(typeof(EntityStates.FlyingVermin.Weapon.Spit), nameof(EntityStates.FlyingVermin.Weapon.Spit.FireProjectile), 3);
        AddParriableStateVisuals<EntityStates.FlyingVermin.Weapon.Spit>(3);
        PatchProjectileStateToParriable(typeof(EntityStates.Bell.BellWeapon.ChargeTrioBomb), nameof(EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate), 3);
        AddParriableStateVisuals<EntityStates.Bell.BellWeapon.ChargeTrioBomb>(3);
        PatchProjectileStateToParriable(typeof(EntityStates.LemurianBruiserMonster.FireMegaFireball), nameof(EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate), CustomLemurianBruiserParriableProjectile);
        AddParriableStateVisuals<EntityStates.LemurianBruiserMonster.ChargeMegaFireball>();
        PatchProjectileStateToParriable(typeof(EntityStates.GreaterWispMonster.FireCannons), nameof(EntityStates.GreaterWispMonster.FireCannons.OnEnter), 0);
        AddParriableStateVisuals<EntityStates.GreaterWispMonster.ChargeCannons>();
        PatchProjectileStateToParriable(typeof(EntityStates.GrandParentBoss.FireSecondaryProjectile), nameof(EntityStates.GrandParentBoss.FireSecondaryProjectile.Fire), 0);
        AddParriableStateVisuals<EntityStates.GrandParentBoss.FireSecondaryProjectile>();
        PatchProjectileStateToParriable(typeof(EntityStates.ClayBoss.FireTarball), nameof(EntityStates.ClayBoss.FireTarball.FireSingleTarball), 1);
        AddParriableStateVisuals<EntityStates.ClayBoss.PrepTarBall>(1);
        PatchProjectileStateToParriable(typeof(EntityStates.BeetleQueenMonster.FireSpit), nameof(EntityStates.BeetleQueenMonster.FireSpit.FireBlob), 0);
        AddParriableStateVisuals<EntityStates.BeetleQueenMonster.ChargeSpit>();
        AddParriableStateVisuals<EntityStates.RoboBallBoss.Weapon.ChargeEyeblast>();
        AddParriableStateVisuals<EntityStates.RoboBallBoss.Weapon.ChargeSuperEyeblast>();
        PatchProjectileStateToParriable(typeof(EntityStates.RoboBallBoss.Weapon.FireEyeBlast), nameof(EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate), CustomRoboBallBossParriableProjectile);
        AddParriableStateVisuals<EntityStates.LemurianBruiserMonster.ChargeMegaFireball>();
        PatchProjectileStateToParriable(typeof(EntityStates.HermitCrab.FireMortar), nameof(EntityStates.HermitCrab.FireMortar.Fire), 0);
        //AddParriableStateVisuals<EntityStates.HermitCrab.FireMortar>();
        AddParriableStateVisuals<EntityStates.Bison.PrepCharge>();
        PatchOverlapStateToParriable(typeof(EntityStates.Bison.Charge), nameof(EntityStates.Bison.Charge.FixedUpdate));
        AddParriableStateVisuals<EntityStates.LemurianMonster.Bite>();
        PatchOverlapStateToParriable(typeof(EntityStates.LemurianMonster.Bite), nameof(EntityStates.LemurianMonster.Bite.FixedUpdate));
        AddParriableStateVisuals<EntityStates.WorkerUnit.WindUpDrillDash>();
        PatchOverlapStateToParriable(typeof(EntityStates.WorkerUnit.FireDrillDash), nameof(EntityStates.WorkerUnit.FireDrillDash.AttackUpdate));
        AddParriableStateVisuals<EntityStates.BeetleMonster.HeadbuttState>();
        PatchOverlapStateToParriable(typeof(EntityStates.BeetleMonster.HeadbuttState), nameof(EntityStates.BeetleMonster.HeadbuttState.FixedUpdate));
        AddParriableStateVisuals<EntityStates.BeetleGuardMonster.GroundSlam>();
        PatchOverlapStateToParriable(typeof(EntityStates.BeetleGuardMonster.GroundSlam), nameof(EntityStates.BeetleGuardMonster.GroundSlam.FixedUpdate));
        AddParriableStateVisuals<EntityStates.Vermin.Weapon.TongueLash>();
        PatchOverlapStateToParriable(typeof(EntityStates.BasicMeleeAttack), nameof(EntityStates.BasicMeleeAttack.AuthorityFireAttack), typeof(EntityStates.Vermin.Weapon.TongueLash));
        AddParriableStateVisuals<EntityStates.Gup.GupSpikesState>();
        PatchOverlapStateToParriable(typeof(EntityStates.BasicMeleeAttack), nameof(EntityStates.BasicMeleeAttack.AuthorityFireAttack), typeof(EntityStates.Gup.GupSpikesState));
        AddParriableStateVisuals<EntityStates.ImpMonster.DoubleSlash>();
        PatchOverlapStateToParriable(typeof(EntityStates.ImpMonster.DoubleSlash), nameof(EntityStates.ImpMonster.DoubleSlash.HandleSlash));
        AddParriableStateVisuals<EntityStates.ImpMonster.DoubleSlash>();
        PatchOverlapStateToParriable(typeof(EntityStates.Lynx2Monster.TwoSwipes), nameof(EntityStates.Lynx2Monster.TwoSwipes.FixedUpdate));
        AddParriableStateVisuals<EntityStates.Lynx2Monster.TwoSwipes>();
        PatchOverlapStateToParriable(typeof(EntityStates.LynxAmbusher.SpearStrike), nameof(EntityStates.LynxAmbusher.SpearStrike.FireSpearStrike));
        PatchOverlapStateToParriable(typeof(EntityStates.LynxAmbusher.SpearStrikeStationary), nameof(EntityStates.LynxAmbusher.SpearStrikeStationary.FireSpearStrike));
        AddParriableStateVisuals<EntityStates.LynxAmbusher.SpearStrikeWindup>();
        PatchOverlapStateToParriable(typeof(EntityStates.BasicMeleeAttack), nameof(EntityStates.BasicMeleeAttack.AuthorityFireAttack), typeof(EntityStates.ImpMonster.DoubleSlash));
        AddParriableStateVisuals<EntityStates.GrandParentBoss.GroundSwipe>();
        PatchOverlapStateToParriable(typeof(EntityStates.BasicMeleeAttack), nameof(EntityStates.BasicMeleeAttack.AuthorityFireAttack), typeof(EntityStates.GrandParentBoss.GroundSwipe));
        AddParriableStateVisuals<EntityStates.BrotherMonster.SprintBash>();
        PatchOverlapStateToParriable(typeof(EntityStates.BasicMeleeAttack), nameof(EntityStates.BasicMeleeAttack.AuthorityFireAttack), typeof(EntityStates.BrotherMonster.SprintBash));
        AddParriableStateVisuals<EntityStates.Halcyonite.GoldenSlash>();
        PatchOverlapStateToParriable(typeof(EntityStates.BasicMeleeAttack), nameof(EntityStates.BasicMeleeAttack.AuthorityFireAttack), typeof(EntityStates.Halcyonite.GoldenSlash));
        AddParriableStateVisuals<EntityStates.Halcyonite.GoldenSwipe>();
        PatchOverlapStateToParriable(typeof(EntityStates.BasicMeleeAttack), nameof(EntityStates.BasicMeleeAttack.AuthorityFireAttack), typeof(EntityStates.Halcyonite.GoldenSwipe));
        AddParriableStateVisuals<EntityStates.BrotherMonster.WeaponSlam>();
        PatchOverlapStateToParriable(typeof(EntityStates.BrotherMonster.WeaponSlam), nameof(EntityStates.BrotherMonster.WeaponSlam.FixedUpdate));
        PatchBlastAttackStateToParriable(typeof(EntityStates.BrotherMonster.WeaponSlam), nameof(EntityStates.BrotherMonster.WeaponSlam.FixedUpdate));
        AddParriableStateVisuals<EntityStates.GolemMonster.ClapState>();
        PatchBlastAttackStateToParriable(typeof(EntityStates.GolemMonster.ClapState), nameof(EntityStates.GolemMonster.ClapState.FixedUpdate));
        AddParriableStateVisuals<EntityStates.ParentMonster.GroundSlam>();
        PatchBlastAttackStateToParriable(typeof(EntityStates.ParentMonster.GroundSlam), nameof(EntityStates.ParentMonster.GroundSlam.FixedUpdate));
        AddParriableStateVisuals<EntityStates.Wisp1Monster.ChargeEmbers>();
        PatchBulletAttackStateToParriable(typeof(EntityStates.Wisp1Monster.FireEmbers), nameof(EntityStates.Wisp1Monster.FireEmbers.OnEnter));
        List<GameObject> gameObjects = [CaeliImperiumAssets.VultureHunterBody, CaeliImperiumAssets.TitanGoldBody, CaeliImperiumAssets.FalseSonBossBody, CaeliImperiumAssets.FalseSonBossBrokenLunarShardBody, CaeliImperiumAssets.FalseSonBossBrokenLunarShardBody, CaeliImperiumAssets.FalseSonBossLunarShardBody, CaeliImperiumAssets.FalseSonBossBrokenLunarShardBody, CaeliImperiumAssets.BrotherBody, CaeliImperiumAssets.BrotherHurtBody, CaeliImperiumAssets.SolusHeartBody, CaeliImperiumAssets.SolusWingBody, CaeliImperiumAssets.MiniVoidRaidCrabBaseBody, CaeliImperiumAssets.MiniVoidRaidCrabPhase1Body, CaeliImperiumAssets.MiniVoidRaidCrabPhase2Body, CaeliImperiumAssets.MiniVoidRaidCrabPhase3Body, CaeliImperiumAssets.ReaperBody, CaeliImperiumAssets.ProvidenceBody];
        foreach (GameObject gameObject1 in gameObjects)
        {
            DoomguyStaggerOnHurt bishopStaggerOnHurt = gameObject1.EnsureComponent<DoomguyStaggerOnHurt>();
            bishopStaggerOnHurt.staggerAmount = StaggerAmountForFinalBosses;
            bishopStaggerOnHurt.staggerDuration = StaggerDuration;
            DoomguyAddBerserkOnEnable doomguyAddBerserkOnEnable = gameObject1.AddComponent<DoomguyAddBerserkOnEnable>();
        }
        if (CaeliImperiumPlugin.enemiesReturnsEnabled) EnemiesReturnsCompatability.BishopInit();
        if (CaeliImperiumPlugin.enemyAbilitiesEnabled) EnemyAbilitiesCompatibility.BishopInit();
    }

    private static float TrajectoryAimAssist_GetAimAssistRadius(On.RoR2.TrajectoryAimAssist.orig_GetAimAssistRadius orig, GameObject owner)
    {
        if (owner.TryGetComponent<CharacterBody>(out CharacterBody characterBody) && characterBody.bodyIndex == Body.bodyIndex) return OverrideAimAssist;
        return orig(owner);
    }

    private static void QuadrantPhase_OnQuadrantComplete(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        int locId = 2;
        ILLabel iLLabel = null;
        if (c.TryGotoNext(
            MoveType.After,
            x => x.MatchLdfld<ChampRushMissionController>(nameof(ChampRushMissionController.bonusTimePerWaveCompleted))
        ))
        {
            c.EmitDelegate(IncreaseDurationOfChamprush);
        }
        else
        {
            CaeliImperiumPlugin.Log.LogError("IL Hook " + il.Method.Name + " failed!");
        }
    }

    private static void Ascending_OnEnter(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        int locId = 2;
        ILLabel iLLabel = null;
        if (c.TryGotoNext(
            MoveType.After,
            x => x.MatchLdfld<ChampRushMissionController>(nameof(ChampRushMissionController.durationOfChamprush))
        ))
        {
            c.EmitDelegate(IncreaseDurationOfChamprush);
        }
        else
        {
            CaeliImperiumPlugin.Log.LogError("IL Hook " + il.Method.Name + " failed!");
        }
    }
    private static float IncreaseDurationOfChamprush(float duration)
    {
        if (CaeliImperiumUtils.IsDoom()) return duration * ChampRushDurationMultiplier;
        return duration;
    }
    private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (!CaeliImperiumUtils.IsDoom() || !sender) return;
        if (sender.isBoss)
        {
            args.healthMultAdd += BossHealthMultiplier - 1f;
        }
        else if (sender.IsHeavyMonster())
        {
            args.healthMultAdd += HeavyHealthMultiplier - 1f;
        }
    }

    private static void MusicController_pickTrackHook(MusicController musicController, ref MusicTrackDef newTrack)
    {
        SceneDef mostRecentSceneDef = SceneCatalog.mostRecentSceneDef;
        if (musicController.enableMusicSystem && mostRecentSceneDef && mostRecentSceneDef.bossTrack && newTrack == mostRecentSceneDef.bossTrack)
        {
            newTrack = TheHighCouncilMusicTrackDef;
        }
    }

    private static void PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3(On.RoR2.PickupDropletController.orig_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 orig, GenericPickupController.CreatePickupInfo pickupInfo, Vector3 position, Vector3 velocity)
    {
        if (CaeliImperiumUtils.IsDoom())
        {
            if (pickupInfo.pickup.isValid)
            {
                PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupInfo.pickup.pickupIndex);
                if (pickupDef != null)
                {
                    if (pickupDef.itemIndex != ItemIndex.None)
                    {
                        ItemDef itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
                        if (itemDef && bannedItemTierDefs.Contains(itemDef._itemTierDef)) return;
                    }
                    if (pickupDef.equipmentIndex != EquipmentIndex.None)
                    {
                        EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex);
                        if (equipmentDef && equipmentDef.isLunar) return;
                    }
                }
               
            }
        }
        orig(pickupInfo, position, velocity);
    }

    private static bool EliteTierDef_CanSelect(On.RoR2.CombatDirector.EliteTierDef.orig_CanSelect orig, CombatDirector.EliteTierDef self, SpawnCard.EliteRules rules)
    {
        if (CaeliImperiumUtils.IsDoom()) return false;
        return orig(self, rules);
    }
    private static void SetStateOnHurt_SetImmobilize(On.RoR2.SetStateOnHurt.orig_SetImmobilize orig, SetStateOnHurt self, GameObject attacker, GameObject inflictor)
    {
        if (IamTooLazyReturn()) return;
        orig(self, attacker, inflictor);
    }
    private static void SetStateOnHurt_SetPain(On.RoR2.SetStateOnHurt.orig_SetPain orig, SetStateOnHurt self)
    {
        if (IamTooLazyReturn()) return;
        orig(self);
    }
    private static void SetStateOnHurt_SetTaunt(On.RoR2.SetStateOnHurt.orig_SetTaunt orig, SetStateOnHurt self, GameObject target)
    {
        if (IamTooLazyReturn()) return;
        orig(self, target);
    }
    private static void SetStateOnHurt_SetStun(On.RoR2.SetStateOnHurt.orig_SetStun orig, SetStateOnHurt self, float duration)
    {
        if (IamTooLazyReturn()) return;
        orig(self, duration);
    }
    private static void SetStateOnHurt_SetShock(On.RoR2.SetStateOnHurt.orig_SetShock orig, SetStateOnHurt self, float duration)
    {
        if (IamTooLazyReturn()) return;
        orig(self, duration);
    }

    private static void SetStateOnHurt_SetFrozen(On.RoR2.SetStateOnHurt.orig_SetFrozen orig, SetStateOnHurt self, float duration)
    {
        if (IamTooLazyReturn()) return;
        orig(self, duration);
    }
    private static bool IamTooLazyReturn() => CaeliImperiumUtils.IsDoom() && IamTooLazy != null && IamTooLazy.damageInfo != null && !IamTooLazy.damageInfo.damageType.IsDamageSourceSkillBased;
    private static DamageReport IamTooLazy;
    private static void SetStateOnHurt_OnTakeDamageServer(On.RoR2.SetStateOnHurt.orig_OnTakeDamageServer orig, SetStateOnHurt self, DamageReport damageReport)
    {
        IamTooLazy = damageReport;
        orig(self, damageReport);
        IamTooLazy = null;
    }

    private static void HealthComponent_TakeDamageForce_PhysForceInfo(On.RoR2.HealthComponent.orig_TakeDamageForce_PhysForceInfo orig, HealthComponent self, PhysForceInfo forceInfo)
    {
        if (CaeliImperiumUtils.IsDoom() && !forceInfo.Flags.HasFlag(PhysForceFlags.massIsOne)) forceInfo.force = forceInfo.force.normalized * 0.1f;
        orig(self, forceInfo);
    }

    private static void SceneInfo_Awake(On.RoR2.SceneInfo.orig_Awake orig, SceneInfo self)
    {
        orig(self);
        if (!self || !self.airNodes) return;
        for (int i = 0; i < self.airNodes.nodes.Length; i++)
        {
            ref NodeGraph.Node node = ref self.airNodes.nodes[i];
            if (!Physics.Raycast(node.position, Vector3.down, out RaycastHit raycastHit, FlyingEnemiesAboveGroundRaycastLength, LayerIndex.world.mask)) continue;
            Vector3 position = raycastHit.point + (Vector3.up * FlyingEnemiesAboveGround);
            if (position.y >= node.position.y) continue;
            node.position = position;
        }
    }

    private static SpawnCard.SpawnResult SpawnCard_DoSpawn(On.RoR2.SpawnCard.orig_DoSpawn orig, SpawnCard self, Vector3 position, Quaternion rotation, DirectorSpawnRequest spawnRequest)
    {
        if (CaeliImperiumUtils.IsDoom() && self && self.nodeGraphType == RoR2.Navigation.MapNodeGroup.GraphType.Air && self is CharacterSpawnCard characterSpawnCard)
        {
            if (Physics.Raycast(position, Vector3.up * -1f, out RaycastHit raycastHit, FlyingEnemiesAboveGroundRaycastLength, LayerIndex.world.mask))
            {
                position = raycastHit.point + (Vector3.up * FlyingEnemiesAboveGround);
            }
            else
            {
                position -= Vector3.up * -1f * FlyingEnemiesAboveGroundRaycastLength;
            }
        }
        return orig(self, position, rotation, spawnRequest);
    }
    private static int Inventory_GetItemCountEffective_ItemIndex(On.RoR2.Inventory.orig_GetItemCountEffective_ItemIndex orig, Inventory self, ItemIndex itemIndex)
    {
        int count = orig(self, itemIndex);
        CharacterMaster characterMaster = self.GetCharacterMaster();
        if (!characterMaster) return count;
        CharacterBody characterBody = characterMaster.GetBody();
        if (!characterBody || !characterBody.HasModdedBodyFlag(DoomguyBodyFlag)) return count;
        ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
        if (!itemDef) return count;
        if (bannedItemTierDefs.Contains(itemDef._itemTierDef)) return 0;
        return count;
    }

    private static void Stage_onStageStartGlobal(Stage obj)
    {
        DoomguyComponent.RemoveAllTimedBerserks();
    }

    private static void ShrineHalcyoniteFinished_OnEnter(On.EntityStates.ShrineHalcyonite.ShrineHalcyoniteFinished.orig_OnEnter orig, EntityStates.ShrineHalcyonite.ShrineHalcyoniteFinished self)
    {
        orig(self);
        DoomguyComponent.RemoveAllTimedBerserks();
    }

    private static void ShrineHalcyoniteActivatedState_OnEnter(On.EntityStates.ShrineHalcyonite.ShrineHalcyoniteActivatedState.orig_OnEnter orig, EntityStates.ShrineHalcyonite.ShrineHalcyoniteActivatedState self)
    {
        orig(self);
        if (!Run.instance || Run.instance.effectiveStageClearCount + 1 < BeginBerserkOnActivatingHalcyoniteShrineAfterStage) return;
        DoomguyComponent.AddTimedBerserk(BerserkDurationOnActivatingHalcyoniteShrine);
    }

    private static void CharacterBody_UpdateAllTemporaryVisualEffects(On.RoR2.CharacterBody.orig_UpdateAllTemporaryVisualEffects orig, CharacterBody self)
    {
        orig(self);
        if (!keyValuePairs7.TryGetValue(self, out TemporaryVisualEffect temporaryVisualEffect))
        {
            temporaryVisualEffect = null;
            keyValuePairs7[self] = temporaryVisualEffect;
        }
        self.UpdateSingleTemporaryVisualEffect(ref temporaryVisualEffect, BerserkEffect, self.radius, DoomguyComponent.HasBerserk() && self.HasModdedBodyFlag(CanBeBerserkedBodyFlag), "Head");
        keyValuePairs7[self] = temporaryVisualEffect;
    }

    private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1) // This is bullshit
    {
        if (!CaeliImperiumUtils.IsDoom()) return;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        List<GameObject> rootObjects = [];
        arg0.GetRootGameObjects(rootObjects);
        List<Renderer> renderers = [];
        //List<Light> lights = [];
        //List<PostProcessVolume> postProcessVolumes = [];
        Dictionary<Material, Material> keyValuePairs = [];
        Dictionary<Texture2D, Texture2D> keyValuePairs2 = [];
        HashSet<string> supportedShaders = new HashSet<string> { "CalmWater/Calm Water [Single Sided]", "CalmWater/Calm Water [DX11]", "Hopoo Games/Deferred/Triplanar Terrain Blend Emissive", "Hopoo Games/Deferred/Snow Topped", "Hopoo Games/Deferred/Triplanar Terrain Blend", "Hopoo Games/Deferred/Standard", "SpeedtreeOverride/SpeedtreeCustom" };
        foreach (GameObject gameObject in rootObjects)
        {
            Renderer[] renderers1 = gameObject.GetComponentsInChildren<Renderer>();
            renderers.AddRange(renderers1);
            //Light[] lights1 = gameObject.GetComponentsInChildren<Light>();
            //lights.AddRange(lights1);
            //PostProcessVolume[] postProcessVolumes1 = gameObject.GetComponentsInChildren<PostProcessVolume>();
            //postProcessVolumes.AddRange(postProcessVolumes1);
        }
        foreach (Renderer renderer in renderers)
        {
            if (!renderer) continue;
            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = renderer.sharedMaterials[i];
                if (!material) continue;
                string shaderName = material.shader.name;
                if (!supportedShaders.Contains(shaderName))
                {
                    materials[i] = material;
                    continue;
                }
                if (!keyValuePairs.TryGetValue(material, out Material darkAgesMaterial))
                {
                    darkAgesMaterial = GameObject.Instantiate(material);
                    darkAgesMaterial.name = material.name + "DarkAges";
                    switch (shaderName)
                    {
                        case "Hopoo Games/Deferred/Triplanar Terrain Blend":
                            HandleTriplanarTerrainBlend(darkAgesMaterial, "_BlueChannelTex", keyValuePairs2);
                            HandleTriplanarTerrainBlend(darkAgesMaterial, "_GreenChannelTex", keyValuePairs2);
                            HandleTriplanarTerrainBlend(darkAgesMaterial, "_RedChannelSideTex", keyValuePairs2);
                            HandleTriplanarTerrainBlend(darkAgesMaterial, "_RedChannelTopTex", keyValuePairs2);
                            HandleTriplanarTerrainBlend(darkAgesMaterial, "_SplatmapTex", keyValuePairs2);
                            break;
                        case "Hopoo Games/Deferred/Triplanar Terrain Blend Emissive":
                            HandleTriplanarTerrainBlend(darkAgesMaterial, "_BlueChannelTex", keyValuePairs2);
                            HandleTriplanarTerrainBlend(darkAgesMaterial, "_GreenChannelTex", keyValuePairs2);
                            HandleTriplanarTerrainBlend(darkAgesMaterial, "_RedChannelSideTex", keyValuePairs2);
                            HandleTriplanarTerrainBlend(darkAgesMaterial, "_RedChannelTopTex", keyValuePairs2);
                            HandleTriplanarTerrainBlend(darkAgesMaterial, "_SplatmapTex", keyValuePairs2);
                            break;
                        case "Hopoo Games/Deferred/Standard":
                            if (darkAgesMaterial.mainTexture)
                            {
                                HandleStandard(darkAgesMaterial, "_MainTex", keyValuePairs2);
                            }
                            else
                            {
                                HandleStageColor(darkAgesMaterial, "_Color");
                            }
                            break;
                        case "Hopoo Games/Deferred/Snow Topped":
                            HandleStandard(darkAgesMaterial, "_DirtTex", keyValuePairs2);
                            HandleStandard(darkAgesMaterial, "_SnowTex", keyValuePairs2);
                            if (darkAgesMaterial.mainTexture)
                            {
                                HandleStandard(darkAgesMaterial, "_MainTex", keyValuePairs2);
                            }
                            else
                            {
                                HandleStageColor(darkAgesMaterial, "_Color");
                            }
                            break;
                        case "SpeedtreeOverride/SpeedtreeCustom":
                            if (darkAgesMaterial.mainTexture)
                            {
                                HandleStandard(darkAgesMaterial, "_MainTex", keyValuePairs2);
                            }
                            HandleStageColor(darkAgesMaterial, "_Color");
                            break;
                        case "CalmWater/Calm Water [DX11]":
                            HandleWater(darkAgesMaterial);
                            break;
                        default:
                            break;
                    }
                    keyValuePairs.Add(material, darkAgesMaterial);
                }
                materials[i] = darkAgesMaterial;
            }
            renderer.sharedMaterials = materials;
        }
        /*foreach (Light light in lights)
        {
            if (!light) continue;
            light.color = light.color.MultiplyHSV(1f, StageLightSaturationMultiplier, StageLightValueMultiplier);
        }
        foreach (PostProcessVolume postProcessVolume in postProcessVolumes)
        {
            if (!postProcessVolume || !postProcessVolume.profile) continue;
            foreach (PostProcessEffectSettings postProcessEffectSettings in postProcessVolume.profile.settings)
            {
                if (postProcessEffectSettings is not RampFog rampFog) continue;
                rampFog.fogColorStart.value = rampFog.fogColorStart.value.MultiplyHSV(1f, StageRampFogSaturationMultiplier, StageRampFogValueMultiplier);
                rampFog.fogColorMid.value = rampFog.fogColorStart.value.MultiplyHSV(1f, StageRampFogSaturationMultiplier, StageRampFogValueMultiplier);
                rampFog.fogColorEnd.value = rampFog.fogColorStart.value.MultiplyHSV(1f, StageRampFogSaturationMultiplier, StageRampFogValueMultiplier);
            }
        }*/
        RenderSettings.ambientEquatorColor = RenderSettings.ambientEquatorColor.MultiplyHSV(1f, StageAmbientSaturationMultiplier, StageAmbientValueMultiplier);
        RenderSettings.ambientGroundColor = RenderSettings.ambientGroundColor.MultiplyHSV(1f, StageAmbientSaturationMultiplier, StageAmbientValueMultiplier);
        RenderSettings.ambientSkyColor = RenderSettings.ambientSkyColor.MultiplyHSV(1f, StageAmbientSaturationMultiplier, StageAmbientValueMultiplier);
        Material material1 = RenderSettings.skybox;
        Material material2 = GameObject.Instantiate(RenderSettings.skybox);
        material2.name += "DarkAges";
        HandleSkyboxColor(material2, "_SkyColor1");
        HandleSkyboxColor(material2, "_SkyColor2");
        HandleSkyboxColor(material2, "_SkyColor3");
        HandleSkyboxColor(material2, "_SunColor");
        RenderSettings.skybox = material2;
        stopwatch.Stop();
        CaeliImperiumPlugin.Log.LogMessage("Completed making the stage dark and griddy in " + stopwatch.ElapsedMilliseconds / 1000f + " seconds");
    }
    private static void HandleWater(Material material)
    {
        HandleStageColor(material, "_FoamColor");
        HandleStageColor(material, "_DepthColor");
    }
    private static void HandleTriplanarTerrainBlend(Material material, string name, Dictionary<Texture2D, Texture2D> keyValuePairs)
    {
        Texture texture = material.GetTexture(name);
        if (!texture) return;
        material.SetTexture(name, GetDarkAgesTexture(texture, keyValuePairs));
        material.SetTextureScale(name, material.GetTextureScale(name) * StageTextureSizeMultiplier);
    }
    private static void HandleStandard(Material material, string name, Dictionary<Texture2D, Texture2D> keyValuePairs)
    {
        Texture texture = material.GetTexture(name);
        if (!texture) return;
        material.SetTexture(name, GetDarkAgesTexture(texture, keyValuePairs));
    }
    private static void HandleStageColor(Material material, string name)
    {
        material.SetColor(name, material.GetColor(name).MultiplyHSV(1f, StageTextureSaturationMultiplier, StageTextureValueMultiplier));
    }
    private static void HandleSkyboxColor(Material material, string name)
    {
        material.SetColor(name, material.GetColor(name).MultiplyHSV(1f, StageSkyboxSaturationMultiplier, StageSkyboxValueMultiplier));
    }
    private static Texture GetDarkAgesTexture(Texture texture, Dictionary<Texture2D, Texture2D> keyValuePairs)
    {
        if (!texture) return texture;
        if (texture is not Texture2D texture2D) return texture;
        if (!keyValuePairs.TryGetValue(texture2D, out Texture2D texture2D1))
        {
            texture2D1 = texture2D.MultiplyHSV(1f, StageTextureSaturationMultiplier, StageTextureValueMultiplier);
            keyValuePairs.Add(texture2D, texture2D1);
        }
        return texture2D1;
    }
    private static HealthComponent.HealthBarValues HealthComponent_GetHealthBarValues(On.RoR2.HealthComponent.orig_GetHealthBarValues orig, HealthComponent self)
    {
        HealthComponent.HealthBarValues healthBarValues = orig(self);
        if (!self || !self.body) return healthBarValues;
        DoomguyComponent bishopComponent = self.body.GetComponent<DoomguyComponent>();
        if (!bishopComponent) return healthBarValues;
        float num = 1f - 1f / self.body.cursePenalty;
        float num2 = (1f - num) / self.fullHealth;
        healthBarValues.regainableHealthFraction = Mathf.Clamp01(healthBarValues.regainableHealthFraction + (bishopComponent.reservedHealPercentage * self.fullHealth * num2));
        return healthBarValues;
    }

    private static void CaeliImperiumHooks_OnSetProjectilePrefabsIndividualPrefab(ProjectileController obj)
    {
        ParriableProjectileController parriableProjectileController = obj.EnsureComponent<ParriableProjectileController>();
        parriableProjectileController.projectileDamage = obj.GetComponent<ProjectileDamage>();
        parriableProjectileController.projectileController = obj;
        if (!projectilesCatalogInit) projectilesCatalogInit = true;
        if (customProjectileSpeedMultiplier.TryGetValue(obj, out float value)) runtimeCustomProjectileSpeedMultiplier.Add(obj.catalogIndex, value);
    }

    private static void Run_onRunSetRuleBookGlobal(Run arg1, RuleBook arg2)
    {
        if (!arg1.GetEventFlag("BishopRun")) return;
        arg1.availableItems.Clear();
        foreach (string itemName in keepItems)
        {
            if (itemName.IsNullOrWhiteSpace()) continue;
            ItemIndex itemIndex = ItemCatalog.FindItemIndex(itemName);
            if (itemIndex == ItemIndex.None) continue;
            arg1.availableItems.Add(itemIndex);
        }
        arg1.availableDrones.Clear();
        foreach (string droneName in keepClankers)
        {
            if (droneName.IsNullOrWhiteSpace()) continue;
            DroneIndex droneIndex = DroneCatalog.FindDroneIndex(droneName);
            if (droneIndex == DroneIndex.None) continue;
            arg1.availableDrones.Add(droneIndex);
        }
        arg1.availableEquipment.Clear();
        foreach (string equipmentName in keepEquipments)
        {
            if (equipmentName.IsNullOrWhiteSpace()) continue;
            EquipmentIndex equipmentIndex = EquipmentCatalog.FindEquipmentIndex(equipmentName);
            if (equipmentIndex == EquipmentIndex.None) continue;
            arg1.availableEquipment.Add(equipmentIndex);
        }
    }

    private static void BaseAI_OnServerDamageDealt(On.RoR2.CharacterAI.BaseAI.orig_OnServerDamageDealt orig, RoR2.CharacterAI.BaseAI self, DamageReport damageReport)
    {
        if (ReturnIfAttackerIsNotAPlayer(damageReport)) return;
        orig(self, damageReport);
    }

    private static void BaseAI_OnBodyDamaged(On.RoR2.CharacterAI.BaseAI.orig_OnBodyDamaged orig, RoR2.CharacterAI.BaseAI self, DamageReport damageReport)
    {
        if (ReturnIfAttackerIsNotAPlayer(damageReport)) return;
        orig(self, damageReport);
    }
    private static bool ReturnIfAttackerIsNotAPlayer(DamageReport damageReport)
    {
        if (CaeliImperiumUtils.IsDoom())
        {
            if (damageReport.attackerBody && !damageReport.attackerBody.isPlayerControlled)
            {
                return true;
            }
        }
        return false;
    }
    private static void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
    {
        if (self.IsDoom())
        {
            if (!self.GetEventFlag("BishopRun"))
            {
                self.SetEventFlag("BishopRun");
                CaeliImperiumPlugin.Log.LogMessage("Set BishopRun Flag");
            }
            else
            {
                CaeliImperiumPlugin.Log.LogMessage("BishopRun Flag Already Set");
            }
        }
        orig(self);
    }

    private static void BaseAI_FindEnemyHurtBox(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        int locId = 2;
        ILLabel iLLabel = null;
        if (c.TryGotoNext(
            MoveType.After,
            x => x.MatchLdloc(out locId),
            x => x.MatchCall<UnityEngine.Object>("op_Implicit"),
            x => x.MatchBrfalse(out iLLabel)
        ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, locId);
            c.EmitDelegate(TargetNonPlayers);
            c.Emit(OpCodes.Brfalse_S, iLLabel);
        }
        else
        {
            CaeliImperiumPlugin.Log.LogError("IL Hook " + il.Method.Name + " failed!");
        }
    }
    private static bool TargetNonPlayers(BaseAI baseAI, HurtBox hurtBox)
    {
        if (baseAI.body && baseAI.body && baseAI.body.teamComponent && baseAI.body.teamComponent.teamIndex != TeamIndex.Player)
        {
            if (CaeliImperiumUtils.IsDoom())
            {
                if (hurtBox.healthComponent && hurtBox.healthComponent.body && !hurtBox.healthComponent.body.isPlayerControlled)
                {
                    return false;
                }
            }
        }
        return true;
    }
    private static bool SetStateOnHurt_GetShouldHitStun(On.RoR2.SetStateOnHurt.orig_GetShouldHitStun orig, SetStateOnHurt self, HealthComponent healthComponent, float trueDamage)
    {
        if (CaeliImperiumUtils.IsDoom()) return false;
        return orig(self, healthComponent, trueDamage);
    }

    private static void BodyCatalog_SetBodyPrefabs(On.RoR2.BodyCatalog.orig_SetBodyPrefabs orig, GameObject[] newBodyPrefabs)
    {
        orig(newBodyPrefabs);
        foreach (GameObject gameObject in BodyCatalog.allBodyPrefabs)
        {
            CharacterBody characterBody = gameObject.GetComponent<CharacterBody>();
            if (!characterBody) continue;
            HandleDoomguyStaggerOnHurt(characterBody);
        }
    }
    private static void HandleDoomguyMoveFlyingEnemiesTowardsTheGround(CharacterBody characterBody)
    {
        Rigidbody rigidbody = characterBody.GetComponent<Rigidbody>();
        if (!rigidbody || (!rigidbody.isKinematic && rigidbody.useGravity)) return;
        CharacterMotor characterMotor = characterBody.GetComponent<CharacterMotor>();
        if (characterMotor && characterMotor.flightParameters.CheckShouldUseFlight()) return;
        DoomguyMoveFlyingEnemiesTowardsTheGround doomguyMoveFlyingEnemiesTowardsTheGround = characterBody.EnsureComponent<DoomguyMoveFlyingEnemiesTowardsTheGround>();
        doomguyMoveFlyingEnemiesTowardsTheGround.updateInterval = FlyingEnemiesUpdateInterval;
        doomguyMoveFlyingEnemiesTowardsTheGround.raycastLengthForCeiling = FlyingEnemiesBelowCeilingRaycastLength;
        doomguyMoveFlyingEnemiesTowardsTheGround.raycastLengthForGround = FlyingEnemiesAboveGroundRaycastLength;
        doomguyMoveFlyingEnemiesTowardsTheGround.distanceAboveGround = FlyingEnemiesAboveGround;
        doomguyMoveFlyingEnemiesTowardsTheGround.distanceBelowCeiling = FlyingEnemiesBelowCeiling;
        doomguyMoveFlyingEnemiesTowardsTheGround.maxSpeed = FlyingEnemiesMaxMoveSpeed;
        doomguyMoveFlyingEnemiesTowardsTheGround.collisionMask = LayerIndex.world.mask;
    }
    private static void HandleDoomguyStaggerOnHurt(CharacterBody characterBody)
    {
        if (!characterBody.isChampion) return;
        DoomguyStaggerOnHurt bishopStaggerOnHurt = characterBody.GetComponent<DoomguyStaggerOnHurt>();
        if (bishopStaggerOnHurt) return;
        bishopStaggerOnHurt = characterBody.gameObject.AddComponent<DoomguyStaggerOnHurt>();
        bishopStaggerOnHurt.staggerAmount = StaggerAmount;
        bishopStaggerOnHurt.staggerDuration = StaggerDuration;
    }
    private static bool IsHeavyMonster(this CharacterBody characterBody)
    {
        if (characterBody.HasModdedBodyFlag(HeavyMonsterBodyFlag)) return true;
        if (characterBody.HasModdedBodyFlag(FodderMonsterBodyFlag)) return false;
        if (characterBody.isChampion || characterBody.hullClassification >= HullClassification.Golem) return true;
        return false;
    }
    private static void BlastAttack_CollectHits(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        while (c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<BlastAttack>(nameof(BlastAttack.radius))
            ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(InflatBlastAttack);
        }
    }

    private static bool DirectorCard_IsAvailable(On.RoR2.DirectorCard.orig_IsAvailable orig, DirectorCard self)
    {
        bool flag = orig(self);
        if (!flag) return flag;
        if (Run.instance && Run.instance.GetEventFlag("BishopRun") && self.spawnCard && self.spawnCard.prefab && !self.spawnCard.prefab.name.IsNullOrWhiteSpace() && purgeMonstersAndInteractables.Contains(self.spawnCard.prefab.name)) return false;
        return flag;
    }

    private static void OverlapAttack_Fire(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        while (c.TryGotoNext(MoveType.After,
                x => x.MatchCallvirt<Transform>("get_lossyScale")
            ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(InflateOverlapAttack);
        }
    }
    private static Vector3 InflateOverlapAttack(Vector3 vector3, OverlapAttack overlapAttack)
    {
        if (overlapAttack.HasModdedDamageType(ParriableDamageType)) return vector3 * ParriableOverlapAttackSizeMultiplier;
        return vector3;
    }
    private static float InflatBlastAttack(float radius, BlastAttack blastAttack)
    {
        if (blastAttack.HasModdedDamageType(ParriableDamageType)) return radius * ParriableBlastAttackSizeMultiplier;
        return radius;
    }
    // This should have been an IL hook but I am lazy baaastaaard
    private static void ProjectileManager_InitializeProjectile(On.RoR2.Projectile.ProjectileManager.orig_InitializeProjectile orig, ProjectileController projectileController, FireProjectileInfo fireProjectileInfo)
    {
        if (CaeliImperiumUtils.IsDoom())
        {
            TeamIndex teamIndex = TeamComponent.GetObjectTeam(fireProjectileInfo.owner);
            if (TeamManager.IsTeamEnemy(teamIndex, TeamIndex.Player))
            {
                if (!runtimeCustomProjectileSpeedMultiplier.TryGetValue(projectileController.catalogIndex, out float speedMultiplier)) speedMultiplier = GlobalProjectileSpeedMultiplier;
                if (fireProjectileInfo.useSpeedOverride)
                {
                    fireProjectileInfo.speedOverride *= speedMultiplier;
                }
                else
                {
                    IProjectileSpeedModifierHandler component6 = projectileController.GetComponent<IProjectileSpeedModifierHandler>();
                    if (component6 != null)
                    {
                        float speed = -1f;
                        if (component6 is ProjectileSimple projectileSimple)
                        {
                            speed = projectileSimple.desiredForwardSpeed;
                        }
                        else if (component6 is CleaverProjectile cleaverProjectile)
                        {
                            speed = cleaverProjectile.travelSpeed;
                        }
                        if (speed > 0f)
                        {
                            fireProjectileInfo.speedOverride = speed * speedMultiplier;
                        }
                    }
                }
            }
            
        }
        orig(projectileController, fireProjectileInfo);
    }

    private static void HealthComponent_TakeDamageProcess1(ILContext il)
    {
        #region notit
        ILCursor c = new ILCursor(il);
        int locID2 = 15;
        if (
            c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(out _),
                x => x.MatchLdfld(out _),
                x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.damage)),
                x => x.MatchStloc(out locID2)
            ))
        {
            ILLabel iLLabel = null;
            if (
            c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<HealthComponent.ItemCounts>(nameof(HealthComponent.ItemCounts.minHealthPercentage)),
                x => x.MatchLdcI4(out _),
                x => x.MatchBle(out iLLabel)
            ))
            {
                c.GotoLabel(iLLabel, MoveType.Before);
                Instruction instruction22 = c.Next;
                Instruction instruction21 = c.Emit(OpCodes.Ldarg_0).Prev;
                iLLabel.Target = instruction21;
                c.Emit(OpCodes.Ldloc, locID2);
                c.EmitDelegate(ModifyDamage);
                c.Emit(OpCodes.Stloc, locID2);
            }
            else
            {
                CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 2 failed!");
            }
        }
        else
        {
            CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 1 failed!");
        }
        #endregion
        c = new ILCursor(il);
        int locID = 15;
        if (
            !c.TryGotoNext(MoveType.Before,
                x => x.MatchNewobj<DamageReport>(),
                x => x.MatchStloc(out locID)
            ))
        {
            CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 3 failed!");
            return;
        }
        Instruction instruction1 = il.Instrs[il.Instrs.Count - 1];
        ILLabel iLLabel2 = null;
        if (
            !c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.SoulSurge)),
                x => x.MatchCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff)),
                x => x.MatchBrfalse(out iLLabel2)
            ))
        {
            CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 4 failed!");
            return;
        }
        c.GotoLabel(iLLabel2, MoveType.Before);
        Instruction instruction2 = c.Next;
        Instruction instruction = c.Emit(OpCodes.Ldarg_0).Prev;
        iLLabel2.Target = instruction;
        c.Emit(OpCodes.Ldloc, locID);
        c.EmitDelegate(CanDie);
        c.Emit(OpCodes.Brtrue_S, instruction2);
        c.Emit(OpCodes.Br_S, instruction1);
    }
    private static float ModifyDamage(HealthComponent healthComponent, float damage)
    {
        DoomguyComponent doomguyComponent = healthComponent.GetComponent<DoomguyComponent>();
        if (!doomguyComponent) return damage;
        float damagePercentage = damage / healthComponent.fullCombinedHealth;
        if (damagePercentage < doomguyComponent.minDamageMaxHealthPercentage) return damage;
        float coof = Mathf.Lerp(0f, doomguyComponent.maxDamageMaxHealthPercentage - doomguyComponent.minDamageMaxHealthPercentage, damagePercentage);
        return Mathf.Lerp(doomguyComponent.minDamageRemap * healthComponent.fullCombinedHealth, doomguyComponent.maxDamageRemap * healthComponent.fullCombinedHealth, coof);
    }
    private static bool CanDie(HealthComponent healthComponent, DamageReport damageReport)
    {
        CharacterBody characterBody = healthComponent.body;
        if (!characterBody) return true;
        if (characterBody.HasBuff(Stagger))
        {
            if (damageReport.damageInfo.damageType.IsDamageSourceSkillBased)
            {
                healthComponent.Networkhealth = 0f;
                return true;
            }
            else
            {
                healthComponent.Networkhealth = 1f;
                return false;
            }
               
        }
        if (characterBody.HasBuff(StaggerInvincibility) && !damageReport.damageInfo.HasModdedDamageType(GloryKillDamageType))
        {
            healthComponent.Networkhealth = 1f;
            return false;
        }
        if (characterBody.HasBuff(WasStaggered)) return true;
        if (DoomguyComponent.enableCount > 0 && !damageReport.damageInfo.damageType.HasModdedDamageType(BerserkedDamageType) && !damageReport.damageInfo.HasModdedDamageType(GloryKillDamageType) && !characterBody.HasBuff(WasStaggered) && !characterBody.isPlayerControlled)
        {
            healthComponent.Networkhealth = 1f;
            characterBody.AddStagger(StaggerDuration, StaggerInvincibilityDuration, WasStaggeredDuration);
            characterBody.SuperStun(StaggerDuration);
            return false;
        }
        else
        {
            return true;
        }
    }
    public static void AddStagger(this CharacterBody characterBody, float staggerDuration, float staggerInvincibilityDuration, float wasStaggeredDuration)
    {
        if (staggerDuration > 0f) characterBody.AddTimedBuff(Stagger, staggerDuration);
        if (staggerInvincibilityDuration > 0f) characterBody.AddTimedBuff(StaggerInvincibility, staggerInvincibilityDuration);
        if (wasStaggeredDuration > 0f) characterBody.AddTimedBuff(WasStaggered, wasStaggeredDuration);
    }

    private static Dictionary<EntityStates.LemurianBruiserMonster.FireMegaFireball, int> keyValuePairs15 = [];
    /*private static void FireMegaFireball_OnExit(On.EntityStates.LemurianBruiserMonster.FireMegaFireball.orig_OnExit orig, EntityStates.LemurianBruiserMonster.FireMegaFireball self)
    {
        orig(self);
        keyValuePairs15.Remove(self);
    }

    private static void FireMegaFireball_OnEnter(On.EntityStates.LemurianBruiserMonster.FireMegaFireball.orig_OnEnter orig, EntityStates.LemurianBruiserMonster.FireMegaFireball self)
    {
        orig(self);
        keyValuePairs15.Add(self, UnityEngine.Random.Range(1, EntityStates.LemurianBruiserMonster.FireMegaFireball.projectileCount));
    }*/

    public struct ParriableFireProjectileInfo
    {
        public int parriableCount;
        public Predicate<ParriableProjectileStuff> customParriableProjectile;
    }
    private static bool CustomLemurianBruiserParriableProjectile(ParriableProjectileStuff parriableProjectileStuff)
    {
        if (parriableProjectileStuff.baseState == null || parriableProjectileStuff.baseState is not EntityStates.LemurianBruiserMonster.FireMegaFireball fireMegaFireBall) return false;
        return IAmTiredOfThinkingNewNames(fireMegaFireBall.projectilesFired, EntityStates.LemurianBruiserMonster.FireMegaFireball.projectileCount);
    }
    private static bool CustomRoboBallBossParriableProjectile(ParriableProjectileStuff parriableProjectileStuff)
    {
        if (parriableProjectileStuff.baseState == null || parriableProjectileStuff.baseState is not EntityStates.RoboBallBoss.Weapon.FireEyeBlast fireEyeBlast) return false;
        return IAmTiredOfThinkingNewNames(fireEyeBlast.projectilesFired, fireEyeBlast.projectileCount);
    }
    private static bool IAmTiredOfThinkingNewNames(int firedCount, int maxCount)
    {
        if (firedCount > 0 && firedCount < maxCount - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private static void ChargeTrioBomb_OnEnter(On.EntityStates.Bell.BellWeapon.ChargeTrioBomb.orig_OnEnter orig, EntityStates.Bell.BellWeapon.ChargeTrioBomb self)
    {
        orig(self);
        if (self.isAuthority) self.characterBody.SetClientBuffCount(PrepareParriableAttackCount, 0);
    }

    public struct ParriableStateInfo
    {
        public bool removeParriableOnStateExit;
        public float parriableDuration;
        public bool scaleWithAttackSpeed;
        public int parriableCount;
    }
    private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
    {
        if (self && self.body)
        {
            if (!damageInfo.damageType.IsDamageSourceSkillBased)
            {
                CharacterBody attackerBody = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
                if (attackerBody)
                {
                    DoomguyComponent bishopComponent = attackerBody.GetComponent<DoomguyComponent>();
                    if (bishopComponent)
                    {
                        damageInfo.damage *= bishopComponent.nonSkillDamageMultiplier;
                    }
                }
            }
            if (self.body.HasBuff(StaggerInvincibility) && !damageInfo.damageType.HasModdedDamageType(GloryKillDamageType))
            {
                return;
            }
            if (damageInfo.damageType.HasModdedDamageType(ParriableDamageType))
            {
                if (self.body.HasBuff(Parry))
                {
                    OnParry(self.body, damageInfo);
                    damageInfo.rejected = true;
                    return;
                }
                if (self.body.HasBuff(Dodge)) return;
            }
        }
        orig(self, damageInfo);
    }
    public static float ParryEffectScale = 3f;
    public struct OnParryInfo
    {
        public CharacterBody bodyThatParries;
        public DamageInfo damageInfo;
        public Vector3 parryPosition => damageInfo == null ? Vector3.zero : damageInfo.position;
    }
    public static void OnParry(CharacterBody bodyThatParries, DamageInfo damageInfo)
    {
        if (bodyThatParries.hasEffectiveAuthority) DoomguyOnParryNetMessage.SendToClients(bodyThatParries.netId, damageInfo);
        if (NetworkServer.active) bodyThatParries.AddIncreasePrimaryDamageStack();
        DoomguySpecialSpearSkillDef.RechargeStocksForSpecialSpearSkills(bodyThatParries);
        EffectData effectData = new EffectData
        {
            scale = ParryEffectScale,
            origin = damageInfo.position
        };
        EffectManager.SpawnEffect(SpearParryEffect.index, effectData, false);
        if (onParry != null)
        {
            onParry.Invoke(new OnParryInfo
            {
                bodyThatParries = bodyThatParries,
                damageInfo = damageInfo
            });
        }
    }
    private static void EntityState_OnExit(On.EntityStates.EntityState.orig_OnExit orig, EntityState self)
    {
        orig(self);
        if (!NetworkServer.active || !parriableStatesTypes.TryGetValue(self.GetType(), out ParriableStateInfo parriableStateInfo) || !parriableStateInfo.removeParriableOnStateExit || self is not BaseState baseState) return;
        RemoveParriable(baseState);
    }
    private static void EntityState_OnEnter(On.EntityStates.EntityState.orig_OnEnter orig, EntityState self)
    {
        orig(self);
        if (DoomguyComponent.enableCount <= 0) return;
        if (!NetworkServer.active || !parriableStatesTypes.TryGetValue(self.GetType(), out ParriableStateInfo parriableStateInfo) || self is not BaseState baseState) return;
        if (parriableStateInfo.parriableCount > 0)
        {
            int buffCount = baseState.characterBody.GetClientBuffCount(PrepareParriableAttackCount);
            if (buffCount >= parriableStateInfo.parriableCount - 1)
            {
                AddParriable(baseState);
                return;
            }
            else
            {
                return;
            }
        }
        AddParriable(baseState);
    }
    private static void EntityState_FixedUpdate(On.EntityStates.EntityState.orig_FixedUpdate orig, EntityState self)
    {
        orig(self);
        if (!NetworkServer.active || !parriableStatesTypes.TryGetValue(self.GetType(), out ParriableStateInfo parriableStateInfo) || parriableStateInfo.parriableDuration <= 0f || self is not BaseState baseState) return;
        float duration = parriableStateInfo.parriableDuration;
        if (parriableStateInfo.scaleWithAttackSpeed) duration *= baseState.attackSpeedStat;
        if (baseState.fixedAge >= duration) RemoveParriable(baseState);
    }
    public static void AddParriableStateVisuals<T>(float parriableDuration, bool scaleWithAttackSpeed) where T : BaseState => AddParriableStateVisuals<T>(true, 0, parriableDuration, scaleWithAttackSpeed);
    public static void AddParriableStateVisuals<T>() where T : BaseState => AddParriableStateVisuals<T>(true, 0, 0, false);
    public static void AddParriableStateVisuals<T>(int parriableCount) where T : BaseState => AddParriableStateVisuals<T>(true, parriableCount, 0, false);
    public static void AddParriableStateVisuals<T>(bool removeParriableOnStateExit, int parriableCount, float parriableDuration, bool scaleWithAttackSpeed) where T : BaseState
    {
        Type type = typeof(T);
        if (parriableStatesTypes.ContainsKey(type)) return;
        ParriableStateInfo parriableStateInfo = new ParriableStateInfo
        {
            removeParriableOnStateExit = removeParriableOnStateExit,
            parriableDuration = parriableDuration,
            scaleWithAttackSpeed = scaleWithAttackSpeed,
            parriableCount = parriableCount
        };
        parriableStatesTypes.Add(type, parriableStateInfo);
    }
    public static void BisonOnParry(DamageReport damageReport, BaseState baseState)
    {
        if (baseState is not EntityStates.Bison.Charge charge) return;
        EffectManager.SimpleMuzzleFlash(EntityStates.Bison.Charge.hitEffectPrefab, baseState.gameObject, "SphereCheckTransform", true);
        baseState.healthComponent.TakeDamageForce(baseState.characterDirection.forward * EntityStates.Bison.Charge.selfStunForce, true, false);
        StunState stunState = new StunState();
        stunState.stunDuration = EntityStates.Bison.Charge.selfStunDuration;
        baseState.outer.SetNextState(stunState);
    }
    public static void AddCustomOnBodyParried<T>(OnBodyParried onBodyParried) where T : BaseState
    {
        Type type = typeof(T);
        if (keyValuePairs4.ContainsKey(type)) return;
        keyValuePairs4.Add(type, onBodyParried);
    }
    private static float AddAboutToAttackParryEffectScale = 0.5f;
    public static void AddParriable(BaseState baseState)
    {
        CharacterBody characterBody = baseState.characterBody;
        if (!characterBody || !NetworkServer.active || keyValuePairs5.Contains(baseState)) return;
        characterBody.AddBuff(PrepareParriableAttackCount);
        EffectData effectData = new EffectData
        {
            origin = baseState.transform.position,
            scale = baseState.characterBody ? baseState.characterBody.radius + AddAboutToAttackParryEffectScale : AddAboutToAttackParryEffectScale
        };
        EffectManager.SpawnEffect(AboutToAttackParryEffect.index, effectData, true);
        keyValuePairs5.Add(baseState);
        /*if (keyValuePairs3.TryGetValue(characterBody, out HashSet<BaseState> baseStates))
        {
            if (baseStates == null) baseStates = [];
            if (!baseStates.Contains(baseState)) baseStates.Add(baseState);
        }
        else
        {
            keyValuePairs3.Add(characterBody, [ baseState ]);
        }*/
    }
    public static void RemoveParriable(BaseState baseState)
    {
        CharacterBody characterBody = baseState.characterBody;
        if (!characterBody || !NetworkServer.active || !keyValuePairs5.Contains(baseState)) return;
        characterBody.RemoveBuff(PrepareParriableAttackCount);
        keyValuePairs5.Remove(baseState);
        //if (keyValuePairs3.TryGetValue(characterBody, out HashSet<BaseState> baseStates)) if (baseStates != null && baseStates.Contains(baseState)) baseStates.Remove(baseState);
    }
    private static void CharacterModel_UpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
    {
        orig(self);
        if (!self) return;
        CharacterBody characterBody = self.body;
        if (!characterBody) return;
        HandleCustomOverlay(characterBody, self, PrepareParriableAttackCount, CaeliImperiumAssets.Parriable, keyValuePairs2);
        HandleCustomOverlay(characterBody, self, Stagger, CaeliImperiumAssets.Staggered, keyValuePairs6);
    }
    private static void HandleCustomOverlay(CharacterBody characterBody, CharacterModel characterModel, BuffDef buffDef, Material material, FixedConditionalWeakTable<CharacterModel, TemporaryOverlayInstance> keyValuePairs)
    {
        if (characterBody.HasBuff(buffDef))
        {
            if (!keyValuePairs.ContainsKey(characterModel))
            {
                TemporaryOverlayInstance temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(characterModel.gameObject);
                temporaryOverlayInstance.originalMaterial = material;
                temporaryOverlayInstance.inspectorCharacterModel = characterModel;
                temporaryOverlayInstance.assignedCharacterModel = characterModel;
                temporaryOverlayInstance.AddToCharacterModel(characterModel);
                keyValuePairs.Add(characterModel, temporaryOverlayInstance);
            }
        }
        else if (keyValuePairs.TryGetValue(characterModel, out TemporaryOverlayInstance temporaryOverlayInstance))
        {
            temporaryOverlayInstance.RemoveFromCharacterModel();
            TemporaryOverlayManager.RemoveOverlay(temporaryOverlayInstance.managerIndex);
            keyValuePairs.Remove(characterModel);
        }
    }
    internal static void PatchProjectileStateToParriable(Type type, string name, Type type1, int parriableCount) => PatchProjectileStateToParriable(type, name, type1, parriableCount, null);
    internal static void PatchProjectileStateToParriable(Type type, string name, int parriableCount) => PatchProjectileStateToParriable(type, name, type, parriableCount, null);
    internal static void PatchProjectileStateToParriable(Type type, string name, int parriableCount, Predicate<ParriableProjectileStuff> customParriableProjectile) => PatchProjectileStateToParriable(type, name, type, parriableCount, customParriableProjectile);
    internal static void PatchProjectileStateToParriable(Type type, string name, Predicate<ParriableProjectileStuff> customParriableProjectile) => PatchProjectileStateToParriable(type, name, type, 0, customParriableProjectile);
    internal static void PatchProjectileStateToParriable(Type type, string name, Type type1, Predicate<ParriableProjectileStuff> customParriableProjectile) => PatchProjectileStateToParriable(type, name, type1, 0, customParriableProjectile);
    internal static void PatchProjectileStateToParriable(Type type, string name, Type type1, int parriableCount, Predicate<ParriableProjectileStuff> customParriableProjectile)
    {
        if (!patchedProjectileStates.Contains(type))
        {
            MethodBase methodBase = AccessTools.Method(type, name);
            _hooks.Add(new ILHook(methodBase, HandleShootParriableProjectileDisplayClass.HandleShootParriableProjectile));
            patchedProjectileStates.Add(type);
        }
        ParriableFireProjectileInfo parriableFireProjectileInfo = new ParriableFireProjectileInfo
        {
            parriableCount = parriableCount,
            customParriableProjectile = customParriableProjectile
        };
        types.Add(type1, parriableFireProjectileInfo);
    }
    internal static void PatchOverlapStateToParriable(Type type, string name) => PatchOverlapStateToParriable(type, name, type);
    internal static void PatchOverlapStateToParriable(Type type, string name, Type type1)
    {
        if (!patchedOverlapStates.Contains(type))
        {
            MethodBase methodBase = AccessTools.Method(type, name);
            _hooks.Add(new ILHook(methodBase, HandleFireOverlapAttack));
            patchedOverlapStates.Add(type);
        }
        types3.Add(type1);
    }
    internal static void PatchBlastAttackStateToParriable(Type type, string name) => PatchBlastAttackStateToParriable(type, name, type);
    internal static void PatchBlastAttackStateToParriable(Type type, string name, Type type1)
    {
        if (!patchedBlastStates.Contains(type))
        {
            MethodBase methodBase = AccessTools.Method(type, name);
            _hooks.Add(new ILHook(methodBase, HandleFireBlastAttack));
            patchedBlastStates.Add(type);
        }
        types3.Add(type1);
    }
    internal static void PatchBulletAttackStateToParriable(Type type, string name) => PatchBulletAttackStateToParriable(type, name, type);
    internal static void PatchBulletAttackStateToParriable(Type type, string name, Type type1)
    {
        if (!patchedBulletStates.Contains(type))
        {
            MethodBase methodBase = AccessTools.Method(type, name);
            _hooks.Add(new ILHook(methodBase, HandleFireBulletAttack));
            patchedBulletStates.Add(type);
        }
        types3.Add(type1);
    }
    private static Gradient GetParriableGradient()
    {
        Gradient gradient = new Gradient();
        GradientAlphaKey[] gradientAlphaKeys = { new GradientAlphaKey {alpha = 1f, time = 0f, }, new GradientAlphaKey { alpha = 1f, time = 1f, } };
        GradientColorKey[] gradientColorKeys = { new GradientColorKey { color = new Color(114f / 256f, 166f / 256f, 36f / 256f), time = 0f, }, new GradientColorKey { color = new Color(0f, 1f, 0.438499f), time = 1f, } };
        gradient.SetKeys(gradientColorKeys, gradientAlphaKeys);
        return gradient;
    }
    private static Gradient GetNonParriableGradient()
    {
        Gradient gradient = new Gradient();
        GradientAlphaKey[] gradientAlphaKeys = { new GradientAlphaKey { alpha = 1f, time = 0f, }, new GradientAlphaKey { alpha = 1f, time = 1f, } };
        GradientColorKey[] gradientColorKeys = { new GradientColorKey { color = new Color(196f / 256f, 40f / 256f, 19f / 256f), time = 0f, }, new GradientColorKey { color = new Color(1f, 0f, 0f), time = 1f, } };
        gradient.SetKeys(gradientColorKeys, gradientAlphaKeys);
        return gradient;
    }
    public static void SetCustomProjectileSpeedMultiplier(GameObject projectilePrefab, float speedMultiplier)
    {
        ProjectileController projectileController = projectilePrefab.GetComponent<ProjectileController>();
        if (!projectileController) return;
        SetCustomProjectileSpeedMultiplier(projectileController, speedMultiplier);
    }
    public static void SetCustomProjectileSpeedMultiplier(ProjectileController projectileController, float speedMultiplier)
    {
        if (projectilesCatalogInit)
        {
            runtimeCustomProjectileSpeedMultiplier[projectileController.catalogIndex] = speedMultiplier;
        }
        else
        {
            customProjectileSpeedMultiplier[projectileController] = speedMultiplier;
        }
    }
    
    private static void GlobalEventManager_onServerDamageDealt(DamageReport obj)
    {
        DamageInfo damageInfo = obj.damageInfo;
        if (damageInfo == null) return;
        CharacterBody victimBody = obj.victimBody;
        if (damageInfo.HasModdedDamageType(SuperStunDamageType)) victimBody.SuperStun(SuperStunDuration * damageInfo.procCoefficient);
        bool hasDamageType = damageInfo.HasModdedDamageType(GloryKillDamageType);
        CharacterBody attackerBody = obj.attackerBody;
        if (attackerBody)
        {
            bool hasStagger = victimBody.HasBuff(Stagger);
            bool damageIsBerserked = damageInfo.damageType.HasModdedDamageType(BerserkedDamageType);
            bool gloryKill = (hasStagger && hasDamageType) || (!victimBody.healthComponent.alive && hasDamageType) || damageIsBerserked;
            if (gloryKill)
            {
                ProcChainMask procChainMask = new ProcChainMask();
                float healCoof = gloryKill ? victimBody.isChampion ? HealFromGloryChampionKillCoeffecient : victimBody.IsHeavyMonster() ? HealFromGloryHeavyKillCoeffecient :  HealFromGloryKillCoeffecient : HealFromWhiffedGloryKillCoeffecient;
                float reserveHealCoof = gloryKill ? victimBody.isChampion ? ReserveHealFromGloryChampionKillCoeffecient : victimBody.IsHeavyMonster() ? ReserveHealFromGloryHeavyKillCoeffecient : ReserveHealFromGloryKillCoeffecient : ReserveHealFromWhiffedGloryKillCoeffecient;
                if (DoomguyStaggerOnHurt.keyValuePairs.TryGetValue(obj, out DoomguyStaggerOnHurt bishopStaggerOnHurt))
                {
                    if (bishopStaggerOnHurt.overrideGloryKillHeal) healCoof = bishopStaggerOnHurt.overrideGloryKillHealFraction;
                    if (bishopStaggerOnHurt.overrideGloryKillHealFromReserve) reserveHealCoof = bishopStaggerOnHurt.overrideGloryKillHealFromReserveFraction;
                }
                procChainMask.AddModdedProc(IgnoreBishopComponentHealRestrictionProcType);
                float healAmount = attackerBody.healthComponent.fullHealth * healCoof;
                float predictedHealth = attackerBody.healthComponent.health + healAmount;
                DoomguyComponent bishopComponent = attackerBody.GetComponent<DoomguyComponent>();
                if (bishopComponent && bishopComponent.reservedHealPercentage > 0f)
                {
                    float reservedHealPercentage = Mathf.Min(bishopComponent.reservedHealPercentage, reserveHealCoof);
                    float reservedHeal = reservedHealPercentage * attackerBody.healthComponent.fullHealth;
                    float predictedHealth2 = predictedHealth + healAmount;
                    float overHeal = predictedHealth2 - attackerBody.healthComponent.fullHealth;
                    if (overHeal > 0f) reservedHeal -= overHeal;
                    if (reservedHeal > 0f)
                    {
                        bishopComponent.SetReservedHealPercentage(bishopComponent.reservedHealPercentage - (reservedHeal / attackerBody.healthComponent.fullHealth));
                        healAmount += reservedHeal;
                    }
                }
                if (healAmount > 0f) attackerBody.healthComponent.Heal(healAmount, procChainMask);
                if (!victimBody.healthComponent.alive || damageIsBerserked)
                {
                    PhysForceInfo physForceInfo = new PhysForceInfo
                    {
                        force = damageInfo.force.normalized * GloryKillForce,
                        doNotExceed = false,
                        ignoreGroundStick = true,
                        massIsOne = true,
                        respectKnockupImmune = false,
                        resetVelocity = false,
                        disableAirControlUntilCollision = true
                    };
                    if (victimBody.characterMotor)
                    {
                        victimBody.characterMotor.ApplyForceImpulse(physForceInfo);
                    }
                    else if (victimBody.rigidbody)
                    {
                        victimBody.rigidbody.AddForceWithInfo(physForceInfo);
                    }
                }
                if (hasStagger) victimBody.SetBuffCount(Stagger.buffIndex, 0);
            }
        }
    }
    
    private static float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
    {
        if (self)
        {
            if (self.body && self.body.HasBuff(Stagger))
            {
                return 0f;
            }
            if (nonRegen)
            {
                DoomguyComponent bishopComponent = self.GetComponent<DoomguyComponent>();
                if (bishopComponent)
                {
                    if (!procChainMask.HasModdedProc(IgnoreBishopComponentHealRestrictionProcType))
                    {
                        float percentageHeal = (amount / self.fullHealth) * Mathf.Lerp(bishopComponent.minHealingMultiplier, bishopComponent.maxDamageRemap, bishopComponent.reservedHealPercentage);
                        bishopComponent.SetReservedHealPercentage(bishopComponent.reservedHealPercentage + percentageHeal);
                        return 0f;
                    }
                }
            }
        }
        float heal = orig(self, amount, procChainMask, nonRegen);
        return heal;
    }
    public static void AddParriableProjectileGhostFromProjectile(GameObject projectile)
    {
        GameObject gameObject = CaeliImperiumUtils.GetProjectileGhost(projectile);
        if (!gameObject) return;
        AddParriableProjectileGhost(gameObject);
    }
    public static void AddParriableProjectileGhost(GameObject projectileGhost)
    {
        if (_parriableProjectileGhosts.Contains(projectileGhost)) return;
        ParriableProjectileGhostColorChanger parriableProjectileGhostColorChanger = projectileGhost.AddComponent<ParriableProjectileGhostColorChanger>();
        parriableProjectileGhostColorChanger.Init(ParriableGradient, NonParriableGradient);
        _parriableProjectileGhosts.Add(projectileGhost);
    }
    public class HandleShootParriableProjectileDisplayClass
    {
        public int parriableCount;
        public Predicate<ParriableProjectileStuff> customParriableProjectile;
        public static void HandleShootParriableProjectile(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.Before,
                    x => x.MatchCallvirt<ProjectileManager>(nameof(ProjectileManager.FireProjectileWithoutDamageType))
                ))
            {
                HandleFireProjectileWithoutDamageType(c);
                c.GotoNext();
            }
            c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.Before,
                    x => x.MatchCallvirt<ProjectileManager>(nameof(ProjectileManager.FireProjectile)) &&
                    x.Operand is Mono.Cecil.MethodReference methodRef &&
                    methodRef.Parameters.Count > 1
                ))
            {
                HandleFireProjectile1(c);
                c.GotoNext();
            }
            c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.Before,
                    x => x.MatchCallvirt<ProjectileManager>(nameof(ProjectileManager.FireProjectile))
                ))
            {
                HandleFireProjectile2(c);
                c.GotoNext();

            }
        }
    }
    private static void HandleFireOverlapAttack(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        while (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallvirt<OverlapAttack>(nameof(OverlapAttack.Fire))
            ))
        {
            int valuesId = c.Context.Body.Variables.Count;
            c.Body.Variables.Add(new Mono.Cecil.Cil.VariableDefinition(c.Context.Import(typeof(ParriableOverlapStuff))));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(ParriableOverlapStuff.HandleFireOverlapAttack2);
            c.Emit(OpCodes.Stloc, valuesId);
            c.Emit(OpCodes.Ldloc, valuesId);
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableOverlapStuff), nameof(ParriableOverlapStuff.overlapAttack)));
            c.Emit(OpCodes.Ldloc, valuesId);
            c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableOverlapStuff), nameof(ParriableOverlapStuff.hurtBoxes)));
            c.Index++;
        }
    }
    private static void HandleFireBlastAttack(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallvirt<BlastAttack>(nameof(BlastAttack.Fire))
            ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(HandleBlastAttack2);
            return;
        }
    }
    private static BlastAttack HandleBlastAttack2(BlastAttack blastAttack, BaseState baseState)
    {
        RemoveParriable(baseState);
        if (types3.Contains(baseState.GetType()))
        {
            if (!blastAttack.HasModdedDamageType(ParriableDamageType)) blastAttack.AddModdedDamageType(ParriableDamageType);
            if (!blastAttack.HasModdedDamageType(ParriableMeleeDamageType)) blastAttack.AddModdedDamageType(ParriableMeleeDamageType);
        }
        return blastAttack;
    }
    private static BulletAttack HandleBulletAttack2(BulletAttack bulletAttack, BaseState baseState)
    {
        RemoveParriable(baseState);
        if (types3.Contains(baseState.GetType()))
        {
            if (!bulletAttack.HasModdedDamageType(ParriableDamageType)) bulletAttack.AddModdedDamageType(ParriableDamageType);
        }
        return bulletAttack;
    }
    private static void HandleFireBulletAttack(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallvirt<BulletAttack>(nameof(BulletAttack.Fire))
            ))
        {
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(HandleBulletAttack2);
            return;
        }
    }
    private struct ParriableOverlapStuff
    {
        public OverlapAttack overlapAttack;
        public List<HurtBox> hurtBoxes;
        public static ParriableOverlapStuff HandleFireOverlapAttack2(OverlapAttack overlapAttack, List<HurtBox> hurtBoxes, BaseState baseState)
        {
            RemoveParriable(baseState);
            if (types3.Contains(baseState.GetType()))
            {
                if (!overlapAttack.HasModdedDamageType(ParriableDamageType)) overlapAttack.AddModdedDamageType(ParriableDamageType);
                if (!overlapAttack.HasModdedDamageType(ParriableMeleeDamageType)) overlapAttack.AddModdedDamageType(ParriableMeleeDamageType);
            }
            return new ParriableOverlapStuff
            {
                overlapAttack = overlapAttack,
                hurtBoxes = hurtBoxes,
            };
        }
    }
    public struct ParriableProjectileStuff
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
        public GameObject owner;
        public float damage;
        public float force;
        public bool crit;
        public DamageColorIndex damageColorIndex;
        public GameObject target;
        public float speedOverride;
        public DamageTypeCombo? damageTypeCombo;
        public BaseState baseState;
        public static ParriableProjectileStuff HandleFireProjectileWithoutDamageTypeBullshit(GameObject prefab, Vector3 position, Quaternion rotation, GameObject owner, float damage,
            float force, bool crit, DamageColorIndex damageColorIndex, GameObject target, float speedOverride, DamageTypeCombo? damageTypeCombo, BaseState baseState)
        {
            RemoveParriable(baseState);
            ParriableProjectileStuff parriableProjectileStuff = new ParriableProjectileStuff
            {
                prefab = prefab,
                position = position,
                rotation = rotation,
                owner = owner,
                damage = damage,
                force = force,
                crit = crit,
                damageColorIndex = damageColorIndex,
                target = target,
                speedOverride = speedOverride,
                damageTypeCombo = damageTypeCombo,
                baseState = baseState,
            };
            if (!CaeliImperiumUtils.IsDoom()) return parriableProjectileStuff;
            Type type = baseState.GetType();
            //Chat.AddMessage("Shooting projectile from state type: " + type);
            if (types.TryGetValue(type, out ParriableFireProjectileInfo parriableFireProjectileInfo))
            {
                if (parriableFireProjectileInfo.customParriableProjectile != null)
                {
                    if (parriableFireProjectileInfo.customParriableProjectile.Invoke(parriableProjectileStuff)) parriableProjectileStuff.damageColorIndex = ParriableDamageColor;
                }
                else
                {
                    if (parriableFireProjectileInfo.parriableCount > 0)
                    {
                        CharacterBody characterBody = baseState.characterBody;
                        if (characterBody)
                        {
                            int buffCount = characterBody.GetClientBuffCount(PrepareParriableAttackCount);
                            //Chat.AddMessage("Shooting projectile parry count: " + buffCount);
                            if (buffCount >= parriableFireProjectileInfo.parriableCount - 1)
                            {
                                parriableProjectileStuff.damageColorIndex = ParriableDamageColor;
                                characterBody.SetClientBuffCount(PrepareParriableAttackCount, 0);
                            }
                            else
                            {
                                characterBody.AddClientBuff(PrepareParriableAttackCount);
                            }
                            buffCount = characterBody.GetClientBuffCount(PrepareParriableAttackCount);
                            SetClientBuffCountNetMessage.SendToClients(characterBody.netId, PrepareParriableAttackCount.buffIndex, buffCount);
                        }
                    }
                    else
                    {
                        parriableProjectileStuff.damageColorIndex = ParriableDamageColor;
                    }
                }
            }
            //Chat.AddMessage(parriableProjectileStuff.damageColorIndex == ParriableDamageColor ? "Shooting projectile parry" : "Shooting default projectile");
            return parriableProjectileStuff;
        }
    }
    private static FireProjectileInfo ModifyFireProjectileInfo(FireProjectileInfo fireProjectileInfo, BaseState baseState)
    {
        RemoveParriable(baseState);
        if (!CaeliImperiumUtils.IsDoom()) return fireProjectileInfo;
        ParriableProjectileStuff parriableProjectileStuff = new ParriableProjectileStuff
        {
            prefab = fireProjectileInfo.projectilePrefab,
            position = fireProjectileInfo.position,
            rotation = fireProjectileInfo.rotation,
            owner = fireProjectileInfo.owner,
            damage = fireProjectileInfo.damage,
            force = fireProjectileInfo.force,
            crit = fireProjectileInfo.crit,
            damageColorIndex = fireProjectileInfo.damageColorIndex,
            target = fireProjectileInfo.target,
            speedOverride = fireProjectileInfo.speedOverride,
            damageTypeCombo = fireProjectileInfo.damageTypeOverride,
            baseState = baseState,
        };
        Type type = baseState.GetType();
        if (types.TryGetValue(type, out ParriableFireProjectileInfo parriableFireProjectileInfo))
        {
            if (parriableFireProjectileInfo.customParriableProjectile != null)
            {
                if (parriableFireProjectileInfo.customParriableProjectile.Invoke(parriableProjectileStuff)) parriableProjectileStuff.damageColorIndex = ParriableDamageColor;
            }
            else
            {
                if (parriableFireProjectileInfo.parriableCount > 0)
                {
                    CharacterBody characterBody = baseState.characterBody;
                    if (characterBody)
                    {
                        int buffCount = characterBody.GetClientBuffCount(PrepareParriableAttackCount);
                        if (buffCount > parriableFireProjectileInfo.parriableCount - 1)
                        {
                            fireProjectileInfo.damageColorIndex = ParriableDamageColor;
                            characterBody.SetClientBuffCount(PrepareParriableAttackCount.buffIndex, 0);
                        }
                        else
                        {
                            characterBody.AddClientBuff(PrepareParriableAttackCount.buffIndex, buffCount + 1);
                        }
                    }
                }
                else
                {
                    fireProjectileInfo.damageColorIndex = ParriableDamageColor;
                }
            }
        }
        return fireProjectileInfo;
    }
    private static DamageTypeCombo? GetNullDamageTypeCombo() => null;
    private static void HandleFireProjectileWithoutDamageType(ILCursor c)
    {
        int valuesId = c.Context.Body.Variables.Count;
        c.Body.Variables.Add(new Mono.Cecil.Cil.VariableDefinition(c.Context.Import(typeof(ParriableProjectileStuff))));
        c.EmitDelegate(GetNullDamageTypeCombo);
        c.Emit(OpCodes.Ldarg_0);
        //c.Emit(OpCodes.Ldc_I4, parriableCount);
        //c.EmitDelegate(() => predicate);
        c.EmitDelegate(ParriableProjectileStuff.HandleFireProjectileWithoutDamageTypeBullshit);
        c.Emit(OpCodes.Stloc, valuesId);
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.prefab)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.position)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.rotation)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.owner)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.damage)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.force)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.crit)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.damageColorIndex)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.target)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.speedOverride)));
    }
    private static void HandleFireProjectile1(ILCursor c)
    {
        int valuesId = c.Context.Body.Variables.Count;
        c.Body.Variables.Add(new Mono.Cecil.Cil.VariableDefinition(c.Context.Import(typeof(ParriableProjectileStuff))));
        c.Emit(OpCodes.Ldarg_0);
        //c.Emit(OpCodes.Ldc_I4, parriableCount);
        //c.EmitDelegate(() => predicate);
        c.EmitDelegate(ParriableProjectileStuff.HandleFireProjectileWithoutDamageTypeBullshit);
        c.Emit(OpCodes.Stloc, valuesId);
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.prefab)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.position)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.rotation)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.owner)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.damage)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.force)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.crit)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.damageColorIndex)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.target)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.speedOverride)));
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ParriableProjectileStuff), nameof(ParriableProjectileStuff.damageTypeCombo)));
    }
    private static void HandleFireProjectile2(ILCursor c)
    {
        int valuesId = c.Context.Body.Variables.Count;
        c.Body.Variables.Add(new Mono.Cecil.Cil.VariableDefinition(c.Context.Import(typeof(ParriableProjectileStuff))));
        c.Emit(OpCodes.Ldarg_0);
        //c.Emit(OpCodes.Ldc_I4, parriableCount);
        //c.EmitDelegate(() => predicate);
        c.EmitDelegate(ModifyFireProjectileInfo);
    }
}
