using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components.Bishop;
[RequireComponent(typeof(ProjectileExplosion))]
public class BishopSpearProjectileComponent : MonoBehaviour
{
    public GameObject headshotExplosionPrefab;
    public float headshotExplosionDamageCoefficient = 5f;
    public float headshotExplosionProcCoefficient = 1f;
    public float headshotExplosionForce = 300f;
    public float headshotExplosionRadius = 24f;
    public AttackerFiltering headshotExplosionAttackerFiltering = AttackerFiltering.Default;
    public BlastAttack.FalloffModel headshotExplosionFalloffModel = BlastAttack.FalloffModel.None;
    public BlastAttack.LoSType headshotExplosionLoSType = BlastAttack.LoSType.None;
    public PhysForceFlags headshotExplosionPhysForceFlags = PhysForceFlags.respectKnockbackImmuneFlag;
    public DamageTypeCombo damageTypeCombo = new DamageTypeCombo(DamageType.Generic, DamageTypeExtended.Generic, DamageSource.NoneSpecified);
    public ProjectileExplosion projectileExplosion;
    private bool eventAdded;
    public void Awake()
    {
        if (!projectileExplosion) projectileExplosion = GetComponent<ProjectileExplosion>();
    }
    public void OnEnable()
    {
        if (!projectileExplosion) return;
        eventAdded = true;
        projectileExplosion.OnProjectileExplosion += OnProjectileExplosion;
    }
    public void OnDisable()
    {
        if (!projectileExplosion || !eventAdded) return;
        projectileExplosion.OnProjectileExplosion -= OnProjectileExplosion;
        eventAdded = false;
    }
    private void OnProjectileExplosion(BlastAttack attack, BlastAttack.Result result)
    {
        bool foundSniperHurtbox = false;
        foreach(BlastAttack.HitPoint hitPoint in result.hitPoints)
        {
            HurtBox hurtBox = hitPoint.hurtBox;
            if (!hurtBox) continue;
            if (hurtBox.healthComponent ? !FriendlyFireManager.ShouldSplashHitProceed(hurtBox.healthComponent, attack.teamIndex) : false) continue;
            if (hurtBox.isSniperTarget)
            {
                foundSniperHurtbox = true;
                break;
            }
        }
        if (!foundSniperHurtbox) return;
        CharacterBody attackerBody = attack.attacker.GetComponent<CharacterBody>();
        BlastAttack blastAttack = new BlastAttack
        {
            attacker = attack.attacker,
            attackerFiltering = headshotExplosionAttackerFiltering,
            baseDamage = headshotExplosionDamageCoefficient * (attackerBody ? attackerBody.damage : 1f),
            baseForce = headshotExplosionForce,
            crit = attack.crit,
            falloffModel = headshotExplosionFalloffModel,
            inflictor = attack.inflictor,
            losType = headshotExplosionLoSType,
            physForceFlags = headshotExplosionPhysForceFlags,
            damageColorIndex = DamageColorIndex.Item,
            damageType = damageTypeCombo,
            position = attack.position,
            procCoefficient = headshotExplosionProcCoefficient,
            radius = headshotExplosionRadius,
            teamIndex = attack.teamIndex
        };
        blastAttack.Fire();
        if (!headshotExplosionPrefab) return;
        EffectData effectData = new EffectData
        {
            origin = blastAttack.position,
            scale = blastAttack.radius,
            rotation = Quaternion.identity
        };
        EffectManager.SpawnEffect(headshotExplosionPrefab, effectData, true);
    }
}
