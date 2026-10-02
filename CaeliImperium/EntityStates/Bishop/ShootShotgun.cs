using BrynzaAPI;
using CaeliImperium;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.Bishop;
public class ShootShotgun : BishopState
{
    public static float baseDamageCoefficient = 1f;
    public static float procCoefficient = 0.5f;
    public static float minSpread = 0f;
    public static float maxSpread = 3f;
    public static float baseDuration = 0.8f;
    public static bool allowTrajectoryAimAssist = true;
    public static uint bulletCount = 5;
    public static DamageType damageType = DamageType.Generic;
    public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
    public static BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.None;
    public static float force = 300f;
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
    public static float shakeAmplitude = 1f;
    public float damage;
    public float duration;
    public override void OnEnter()
    {
        base.OnEnter();
        SetValues();
        Fire();
    }
    public void SetValues()
    {
        damage = baseDamageCoefficient * characterBody.damage;
        duration = baseDuration / characterBody.attackSpeed/ (isAuthority ? (float)(characterBody.GetClientBuffCount(RoR2Content.Buffs.NoCooldowns) + 1) : 1f);
    }
    public void Fire()
    {
        Ray ray = GetAimRay();
        StartAimMode(ray);
        AddRecoil(minVerticalRecoil, maxVerticalRecoil, minHorizontalRecoil, maxHorizontalRecoil);
        (this as IBishopState).PlayFirstPersonCrossfade("RightArm, Override", "Fire", "rightArm.playbackRate", 1f, 0.05f, true);
        Util.PlaySound("Play_DoomTDA_CombatShotgun_Fire", gameObject);
        (this as IBishopState).Shake(shakeDuration, shakeRadius, shakeFrequency, shakeAmplitude);
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
        bulletAttack.Fire();
        characterBody.AddSpreadBloom(spreadBloom);
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isAuthority || fixedAge < duration) return;
        outer.SetNextStateToMain();
    }
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
}
