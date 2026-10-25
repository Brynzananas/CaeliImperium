using BrynzaAPI;
using CaeliImperium;
using CaeliImperium.Bodies;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.Doomguy;
public class ShootShotgun : DoomguyRightWeaponState
{
    public static float damageCoefficient = 1.2f;
    public static float procCoefficient = 1f;
    public static float minSpread = 15f;
    public static float maxSpread = 15f;
    public static float baseDuration = 0.8f;
    public static float durationMultiplierWhenOrbiting = 0.7f;
    public static bool allowTrajectoryAimAssist = true;
    public static uint bulletCount = 5;
    public static DamageType damageType = DamageType.Generic;
    public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
    public static BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.Slug;
    public static float force = 0f;
    public static float radius = 0.1f;
    public static float trajectoryAimAssistMultiplier = 0.75f;
    public static bool smartCollision = true;
    public static float maxDistance = 256f;
    public static PhysForceFlags physForceFlags = PhysForceFlags.None;
    public static float minVerticalRecoil = -0.4f;
    public static float maxVerticalRecoil = -0.8f;
    public static float maxHorizontalRecoil = -0.3f;
    public static float minHorizontalRecoil = 0.3f;
    public static float spreadBloom = 1.5f;
    public static float shakeFrequency = 12f;
    public static float shakeDuration = 0.7f;
    public static float shakeRadius = 6f;
    public static float shakeAmplitude = 0.3f;
    public float damage;
    public float duration;
    public override float durationForStateModification => duration;
    public override float fixedAgeForStateModification => fixedAge;

    public override void OnEnter()
    {
        base.OnEnter();
        SetValues();
        Fire();
    }
    public void SetValues()
    {
        damage = damageCoefficient * characterBody.damage;
        duration = baseDuration / characterBody.attackSpeed;
        if (characterBody.HasBuff(DoomguyEvents.SpearOrbitLockOn)) duration *= durationMultiplierWhenOrbiting;
    }
    public void Fire()
    {
        Ray ray = GetAimRay();
        StartAimMode(ray);
        AddRecoil(minVerticalRecoil, maxVerticalRecoil, minHorizontalRecoil, maxHorizontalRecoil);
        (this as IDoomguyState).PlayFirstPersonCrossfade("RightArm, Override", "FireShotgun", "rightArm.playbackRate", 1f, 0.05f, true);
        (this as IDoomguyState).MuzzleFlash();
        Util.PlaySound("Play_DoomTDA_CombatShotgun_Fire", gameObject);
        (this as IDoomguyState).Shake(shakeDuration, shakeRadius, shakeFrequency, shakeAmplitude);
        if (!isAuthority) return;
        BulletAttack bulletAttack = new BulletAttack
        {
            owner = gameObject,
            weapon = gameObject,
            origin = ray.origin,
            aimVector = ray.direction,
            minSpread = minSpread,
            maxSpread = maxSpread,
            damage = damage,
            muzzleName = "Muzzle",
            allowTrajectoryAimAssist = allowTrajectoryAimAssist,
            bulletCount = bulletCount,
            damageType = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource(DamageSource.Primary)),
            falloffModel = falloffModel,
            force = force,
            tracerEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab,
            isCrit = RollCrit(),
            trajectoryAimAssistMultiplier = trajectoryAimAssistMultiplier,
            radius = radius,
            hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
            smartCollision = smartCollision,
            maxDistance = maxDistance,
            physForceFlags = physForceFlags,
            procCoefficient = procCoefficient
        };
        bulletAttack.SetBulletPatternDef(DoomguyEvents.ShotgunBulletPattern);
        bulletAttack.Fire();
        characterBody.AddSpreadBloom(spreadBloom);
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isAuthority || fixedAge < duration) return;
        outer.SetNextStateToMain();
    }
}
