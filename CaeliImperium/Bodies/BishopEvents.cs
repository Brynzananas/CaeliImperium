using BepInEx;
using BrynzaAPI;
using CaeliImperium.Components;
using CaeliImperium.Components.Bishop;
using CaeliImperium.NetworkMessages;
using CaeliImperium.ScriptableObjects;
using CaeliImperiumComponents.Bishop;
using CaeliImperiumEntityStates.Bishop;
using EntityStates;
using HarmonyLib;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using R2API;
using R2API.Networking;
using RoR2;
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
using UnityEngine.UIElements;
using static CaeliImperium.Bodies.BishopEvents;
using static UnityEngine.ParticleSystem.PlaybackState;
using static UnityEngine.UI.GridLayoutGroup;

namespace CaeliImperium.Bodies;
public static class BishopEvents
{
    public static GameObject BodyPrefab;
    public static CharacterBody Body;
    public static SurvivorDef Bishop;
    public static SkillFamily Primary;
    public static SkillFamily Secondary;
    public static SkillFamily Utility;
    public static SkillFamily Special;
    public static SkillFamily Sprint;
    public static SkillDef ShotgunFire;
    public static SkillDef AcceleratorFire;
    public static SkillDef SpearSwing;
    public static SkillDef Punch;
    public static SkillDef Dash;
    public static SkillDef SpearSpecial;
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
    public static ModdedProcType IgnoreBishopComponentHealRestrictionProcType;
    public static DamageAPI.ModdedDamageType ParriableDamageType;
    public static DamageAPI.ModdedDamageType ParryDamageType;
    public static DamageAPI.ModdedDamageType GloryKillDamageType;
    public static DamageAPI.ModdedDamageType SuperStunDamageType;
    public static CharacterBodyAPI.ModdedBodyFlag HeavyMonsterBodyFlag;
    public static CharacterBodyAPI.ModdedBodyFlag FodderMonsterBodyFlag;
    public static DamageColorIndex ParriableDamageColor;
    public static float HealFromGloryChampionKillCoeffecient =  1f / 4f;
    public static float HealFromGloryKillCoeffecient = 1f / 32f;
    public static float HealFromGloryHeavyKillCoeffecient = 1f / 8f;
    public static float HealFromWhiffedGloryKillCoeffecient = 0f;
    public static float ReserveHealFromGloryKillCoeffecient = 1f / 3f;
    public static float ReserveHealFromGloryHeavyKillCoeffecient = 0.3f;
    public static float ReserveHealFromGloryChampionKillCoeffecient = 0.5f;
    public static float DefaultOnParryStunDuration = 0.6f;  
    public static int StaggerAmount = 3;
    public static int StaggerAmountForFinalBosses = 5;
    public static float StaggerDuration = 6f;
    public static float StaggerInvincibilityDuration = 0.5f;
    public static float WasStaggeredDuration = 80f;
    public static float MeleeComboDuration = 1f;
    public static Gradient ParriableGradient = GetParriableGradient();
    public static float GlobalProjectileSpeedMultiplier = 0.65f;
    public static float SuperStunDuration = 2f;
    public static float ParriableOverlapAttackSizeMultiplier = 2f;
    public static float ParriableBlastAttackSizeMultiplier = 2f;
    public static float GloryKillForce = 48f;
    private static HashSet<GameObject> _parriableProjectileGhosts = [];
    private static bool init;
    private static List<ILHook> _hooks = new List<ILHook>();
    private static Dictionary<MethodBase, int> keyValuePairs = [];
    private static FixedConditionalWeakTable<CharacterModel, TemporaryOverlayInstance> keyValuePairs2 = [];
    private static FixedConditionalWeakTable<CharacterModel, TemporaryOverlayInstance> keyValuePairs6 = [];
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
    private static Dictionary<string, DroneIndex> GearboxYouHadOneJob = [];
    private static HashSet<string> purgeTheWeak = new HashSet<string> { "BeetleMaster" ,"GipMaster", "GeepMaster", "GupMaster", "JellyfishMaster", "AcidLarvaMaster", "MegaConstructMaster" };
    private static HashSet<string> purgeTheBoring = new HashSet<string> { "SecondarySkillMagazine", "FlatHealth", "Firework", "HealingPotion", "GoldOnHurt", "WardOnLevel", "Missile", "DronesDropDynamite", "BonusGoldPackOnKill", "ExecuteLowHealthElite", "Phasing", "ExtraStatsOnLevelUp", "Thorns", "SprintOutOfCombat", "RegeneratingScrap", "Squid", "ChainLightning", "RandomEquipmentTrigger", "StunAndPierce", "GhostOnKill", "PhysicsProjectile", "MoreMissile", "MeteorAttackOnHighDamage", "ItemDropChanceOnKill", "DroneWeapons", "ShockNearby", "Icicle", "HeadHunter", "BarrageOnBoss", "CritGlassesVoid", "ChainLightningVoid" };
    private static HashSet<string> purgeTheClankers = new HashSet<string> { "Drone1", "FlameDrone", "MissileDrone", "JunkDrone", "MegaDrone", "Turret1", "CopycatDrone", "EquipmentDrone", "BombardmentDrone" };
    public static void Init(GameObject gameObject)
    {
        On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
        GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        On.RoR2.Projectile.ProjectileController.Start += ProjectileController_Start;
        On.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
        On.EntityStates.EntityState.OnEnter += EntityState_OnEnter;
        On.EntityStates.EntityState.OnExit += EntityState_OnExit;
        On.EntityStates.EntityState.FixedUpdate += EntityState_FixedUpdate;
        On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
        On.EntityStates.Bell.BellWeapon.ChargeTrioBomb.OnEnter += ChargeTrioBomb_OnEnter;
        /*On.EntityStates.LemurianBruiserMonster.FireMegaFireball.OnEnter += FireMegaFireball_OnEnter;
        On.EntityStates.LemurianBruiserMonster.FireMegaFireball.OnExit += FireMegaFireball_OnExit;*/
        On.RoR2.GenericSkill.RunRecharge += GenericSkill_RunRecharge;
        IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess1;
        On.RoR2.Projectile.ProjectileManager.InitializeProjectile += ProjectileManager_InitializeProjectile;
        IL.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
        On.RoR2.DirectorCard.IsAvailable += DirectorCard_IsAvailable;
        Run.onRunStartGlobal += Run_onRunStartGlobal;
        IL.RoR2.BlastAttack.CollectHits += BlastAttack_CollectHits;
        On.RoR2.DroneCatalog.SetDroneDefs += DroneCatalog_SetDroneDefs;
        On.RoR2.BodyCatalog.SetBodyPrefabs += BodyCatalog_SetBodyPrefabs;
        if (init) return;
        init = true;
        Body = gameObject.GetComponent<CharacterBody>();
        Body.ApplyFootstepDustPrefab(BaseFootstepDustType.Generic);
        Body.AddModdedBodyFlag(CharacterBodyAPI.AlwaysSprint);
        SkillLocator skillLocator = gameObject.GetComponent<SkillLocator>();
        BishopComponent bishopComponent = gameObject.GetComponent<BishopComponent>();
        skillLocator.SetSprintSkill(bishopComponent.dashSkill);
        /*FirstPersonCameraTargetParams firstPersonCameraTargetParams = gameObject.GetComponent<FirstPersonCameraTargetParams>();
        FirstPersonCharacterCameraParams firstPersonCharacterCameraParams = firstPersonCameraTargetParams.firstPersonCharacterCameraParams;
        FirstPersonCameraController firstPersonCameraController = firstPersonCharacterCameraParams.firstPersonCameraController;
        Transform camera = firstPersonCameraController.transform.Find("Camera");
        PostProcessLayer postProcessLayer = camera.GetComponent<PostProcessLayer>();
        PostProcessLayer postProcessLayer2 = CaeliImperiumAssets.MainCamera.GetComponentInChildren<PostProcessLayer>();
        postProcessLayer.m_Resources = postProcessLayer2.m_Resources;*/
        Bishop = CaeliImperiumAssets.assetBundle.LoadAsset<SurvivorDef>("Assets/CaeliImperium/Bodies/Bishop/Bishop.asset").RegisterSurvivor();
        Primary = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Bishop/sfBishopPrimary.asset").RegisterSkillFamily();
        Secondary = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Bishop/sfBishopSecondary.asset").RegisterSkillFamily();
        Utility = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Bishop/sfBishopUtility.asset").RegisterSkillFamily();
        Special = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Bishop/sfBishopSpecial.asset").RegisterSkillFamily();
        Sprint = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/Bishop/sfBishopSprint.asset").RegisterSkillFamily();
        ShotgunFire = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/Bishop/sdBishopShotgunFire.asset").RegisterSkillDef();
        AcceleratorFire = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/Bishop/sdBishopAcceleratorFire.asset").RegisterSkillDef();
        SpearSwing = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/Bishop/sdBishopSpearSwing.asset").RegisterSkillDef();
        Punch = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/Bishop/sdBishopPunch.asset").RegisterSkillDef();
        Dash = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/Bishop/sdBishopDash.asset").RegisterSkillDef();
        SpearSpecial = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/Bishop/sdBishopSpearSpecial.asset").RegisterSkillDef();
        PrepareParriableAttackCount = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Bishop/bdPrepareParriableAttackCount.asset").RegisterBuffDef();
        Parry = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Bishop/bdParry.asset").RegisterBuffDef();
        Dodge = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Bishop/bdDodge.asset").RegisterBuffDef();
        SpearOrbitLockOn = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Bishop/bdSpearOrbitLockOn.asset").RegisterBuffDef();
        Stagger = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Bishop/bdStagger.asset").RegisterBuffDef();
        StaggerInvincibility = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Bishop/bdStaggerInvincibility.asset").RegisterBuffDef();
        WasStaggered = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Bishop/bdWasStaggered.asset").RegisterBuffDef();
        MeleeCombo = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Bodies/Bishop/bdMeleeCombo.asset").RegisterBuffDef();
        SpearThrowProjectile = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Bishop/CIBishopSpearThrowProjectile.prefab").RegisterProjectile();
        AcceleratorPlasmaProjectile = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Bishop/Weapons/Accelerator/CIAcceleratorPlasmaProjectile.prefab").RegisterProjectile();
        SpearThrowHeadshotExplosionEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Bishop/Effects/SpearThrowHeadshotHitVFX.prefab").RegisterEffect();
        SpearThrowHitEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Bishop/Effects/SpearThrowHitVFX.prefab").RegisterEffect();
        AcceleratorPlasmaImpactEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Bishop/Effects/AcceleratorPlasmaImpact.prefab").RegisterEffect();
        SpearSlashHitEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Bishop/Effects/SpearSlashHitVFX.prefab").RegisterEffect();
        SpearSlamExplosionEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Bishop/Effects/SpearSlamVFX.prefab").RegisterEffect();
        SpearParryEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Bishop/Effects/SpearParryVFX.prefab").RegisterEffect();
        AboutToAttackParryEffect = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Bishop/Effects/AboutToAttackParriable.prefab").RegisterEffect();
        SpearCrosshair = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Bishop/BishopSpearInputsCrosshair.prefab");
        ShotgunBulletPattern = CaeliImperiumAssets.assetBundle.LoadAsset<BulletPatternDef>("Assets/CaeliImperium/Bodies/Bishop/Weapons/Shotgun/bpdShotgun.asset");
        AcceleratorBulletPattern = CaeliImperiumAssets.assetBundle.LoadAsset<BulletPatternDef>("Assets/CaeliImperium/Bodies/Bishop/Weapons/Accelerator/bpdAccelerator.asset");
        SniperTargetViewer sniperTargetViewer = SpearCrosshair.AddComponent<SniperTargetViewer>();
        sniperTargetViewer.visualizerPrefab = CaeliImperiumAssets.LightSniperTargetVisualizer;
        IgnoreBishopComponentHealRestrictionProcType = ProcTypeAPI.ReserveProcType();
        ParriableDamageType = DamageAPI.ReserveDamageType();
        ParryDamageType = DamageAPI.ReserveDamageType();
        GloryKillDamageType = DamageAPI.ReserveDamageType();
        SuperStunDamageType = DamageAPI.ReserveDamageType();
        BishopSpearProjectileComponent bishopSpearProjectileComponent = SpearThrowProjectile.GetComponent<BishopSpearProjectileComponent>();
        bishopSpearProjectileComponent.damageTypeCombo.AddModdedDamageType(SuperStunDamageType);
        HeavyMonsterBodyFlag = CharacterBodyAPI.ReserveBodyFlag();
        FodderMonsterBodyFlag = CharacterBodyAPI.ReserveBodyFlag();
        ParriableDamageColor = ColorsAPI.RegisterDamageColor(new Color(0f, 1f, 0.4f));
        R2API.Networking.NetworkingAPI.RegisterMessageType<BishopRechargeSpecialSpearSkillsNetMessage>();
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
        AddParriableProjectileGhost(CaeliImperiumAssets.FlyingVerminSpitProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.MinorConstructProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.BellBallProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.BellBallSmallProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.GreaterWispProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.HermitCrabProjectileGhost);
        AddParriableProjectileGhost(CaeliImperiumAssets.HermitCrabProjectileOptGhost);
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
        PatchOverlapStateToParriable(typeof(EntityStates.BasicMeleeAttack), nameof(EntityStates.BasicMeleeAttack.AuthorityFireAttack), typeof(EntityStates.ImpMonster.DoubleSlash));
        AddParriableStateVisuals<EntityStates.GrandParentBoss.GroundSwipe>();
        PatchOverlapStateToParriable(typeof(EntityStates.BasicMeleeAttack), nameof(EntityStates.BasicMeleeAttack.AuthorityFireAttack), typeof(EntityStates.GrandParentBoss.GroundSwipe));
        AddParriableStateVisuals<EntityStates.BrotherMonster.SprintBash>();
        PatchOverlapStateToParriable(typeof(EntityStates.BasicMeleeAttack), nameof(EntityStates.BasicMeleeAttack.AuthorityFireAttack), typeof(EntityStates.BrotherMonster.SprintBash));
        AddParriableStateVisuals<EntityStates.BrotherMonster.WeaponSlam>();
        PatchOverlapStateToParriable(typeof(EntityStates.BrotherMonster.WeaponSlam), nameof(EntityStates.BrotherMonster.WeaponSlam.FixedUpdate));
        PatchBlastAttackStateToParriable(typeof(EntityStates.BrotherMonster.WeaponSlam), nameof(EntityStates.BrotherMonster.WeaponSlam.FixedUpdate));
        AddParriableStateVisuals<EntityStates.GolemMonster.ClapState>();
        PatchBlastAttackStateToParriable(typeof(EntityStates.GolemMonster.ClapState), nameof(EntityStates.GolemMonster.ClapState.FixedUpdate));
        AddParriableStateVisuals<EntityStates.ParentMonster.GroundSlam>();
        PatchBlastAttackStateToParriable(typeof(EntityStates.ParentMonster.GroundSlam), nameof(EntityStates.ParentMonster.GroundSlam.FixedUpdate));
        AddParriableStateVisuals<EntityStates.Wisp1Monster.ChargeEmbers>();
        PatchBulletAttackStateToParriable(typeof(EntityStates.Wisp1Monster.FireEmbers), nameof(EntityStates.Wisp1Monster.FireEmbers.OnEnter));
        List<GameObject> gameObjects = [CaeliImperiumAssets.VultureHunterBody, CaeliImperiumAssets.TitanGoldBody, CaeliImperiumAssets.FalseSonBossBody, CaeliImperiumAssets.FalseSonBossBrokenLunarShardBody, CaeliImperiumAssets.FalseSonBossBrokenLunarShardBody, CaeliImperiumAssets.FalseSonBossLunarShardBody, CaeliImperiumAssets.FalseSonBossBrokenLunarShardBody, CaeliImperiumAssets.BrotherBody, CaeliImperiumAssets.BrotherHurtBody, CaeliImperiumAssets.SolusHeartBody, CaeliImperiumAssets.SolusWingBody, CaeliImperiumAssets.MiniVoidRaidCrabBaseBody, CaeliImperiumAssets.MiniVoidRaidCrabPhase1Body, CaeliImperiumAssets.MiniVoidRaidCrabPhase2Body, CaeliImperiumAssets.MiniVoidRaidCrabPhase3Body];
        foreach (GameObject gameObject1 in gameObjects)
        {
            BishopStaggerOnHurt bishopStaggerOnHurt = gameObject1.EnsureComponent<BishopStaggerOnHurt>();
            bishopStaggerOnHurt.staggerAmount = StaggerAmountForFinalBosses;
            bishopStaggerOnHurt.staggerDuration = StaggerDuration;
        }
    }
    private static void BodyCatalog_SetBodyPrefabs(On.RoR2.BodyCatalog.orig_SetBodyPrefabs orig, GameObject[] newBodyPrefabs)
    {
        orig(newBodyPrefabs);
        foreach (GameObject gameObject in BodyCatalog.allBodyPrefabs)
        {
            CharacterBody characterBody = gameObject.GetComponent<CharacterBody>();
            if (!characterBody || !characterBody.isChampion) continue;
            BishopStaggerOnHurt bishopStaggerOnHurt = gameObject.GetComponent<BishopStaggerOnHurt>();
            if (!bishopStaggerOnHurt)
            {
                bishopStaggerOnHurt = gameObject.AddComponent<BishopStaggerOnHurt>();
                bishopStaggerOnHurt.staggerAmount = StaggerAmount;
                bishopStaggerOnHurt.staggerDuration = StaggerDuration;
            }
        }
    }
    private static bool IsHeavyMonster(this CharacterBody characterBody)
    {
        if (characterBody.HasModdedBodyFlag(HeavyMonsterBodyFlag)) return true;
        if (characterBody.HasModdedBodyFlag(FodderMonsterBodyFlag)) return false;
        if (characterBody.hullClassification >= HullClassification.Golem) return true;
        return false;
    }
    private static void DroneCatalog_SetDroneDefs(On.RoR2.DroneCatalog.orig_SetDroneDefs orig, DroneDef[] newDroneDefs)
    {
        orig(newDroneDefs);
        foreach (DroneDef droneDef in DroneCatalog.allDroneDefs)
        {
            if (droneDef.name.IsNullOrWhiteSpace()) continue;
            GearboxYouHadOneJob.Add(droneDef.name, droneDef.droneIndex);
        }
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
    private static void Run_onRunStartGlobal(Run obj)
    {
        bool foundBishop;
        foreach (NetworkUser networkUser in NetworkUser.instancesList)
        {
            if (!networkUser) continue;
            SurvivorDef survivorDef = networkUser.GetSurvivorPreference();
            if (!survivorDef) continue;
            if (survivorDef == Bishop)
            {
                foundBishop = true;
                break;
            }
        }
        if (!obj.GetEventFlag("BishopRun"))
        {
            obj.SetEventFlag("BishopRun");
            foreach (string itemName in purgeTheBoring)
            {
                if (itemName.IsNullOrWhiteSpace()) continue;
                ItemIndex itemIndex = ItemCatalog.FindItemIndex(itemName);
                if (itemIndex == ItemIndex.None) continue;
                obj.availableItems.Remove(itemIndex);
            }
            foreach (string droneName in purgeTheClankers)
            {
                if (droneName.IsNullOrWhiteSpace()) continue;
                DroneIndex droneIndex = GearboxYouHadOneJob[droneName];
                if (droneIndex == DroneIndex.None) continue;
                obj.availableDrones.Remove(droneIndex);
            }
            CaeliImperiumPlugin.Log.LogMessage("Set BishopRun Flag");
        }
        else
        {
            CaeliImperiumPlugin.Log.LogMessage("BishopRun Flag Already Set");
        }
    }

    private static bool DirectorCard_IsAvailable(On.RoR2.DirectorCard.orig_IsAvailable orig, DirectorCard self)
    {
        bool flag = orig(self);
        if (!flag) return flag;
        if (Run.instance && Run.instance.GetEventFlag("BishopRun") && self.spawnCard && self.spawnCard.prefab && !self.spawnCard.prefab.name.IsNullOrWhiteSpace() && purgeTheWeak.Contains(self.spawnCard.prefab.name)) return false;
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
    private static void ProjectileManager_InitializeProjectile(On.RoR2.Projectile.ProjectileManager.orig_InitializeProjectile orig, ProjectileController projectileController, FireProjectileInfo fireProjectileInfo)
    {
        if (BishopComponent.enableCount > 0)
        {
            TeamIndex teamIndex = TeamComponent.GetObjectTeam(fireProjectileInfo.owner);
            if (TeamManager.IsTeamEnemy(teamIndex, TeamIndex.Player))
            {
                if (fireProjectileInfo.useSpeedOverride)
                {
                    fireProjectileInfo.speedOverride *= GlobalProjectileSpeedMultiplier;
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
                            fireProjectileInfo.speedOverride = speed * GlobalProjectileSpeedMultiplier;
                        }
                    }
                }
            }
            
        }
        orig(projectileController, fireProjectileInfo);
    }

    private static void HealthComponent_TakeDamageProcess1(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        int locID = 15;
        if (
            !c.TryGotoNext(MoveType.Before,
                x => x.MatchNewobj<DamageReport>(),
                x => x.MatchStloc(out locID)
            ))
        {
            CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 1 failed!");
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
            CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 2 failed!");
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
        if (BishopComponent.enableCount > 0 && !characterBody.HasBuff(WasStaggered))
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
    private static void GenericSkill_RunRecharge(On.RoR2.GenericSkill.orig_RunRecharge orig, GenericSkill self, float dt)
    {
        if (self && self.characterBody && self.skillDef && self.skillDef is BishopSkillDef bishopSkillDef && bishopSkillDef.rechargeOnlyOnGround && self.characterBody.characterMotor && !self.characterBody.characterMotor.isGrounded) // This if is so stupidly long but funny
        {
            dt = 0f;
        }
        orig(self, dt);
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
            if (self.body.HasBuff(StaggerInvincibility) && !damageInfo.damageType.HasModdedDamageType(GloryKillDamageType))
            {
                return;
            }
            if (damageInfo.damageType.HasModdedDamageType(ParriableDamageType) && self.body.HasBuff(Parry))
            {
                CharacterBody attackerBody = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
                if (attackerBody)
                {
                    OnParry(self.body, damageInfo.position);
                    return;
                }
            }
        }
        orig(self, damageInfo);
    }
    public static float ParryEffectScale = 3f;
    public static void OnParry(CharacterBody bodyThatParries, Vector3 parryPosition)
    {
        BishopSpecialSpearSkillDef.RechargeStocksForSpecialSpearSkills(bodyThatParries);
        BishopRechargeSpecialSpearSkillsNetMessage.SendToClients(0, bodyThatParries.netId);
        EffectData effectData = new EffectData
        {
            scale = ParryEffectScale,
            origin = parryPosition
        };
        EffectManager.SpawnEffect(SpearParryEffect.index, effectData, true);
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
    private static void PatchProjectileStateToParriable(Type type, string name, Type type1, int parriableCount) => PatchProjectileStateToParriable(type, name, type1, parriableCount, null);
    private static void PatchProjectileStateToParriable(Type type, string name, int parriableCount) => PatchProjectileStateToParriable(type, name, type, parriableCount, null);
    private static void PatchProjectileStateToParriable(Type type, string name, int parriableCount, Predicate<ParriableProjectileStuff> customParriableProjectile) => PatchProjectileStateToParriable(type, name, type, parriableCount, customParriableProjectile);
    private static void PatchProjectileStateToParriable(Type type, string name, Predicate<ParriableProjectileStuff> customParriableProjectile) => PatchProjectileStateToParriable(type, name, type, 0, customParriableProjectile);
    private static void PatchProjectileStateToParriable(Type type, string name, Type type1, Predicate<ParriableProjectileStuff> customParriableProjectile) => PatchProjectileStateToParriable(type, name, type1, 0, customParriableProjectile);
    private static void PatchProjectileStateToParriable(Type type, string name, Type type1, int parriableCount, Predicate<ParriableProjectileStuff> customParriableProjectile)
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
    private static void PatchOverlapStateToParriable(Type type, string name) => PatchOverlapStateToParriable(type, name, type);
    private static void PatchOverlapStateToParriable(Type type, string name, Type type1)
    {
        if (!patchedOverlapStates.Contains(type))
        {
            MethodBase methodBase = AccessTools.Method(type, name);
            _hooks.Add(new ILHook(methodBase, HandleFireOverlapAttack));
            patchedOverlapStates.Add(type);
        }
        types3.Add(type1);
    }
    private static void PatchBlastAttackStateToParriable(Type type, string name) => PatchBlastAttackStateToParriable(type, name, type);
    private static void PatchBlastAttackStateToParriable(Type type, string name, Type type1)
    {
        if (!patchedBlastStates.Contains(type))
        {
            MethodBase methodBase = AccessTools.Method(type, name);
            _hooks.Add(new ILHook(methodBase, HandleFireBlastAttack));
            patchedBlastStates.Add(type);
        }
        types3.Add(type1);
    }
    private static void PatchBulletAttackStateToParriable(Type type, string name) => PatchBulletAttackStateToParriable(type, name, type);
    private static void PatchBulletAttackStateToParriable(Type type, string name, Type type1)
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
        //GradientColorKey[] gradientColorKeys = { new GradientColorKey { color = new Color(0.7969813f, 0f, 1f), time = 0f, }, new GradientColorKey { color = new Color(0f, 1f, 0.438499f), time = 1f, } };
        GradientColorKey[] gradientColorKeys = { new GradientColorKey { color = new Color(114f / 256f, 166f / 256f, 36f / 256f), time = 0f, }, new GradientColorKey { color = new Color(0f, 1f, 0.438499f), time = 1f, } };
        gradient.SetKeys(gradientColorKeys, gradientAlphaKeys);
        return gradient;
    }
    private static void ProjectileController_Start(On.RoR2.Projectile.ProjectileController.orig_Start orig, ProjectileController self)
    {
        orig(self);
        ProjectileGhostController projectileGhostController = self.ghost;
        if (!projectileGhostController) return;
        ParriableProjectileGhostColorChanger parriableProjectileGhostColorChanger = projectileGhostController.GetComponent<ParriableProjectileGhostColorChanger>();
        if (!parriableProjectileGhostColorChanger) return;
        ProjectileDamage projectileDamage = self.GetComponent<ProjectileDamage>();
        if (!projectileDamage) return;
        if (projectileDamage.damageColorIndex == ParriableDamageColor)
        {
            parriableProjectileGhostColorChanger.ApplyParriableMaterial();
            RoR2.Util.PlaySound("Play_DoomTDA_ParryableProjectile_Loop", self.gameObject);
        }
        else
        {
            parriableProjectileGhostColorChanger.ApplyDefaultMaterial();
        }
    }
    
    private static void GlobalEventManager_onServerDamageDealt(DamageReport obj)
    {
        DamageInfo damageInfo = obj.damageInfo;
        if (damageInfo == null) return;
        CharacterBody victimBody = obj.victimBody;
        if (damageInfo.HasModdedDamageType(SuperStunDamageType))
        {
            victimBody.SuperStun(SuperStunDuration * damageInfo.procCoefficient);
        }
        bool hasDamageType = damageInfo.HasModdedDamageType(GloryKillDamageType);
        CharacterBody attackerBody = obj.attackerBody;
        if (attackerBody)
        {
            if (victimBody.HasBuff(Stagger))
            {
                ProcChainMask procChainMask = new ProcChainMask();
                float healCoof = hasDamageType ? victimBody.isChampion ? HealFromGloryChampionKillCoeffecient : victimBody.IsHeavyMonster() ? HealFromGloryHeavyKillCoeffecient :  HealFromGloryKillCoeffecient : HealFromWhiffedGloryKillCoeffecient;
                float reserveHealCoof = ReserveHealFromGloryKillCoeffecient;
                if (BishopStaggerOnHurt.keyValuePairs.TryGetValue(obj, out BishopStaggerOnHurt bishopStaggerOnHurt))
                {
                    if (bishopStaggerOnHurt.overrideGloryKillHeal) healCoof = bishopStaggerOnHurt.overrideGloryKillHealFraction;
                    if (bishopStaggerOnHurt.overrideGloryKillHealFromReserve) reserveHealCoof = bishopStaggerOnHurt.overrideGloryKillHealFromReserveFraction;
                }
                procChainMask.AddModdedProc(IgnoreBishopComponentHealRestrictionProcType);
                float healAmount = attackerBody.healthComponent.fullHealth * healCoof;
                float predictedHealth = attackerBody.healthComponent.health + healAmount;
                BishopComponent bishopComponent = attackerBody.GetComponent<BishopComponent>();
                if (hasDamageType && bishopComponent && bishopComponent.reservedHealPercentage > 0f)
                {
                    float reservedHeal = Mathf.Min(bishopComponent.reservedHealPercentage * attackerBody.healthComponent.fullHealth, attackerBody.healthComponent.fullHealth * reserveHealCoof);
                    float predictedHealth2 = predictedHealth + healAmount;
                    float overHeal = predictedHealth2 - attackerBody.healthComponent.fullHealth;
                    if (overHeal > 0f) reservedHeal -= overHeal;
                    if (reservedHeal > 0f)
                    {
                        bishopComponent.reservedHealPercentage -= reservedHeal;
                        healAmount += reservedHeal;
                    }
                }
                attackerBody.healthComponent.Heal(healAmount, procChainMask);
                if (hasDamageType) victimBody.SetBuffCount(Stagger.buffIndex, 0);
            }
        }
        if (victimBody && victimBody.healthComponent && !victimBody.healthComponent.alive && victimBody.HasBuff(Stagger) && hasDamageType)
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
        /*if (victimBody)
        {
            DamageSource damageSource = damageInfo.damageType.damageSource;
            if (victimBody.HasBuff(Stagger) && (damageSource.HasFlag(DamageSource.Primary) || damageSource.HasFlag(DamageSource.Secondary) || damageSource.HasFlag(DamageSource.Utility) || damageSource.HasFlag(DamageSource.Special)))
            {
                victimBody.RemoveBuff(Stagger);
                if (victimBody.HasBuff(StaggerInvincibility))
                {
                    victimBody.RemoveBuff(StaggerInvincibility);
                }
            }
        }*/
        /*if (victimBody)
        {
            if (damageInfo.HasModdedDamageType(ParryDamageType))
            {
                if (keyValuePairs3.TryGetValue(victimBody, out HashSet<BaseState> baseStates))
                {
                    foreach (BaseState state in baseStates)
                    {
                        Type type = state.GetType();
                        if (keyValuePairs4.TryGetValue(type, out OnBodyParried onBodyParried))
                        {
                            onBodyParried.Invoke(obj, state);
                        }
                        else
                        {
                            StunState stunState = new StunState();
                            stunState.stunDuration = DefaultOnParryStunDuration;
                            state.outer.SetNextState(stunState);
                        }
                    }
                }
            }
        }
        if (!attackerBody) return;
        if (!attackerBody.healthComponent) return;
        if (damageInfo.HasModdedDamageType(HealMeleeDamageType))
        {
            ProcChainMask procChainMask = new ProcChainMask();
            procChainMask.AddModdedProc(HealMeleeProcType);
            float healAmount = attackerBody.healthComponent.fullHealth * HealMeleeCooffecient;
            BishopComponent bishopComponent = attackerBody.GetComponent<BishopComponent>();
            if (bishopComponent && bishopComponent.reservedHeal > 0f)
            {
                float reservedHeal = Mathf.Min(bishopComponent.reservedHeal, attackerBody.healthComponent.fullHealth * ReserveHealMeleeCooffecient);
                bishopComponent.reservedHeal -= reservedHeal;
                healAmount += reservedHeal;
            }
            attackerBody.healthComponent.Heal(healAmount, procChainMask);
        }*/
        

    }
    
    private static float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
    {
        if (nonRegen && self)
        {
            BishopComponent bishopComponent = self.GetComponent<BishopComponent>();
            if (bishopComponent)
            {
                if (!procChainMask.HasModdedProc(IgnoreBishopComponentHealRestrictionProcType))
                {
                    bishopComponent.SetReservedHealPercentage(bishopComponent.reservedHealPercentage + (amount / self.fullHealth));
                    return 0f;
                }
            }
        }
        float heal = orig(self, amount, procChainMask, nonRegen);
        return heal;
    }
    public static void AddParriableProjectileGhost(GameObject projectileGhost)
    {
        if (_parriableProjectileGhosts.Contains(projectileGhost)) return;
        ParriableProjectileGhostColorChanger parriableProjectileGhostColorChanger = projectileGhost.AddComponent<ParriableProjectileGhostColorChanger>();
        parriableProjectileGhostColorChanger.Init(ParriableGradient);
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
        if (c.TryGotoNext(MoveType.Before,
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
            return;
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
            if (BishopComponent.enableCount <= 0) return parriableProjectileStuff;
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
        if (BishopComponent.enableCount <= 0) return fireProjectileInfo;
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
