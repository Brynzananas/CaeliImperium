using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static CaeliImperium.Bodies.BishopEvents;

namespace CaeliImperium;
public static class EnemiesReturnsCompatability
{
    public const string GUID = "com.Viliger.EnemiesReturns";
    private static bool init;
    public static void BishopInit()
    {
        if (init) return;
        init = true;
        RoR2Application.onLoadFinished += OnLoadFinished;
        PatchProjectileStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.MechanicalSpider.DoubleShot.BaseFire), nameof(EnemiesReturns.ModdedEntityStates.MechanicalSpider.DoubleShot.BaseFire.FireProjectile), typeof(EnemiesReturns.ModdedEntityStates.MechanicalSpider.DoubleShot.Enemy.Fire), 0);
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.MechanicalSpider.DoubleShot.Enemy.Fire>();
        PatchProjectileStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.LynxTribe.Archer.FireArrow), nameof(EnemiesReturns.ModdedEntityStates.LynxTribe.Archer.FireArrow.FireProjectile), 0);
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.LynxTribe.Archer.FireArrow>();
        PatchProjectileStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.LynxTribe.Shaman.SummonTrackingProjectilesShotgun), nameof(EnemiesReturns.ModdedEntityStates.LynxTribe.Shaman.SummonTrackingProjectilesShotgun.FixedUpdate), 0);
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.LynxTribe.Shaman.SummonTrackingProjectilesShotgun>();
        PatchProjectileStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.LynxTribe.Totem.GroundpoundProjectile), nameof(EnemiesReturns.ModdedEntityStates.LynxTribe.Totem.GroundpoundProjectile.FixedUpdate), 0);
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.LynxTribe.Totem.GroundpoundProjectile>();
        PatchProjectileStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.ArcherBugs.FireCausticSpit), nameof(EnemiesReturns.ModdedEntityStates.ArcherBugs.FireCausticSpit.FireAttackAuthority), 0);
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.ArcherBugs.FireCausticSpit>();
        PatchProjectileStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.Spitter.FireChargedSpit), nameof(EnemiesReturns.ModdedEntityStates.Spitter.FireChargedSpit.FireProjectile), 0);
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.Spitter.FireChargedSpit>();
        PatchProjectileStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.Colossus.HeadLaserBarrage.HeadLaserBarrageAttack), nameof(EnemiesReturns.ModdedEntityStates.Colossus.HeadLaserBarrage.HeadLaserBarrageAttack.Fire), 0);
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.Colossus.HeadLaserBarrage.HeadLaserBarrageStart>();
        PatchProjectileStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.Colossus.RockClap.RockClapEnd), nameof(EnemiesReturns.ModdedEntityStates.Colossus.RockClap.RockClapEnd.FireProjectiles), 0);
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.Colossus.RockClap.RockClapStart>();
        PatchOverlapStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.LynxTribe.Hunter.Lunge.FireLunge), nameof(EnemiesReturns.ModdedEntityStates.LynxTribe.Hunter.Lunge.FireLunge.FixedUpdate));
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.LynxTribe.Hunter.Lunge.ChargeLunge>();
        PatchOverlapStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.LynxTribe.Scout.DoubleSlash), nameof(EnemiesReturns.ModdedEntityStates.LynxTribe.Scout.DoubleSlash.FixedUpdate));
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.LynxTribe.Scout.DoubleSlash>();
        PatchOverlapStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.Swift.Dive.Dive), nameof(EnemiesReturns.ModdedEntityStates.Swift.Dive.Dive.FixedUpdate));
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.Swift.Dive.DivePrep>();
        PatchOverlapStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.Spitter.Bite), nameof(EnemiesReturns.ModdedEntityStates.Spitter.Bite.Fire));
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.Spitter.Bite>();
        PatchOverlapStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.SandCrab.Snip.FireSnip), nameof(EnemiesReturns.ModdedEntityStates.SandCrab.Snip.FireSnip.FixedUpdate));
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.SandCrab.Snip.ChargeSnip>();
        PatchOverlapStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.Colossus.Stomp.StompBase), nameof(EnemiesReturns.ModdedEntityStates.Colossus.Stomp.StompBase.FixedUpdate), typeof(EnemiesReturns.ModdedEntityStates.Colossus.Stomp.StompL));
        PatchOverlapStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.Colossus.Stomp.StompBase), nameof(EnemiesReturns.ModdedEntityStates.Colossus.Stomp.StompBase.FixedUpdate), typeof(EnemiesReturns.ModdedEntityStates.Colossus.Stomp.StompR));
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.Colossus.Stomp.StompBase>();
        PatchBlastAttackStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.Colossus.RockClap.RockClapEnd), nameof(EnemiesReturns.ModdedEntityStates.Colossus.RockClap.RockClapEnd.FireProjectiles));
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.Colossus.RockClap.RockClapStart>();
        PatchBlastAttackStateToParriable(typeof(EnemiesReturns.ModdedEntityStates.Ifrit.Pillar.BaseFireExplosion), nameof(EnemiesReturns.ModdedEntityStates.Ifrit.Pillar.BaseFireExplosion.OnEnter), typeof(EnemiesReturns.ModdedEntityStates.Ifrit.Pillar.Enemy.FireExplosion));
        AddParriableStateVisuals<EnemiesReturns.ModdedEntityStates.Ifrit.Pillar.Enemy.FireExplosion>();
    }

    private static void OnLoadFinished()
    {
        AddParriableProjectileGhostFromProjectile(EnemiesReturns.ModdedEntityStates.MechanicalSpider.DoubleShot.BaseFire.projectilePrefab);
        AddParriableProjectileGhostFromProjectile(EnemiesReturns.ModdedEntityStates.LynxTribe.Archer.FireArrow.projectilePrefab);
        SetCustomProjectileSpeedMultiplier(EnemiesReturns.ModdedEntityStates.LynxTribe.Archer.FireArrow.projectilePrefab, 1f);
        AddParriableProjectileGhostFromProjectile(EnemiesReturns.ModdedEntityStates.LynxTribe.Shaman.SummonTrackingProjectilesShotgun.trackingProjectilePrefab);
        AddParriableProjectileGhostFromProjectile(EnemiesReturns.ModdedEntityStates.LynxTribe.Totem.GroundpoundProjectile.groundpoundProjectilePrefab);
        AddParriableProjectileGhostFromProjectile(EnemiesReturns.ModdedEntityStates.ArcherBugs.FireCausticSpit.projectilePrefab);
    }
}
