using CaeliImperium;
using CaeliImperium.Bodies;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.Doomguy;
public class SpearThrow : DoomguyState
{
    public static float damageCoefficient = 20f;
    public static float baseDuration = 0.4f;
    public static float force = 300f;
    public static DamageType damageType = DamageType.Generic;
    public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
    public float damage;
    public float duration;
    public override void OnEnter()
    {
        base.OnEnter();
        SetValues();
        FireSpear();
    }
    public void SetValues()
    {
        damage = characterBody.damage * damageCoefficient;
        duration = baseDuration / characterBody.attackSpeed;
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isAuthority || fixedAge < duration) return;
        outer.SetNextStateToMain();
    }
    public void FireSpear()
    {
        (this as IDoomguyState).PlayFirstPersonCrossfade("LeftArm, Override", "SpearThrow", "leftArm.playbackRate", attackSpeedStat, 0.05f, true);
        Util.PlaySound("Play_DoomTDA_Spear_Throw", gameObject);
        Ray ray = GetAimRay();
        StartAimMode(ray);
        if (!isAuthority) return;
        FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
        {
            projectilePrefab = DoomguyEvents.SpearThrowProjectile,
            damage = damage,
            crit = RollCrit(),
            damageColorIndex = DamageColorIndex.Default,
            force = force,
            damageTypeOverride = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource(DamageSource.Special)),
            owner = gameObject,
            position = ray.origin,
            rotation = Util.QuaternionSafeLookRotation(ray.direction)
        };
        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
    }
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
}
