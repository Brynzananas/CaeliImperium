using CaeliImperium;
using CaeliImperium.Bodies;
using CaeliImperiumScriptableObjects;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.Bishop;
public class ShootAccelerator : BishopRightWeaponState
{
    public static float damageCoefficient = 0.5f;
    public static float superchargeDamageCoefficient = 1.25f;
    public static float procCoefficient = 1f;
    public static float baseDuration = 0.08f;
    public static float durationMultiplierWhenOrbiting = 0.7f;
    public static bool allowTrajectoryAimAssist = true;
    public static float spread = 1f;
    public static float chargePerShot = 100f / 30f;
    public static float chargeNeededForSupercharge = 100f;
    public static DamageType damageType = DamageType.Generic;
    public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
    public static BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.None;
    public static float force = 0f;
    public static float radius = 0.1f;
    public static float trajectoryAimAssistMultiplier = 1f;
    public static float minVerticalRecoil = -0.4f;
    public static float maxVerticalRecoil = -0.8f;
    public static float maxHorizontalRecoil = -0.3f;
    public static float minHorizontalRecoil = 0.3f;
    public static float spreadBloom = 0.5f;
    public static float shakeFrequency = 12f;
    public static float shakeDuration = 0.4f;
    public static float shakeRadius = 3f;
    public static float shakeAmplitude = 0.5f;
    public static Vector3 offsetRayOrigin = new Vector3(0.5f, -0.2894f, 0.5f);
    public float damage;
    public float duration;
    public float stopwatch;
    public int shotCount;
    public float charge;
    public bool supercharged;
    public bool supercharge => charge >= chargeNeededForSupercharge;
    public override float durationForStateModification => duration;
    public override float fixedAgeForStateModification => stopwatch;

    public override void OnEnter()
    {
        base.OnEnter();
        SetValues();
        Fire();
    }
    public override void OnExit()
    {
        base.OnExit();
        if (supercharged)
        {
            Util.PlaySound("Stop_DoomTDA_Accelerator_Fire_Loop", gameObject);
        }
    }
    public void SetValues()
    {
        duration = baseDuration / characterBody.attackSpeed;
        if (characterBody.HasBuff(BishopEvents.SpearOrbitLockOn)) duration *= durationMultiplierWhenOrbiting;
        if (skillLocator) charge = BishopAcceleratorSkillDef.GetTotalCharge(skillLocator);
        damage = (supercharge ? superchargeDamageCoefficient : damageCoefficient) * characterBody.damage;
        if (!supercharged && supercharge)
        {
            supercharged = true;
            Util.PlaySound("Play_DoomTDA_Accelerator_Adrenaline_Activate", gameObject);
            Util.PlaySound("Play_DoomTDA_Accelerator_Fire_Loop", gameObject);
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        stopwatch += GetDeltaTime();
        if (stopwatch >= duration)
        {
            stopwatch = 0f;
            SetValues();
            Fire();
        }
        if (!isAuthority) return;
        if (!IsKeyDownAuthority()) outer.SetNextState(new ExitAccelerator { activatorSkillSlot = activatorSkillSlot, supercharge = supercharge });
    }
    public void Fire()
    {
        shotCount++;
        (this as IBishopState).PlayFirstPersonCrossfade("RightArm, Override", "FireAccelerator", "rightArm.playbackRate", 1f, 0.05f, true);
        Util.PlaySound("Play_DoomTDA_Accelerator_Fire", gameObject);
        Util.PlaySound(supercharge ? "Play_DoomTDA_Accelerator_Fire_Supercharge" : "Play_DoomTDA_Accelerator_Fire_Stabilizer", gameObject);
        Ray ray = GetAimRay();
        StartAimMode(ray);
        AddRecoil(minVerticalRecoil, maxVerticalRecoil, minHorizontalRecoil, maxHorizontalRecoil);
        (this as IBishopState).Shake(shakeDuration, shakeRadius, shakeFrequency, shakeAmplitude);
        if (activatorSkillSlot) BishopAcceleratorSkillDef.AddCharge(activatorSkillSlot, chargePerShot);
        if (!isAuthority) return;
        Quaternion quaternion = Util.QuaternionSafeLookRotation(ray.direction);
        CameraRigController cameraRigController = characterBody.GetCameraRigController();
        if (cameraRigController)
        {
            Vector3 rotatedOffset = quaternion * offsetRayOrigin;
            ray.origin += rotatedOffset;
            ray.direction = (cameraRigController.crosshairWorldPosition - ray.origin).normalized;
        }
        if (allowTrajectoryAimAssist) TrajectoryAimAssist.ApplyTrajectoryAimAssist(ref ray, BishopEvents.AcceleratorPlasmaProjectile, gameObject, trajectoryAimAssistMultiplier);
        ray.direction = BishopEvents.AcceleratorBulletPattern.GetAimRay(characterBody, ray.direction, spread);
        FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
        {
            projectilePrefab = BishopEvents.AcceleratorPlasmaProjectile,
            damage = damage,
            crit = RollCrit(),
            damageColorIndex = DamageColorIndex.Default,
            force = force,
            damageTypeOverride = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource(DamageSource.Primary)),
            owner = gameObject,
            position = ray.origin,
            rotation = Util.QuaternionSafeLookRotation(ray.direction)
        };
        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        characterBody.AddSpreadBloom(spreadBloom);
    }
}
