using CaeliImperium.Bodies;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components.Doomguy;
[RequireComponent(typeof(ProjectileDamage), typeof(ProjectileController))]
public class DoomguySpearProjectileComponent : MonoBehaviour, IProjectileImpactBehavior
{
    public GameObject headshotExplosionPrefab;
    public float damageCoefficient = 20f;
    public float procCoefficient = 1f;
    public float minSpread = 0f;
    public float maxSpread = 0f;
    public bool allowTrajectoryAimAssist = false;
    public uint bulletCount = 1;
    public float radius = 1f;
    public float trajectoryAimAssistMultiplier = 1f;
    public bool smartCollision = true;
    public float maxDistance = 1024f;
    public BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.None;
    public PhysForceFlags physForceFlags = PhysForceFlags.None;
    private Vector3 startPosition;
    private ProjectileDamage projectileDamage;
    private ProjectileController projectileController;
    private bool hit;
    public void Awake()
    {
        projectileDamage = GetComponent<ProjectileDamage>();
        projectileController = GetComponent<ProjectileController>();
    }
    public void Start()
    {
        startPosition = transform.position;
    }

    public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
    {
        if (!NetworkServer.active || hit) return;
        hit = true;
        Vector3 vector3 = transform.position - startPosition;
        BulletAttack bulletAttack = new BulletAttack
        {
            owner = projectileController.owner,
            weapon = projectileController.owner,
            origin = startPosition,
            aimVector = vector3.normalized,
            minSpread = minSpread,
            maxSpread = maxSpread,
            damage = projectileDamage.damage,
            allowTrajectoryAimAssist = allowTrajectoryAimAssist,
            bulletCount = bulletCount,
            damageType = projectileDamage.damageType,
            falloffModel = falloffModel,
            force = projectileDamage.force,
            isCrit = projectileDamage.crit,
            trajectoryAimAssistMultiplier = trajectoryAimAssistMultiplier,
            radius = radius,
            smartCollision = smartCollision,
            maxDistance = maxDistance,
            physForceFlags = physForceFlags,
            procCoefficient = procCoefficient,
            hitMask = LayerIndex.entityPrecise.mask,
            stopperMask = LayerIndex.ui.mask,
            modifyOutgoingDamageCallback = ModifyOutgoingDamageCallback,
            sniper = true
        };
        bulletAttack.Fire();
        Destroy(gameObject);
    }

    private void ModifyOutgoingDamageCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo, DamageInfo damageInfo)
    {
        EffectData effectData = new EffectData
        {
            origin = hitInfo.point,
            scale = 1f
        };
        EffectIndex effectIndex = DoomguyEvents.SpearThrowHitEffect.index;
        if (hitInfo.hitHurtBox && hitInfo.isSniperHit)
        {
            damageInfo.crit = true;
            effectIndex = DoomguyEvents.SpearThrowHeadshotExplosionEffect.index;
            damageInfo.AddModdedDamageType(DoomguyEvents.SuperStunDamageType);
        }
        else
        {
            damageInfo.crit = bulletAttack.isCrit;
        }
        EffectManager.SpawnEffect(effectIndex, effectData, true);
    }
}
