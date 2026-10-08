using BrynzaAPI;
using CaeliImperium;
using CaeliImperium.Bodies;
using CaeliImperium.NetworkMessages;
using CaeliImperiumEntityStates.Bishop;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.SendMouseEvents;

namespace CaeliImperiumEntityStates.Bishop;
public class SwingSpear : BishopState
{
    public static float baseDamageCoefficient = 2f;
    public static float procCoefficient = 1f;
    public static float minSpread = 0f;
    public static float maxSpread = 0f;
    public static float baseDuration = 0.6f;
    public static bool allowTrajectoryAimAssist = false;
    public static uint bulletCount = 1;
    public static DamageType damageType = DamageType.Generic;
    public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
    public static BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.None;
    public static float force = 300f;
    public static float radius = 3f;
    public static float trajectoryAimAssistMultiplier = 0f;
    public static bool smartCollision = true;
    public static float maxDistance = 4f;
    public static PhysForceFlags physForceFlags = PhysForceFlags.None;
    public static float parryDuration = 0.4f;
    public BulletAttack bulletAttack;
    public float damage;
    public float duration;
    public Ray ray;
    public HashSet<GameObject> hitProjectiles = [];
    public override void OnEnter()
    {
        base.OnEnter();
        SetValues();
        if (NetworkServer.active) characterBody.AddTimedBuff(BishopEvents.Parry, parryDuration);
        (this as IBishopState).PlayFirstPersonCrossfade("LeftArm, Override", "SpearSwing", "leftArm.playbackRate", 1f, 0.05f, true);
        Util.PlaySound("Play_DoomTDA_Spear_Slash_LeftToRight", gameObject);
        if (!isAuthority) return;
        CreateBulletAttack();
    }
    public void SetValues()
    {
        ray = GetAimRay();
        damage = baseDamageCoefficient * characterBody.damage;
        duration = baseDuration;
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isAuthority) return;
        UpdateBulletAttack();
        Fire();
        if (fixedAge < duration) return;
        outer.SetNextStateToMain();
    }
    public void CreateBulletAttack()
    {
        bulletAttack = new BulletAttack
        {
            owner = gameObject,
            weapon = gameObject,
            origin = ray.origin,
            aimVector = ray.direction,
            minSpread = minSpread,
            maxSpread = maxSpread,
            damage = damage,
            allowTrajectoryAimAssist = allowTrajectoryAimAssist,
            bulletCount = bulletCount,
            damageType = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource(DamageSource.Secondary)),
            falloffModel = falloffModel,
            force = force,
            isCrit = RollCrit(),
            trajectoryAimAssistMultiplier = trajectoryAimAssistMultiplier,
            radius = radius,
            smartCollision = smartCollision,
            maxDistance = maxDistance,
            physForceFlags = physForceFlags,
            procCoefficient = procCoefficient,
            stopperMask = LayerIndex.ui.mask,
            hitEffectPrefab = BishopEvents.SpearSlashHitEffect.prefab,
            hitMask = LayerIndex.entityPrecise.mask | LayerIndex.projectile.mask,
            hitCallback = HitCallback
        };
        bulletAttack.AddModdedDamageType(BishopEvents.ParryDamageType);
        bulletAttack.AddModdedDamageType(CaeliImperiumAssets.CannotHitstun);
        bulletAttack.SetIgnoreHitTargets(true);
    }
    public bool HitCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
    {
        if (!HitProjectileCallback(ref hitInfo)) return false;
        bool hit = BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
        return hit;
    }
    public bool HitProjectileCallback(ref BulletAttack.BulletHit hitInfo)
    {
        GameObject colliderGameObject = hitInfo.collider.transform.root.gameObject;
        if (hitProjectiles.Contains(colliderGameObject)) return false;
        if (colliderGameObject.layer == LayerIndex.projectile.intVal)
        {
            hitProjectiles.Add(colliderGameObject);
            ProjectileDamage projectileDamage = colliderGameObject.GetComponent<ProjectileDamage>();
            if (projectileDamage)
            {
                if (projectileDamage.damageColorIndex == BishopEvents.ParriableDamageColor)
                {
                    ProjectileController projectileController = colliderGameObject.GetComponent<ProjectileController>();
                    if (projectileController)
                    {
                        GameObject projectilePrefab = ProjectileCatalog.GetProjectilePrefab(projectileController.catalogIndex);
                        if (projectilePrefab)
                        {
                            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                            {
                                projectilePrefab = projectilePrefab,
                                damage = projectileDamage.damage,
                                crit = bulletAttack.isCrit,
                                damageColorIndex = DamageColorIndex.Default,
                                force = projectileDamage.force,
                                damageTypeOverride = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource(DamageSource.Secondary)),
                                owner = gameObject,
                                position = projectileController.transform.position,
                                rotation = Util.QuaternionSafeLookRotation(ray.direction),
                                speedOverride = projectileController.rigidbody ? projectileController.rigidbody.velocity.magnitude : -1f,
                                target = projectileController.owner
                            };
                            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                        }
                    }
                    DamageInfo damageInfo = new DamageInfo
                    {
                        damage = projectileDamage.damage,
                        attacker = projectileController ? projectileController.owner : null,
                        canRejectForce = projectileDamage.force > 0f,
                        crit = projectileDamage.crit,
                        damageColorIndex = projectileDamage.damageColorIndex,
                        damageType = projectileDamage.damageType,
                        force = projectileDamage.transform.forward * projectileDamage.force,
                        inflictor = projectileDamage.gameObject,
                        position = projectileDamage.transform.position,
                        procCoefficient = 0f // TODO: Create a method to get proc coefficient from projectile
                    };
                    BishopEvents.OnParry(characterBody, damageInfo);
                    if (NetworkServer.active)
                    {
                        Destroy(projectileDamage.gameObject);
                    }
                    else
                    {
                        DestroyNetworkObjectNetMessage.SendToServer(projectileController.netId);
                    }
                }
                return false;
            }
        }
        return true;
    }
    public void UpdateBulletAttack()
    {
        if (bulletAttack == null) return;
        ray = GetAimRay();
        bulletAttack.origin = ray.origin;
        bulletAttack.aimVector = ray.direction;
    }
    public void Fire()
    {
        if (bulletAttack == null) return;
        bulletAttack.Fire();
    }
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
}
