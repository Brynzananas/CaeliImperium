using BrynzaAPI;
using CaeliImperium;
using CaeliImperium.Bodies;
using CaeliImperiumScriptableObjects;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates.Bishop;
public class ExitAccelerator : BishopRightWeaponState
{
    public static float damageCoefficient = 5f;
    public static float procCoefficient = 1f;
    public static float minSpread = 0f;
    public static float maxSpread = 0f;
    public static float baseDuration = 0.5f;
    public static bool allowTrajectoryAimAssist = false;
    public static uint bulletCount = 1;
    public static DamageType damageType = DamageType.SlowOnHit;
    public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
    public static BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.None;
    public static float force = 900f;
    public static float selfVelocity = 12f;
    public static float radius = 5f;
    public static float trajectoryAimAssistMultiplier = 0.75f;
    public static bool smartCollision = true;
    public static float maxDistance = 6f;
    public static PhysForceFlags physForceFlags = PhysForceFlags.None;
    public static float minVerticalRecoil = -0.4f;
    public static float maxVerticalRecoil = -0.8f;
    public static float maxHorizontalRecoil = -0.3f;
    public static float minHorizontalRecoil = 0.3f;
    public static float spreadBloom = 1.5f;
    public static float shakeFrequency = 12f;
    public static float shakeDuration = 0.7f;
    public static float shakeRadius = 6f;
    public static float shakeAmplitude = 2f;
    public bool supercharge;
    public float duration;
    public float damage;
    public override float durationForStateModification => duration;
    public override float fixedAgeForStateModification => fixedAge;

    public override void OnSerialize(NetworkWriter writer)
    {
        base.OnSerialize(writer);
        writer.Write(supercharge);
    }
    public override void OnDeserialize(NetworkReader reader)
    {
        base.OnDeserialize(reader);
        supercharge = reader.ReadBoolean();
    }
    public override void OnEnter()
    {
        base.OnEnter();
        SetValues();
        (this as IBishopState).PlayFirstPersonCrossfade("RightArm, Override", "ExitAccelerator", "rightArm.playbackRate", 1f, 0.05f, true);
        if (supercharge) Fire();
    }
    public void SetValues()
    {
        damage = damageCoefficient * characterBody.damage;
        duration = baseDuration / characterBody.attackSpeed;
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isAuthority || fixedAge < duration) return;
        outer.SetNextStateToMain();
    }
    public void Fire()
    {
        Ray ray = GetAimRay();
        StartAimMode(ray);
        AddRecoil(minVerticalRecoil, maxVerticalRecoil, minHorizontalRecoil, maxHorizontalRecoil);
        Util.PlaySound("Play_DoomTDA_Accelerator_Heatblast", gameObject);
        (this as IBishopState).Shake(shakeDuration, shakeRadius, shakeFrequency, shakeAmplitude);
        if (!isAuthority) return;
        this.AddVelocity(ray.direction * -1f * selfVelocity);
        if (activatorSkillSlot) BishopAcceleratorSkillDef.RemoveAllCharge(activatorSkillSlot);
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
            procCoefficient = procCoefficient,
            stopperMask = LayerIndex.ui.mask,
            hitMask = LayerIndex.entityPrecise.mask
        };
        bulletAttack.Fire();
        characterBody.AddSpreadBloom(spreadBloom);
    }
}
