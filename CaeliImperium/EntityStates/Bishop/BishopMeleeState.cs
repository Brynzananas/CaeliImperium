using BrynzaAPI;
using CaeliImperium;
using CaeliImperium.Bodies;
using CaeliImperium.NetworkMessages;
using CaeliImperiumEntityStates.Bishop;
using EntityStates;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.Bishop;
public abstract class BishopMeleeState : BishopState
{
    public abstract float damageCoefficient { get; }
    public abstract float procCoefficient { get; }
    public abstract float minSpread { get; }
    public abstract float maxSpread { get; }
    public abstract float baseDuration { get; }
    public abstract bool allowTrajectoryAimAssist { get; }
    public abstract uint bulletCount { get; }
    public abstract DamageType damageType { get; }
    public abstract DamageTypeExtended damageTypeExtended { get; }
    public abstract BulletAttack.FalloffModel falloffModel { get; }
    public abstract float force { get; }
    public abstract float radius { get; }
    public abstract float trajectoryAimAssistMultiplier { get; }
    public abstract bool smartCollision { get; }
    public abstract float maxDistance { get; }
    public abstract PhysForceFlags physForceFlags { get; }
    public abstract float backoffVelocity { get; }
    public abstract float minFlyToTargetSpeed { get; }
    public abstract float findTargetDistance { get; }
    public abstract float findTargetRadius { get; }
    public abstract bool giveEmpowermentOnExecute { get; }
    public BulletAttack bulletAttack;
    public Ray ray;
    public float damage;
    public float duration;
    public bool stopMoving;
    public HurtBox target;
    public bool hitConfirmed;
    public Vector3 moveDirection;
    public bool hasRequiredStockAndDelay;
    public bool targetIsStaggered;
    public bool wasStaggeredOnHit;
    public float addDuration;
    public float flyToTargetSpeed;
    public override void OnEnter()
    {
        base.OnEnter();
        SetValues();
        if (!isAuthority) return;
        FindTarget();
        CreateBulletAttack();
    }
    public virtual void SetValues()
    {
        damage = damageCoefficient * characterBody.damage;
        duration = baseDuration / characterBody.attackSpeed;
        ray = GetAimRay();
        hasRequiredStockAndDelay = activatorSkillSlot ? activatorSkillSlot.HasRequiredStockAndDelay() : false;
        flyToTargetSpeed = minFlyToTargetSpeed;
    }
    public virtual void FindTarget()
    {
        Collider[] colliders = Physics.OverlapCapsule(ray.origin, ray.origin + (ray.direction * findTargetDistance), findTargetRadius, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);
        if (colliders != null && colliders.Length > 0)
        {
            List<Collider> validColliders = [];
            List<Collider> validAndStaggeredColliders = [];
            bool atleastOneStaggered = false;
            foreach (Collider collider in colliders)
            {
                HurtBox hurtBox = collider.GetComponent<HurtBox>();
                if (!hurtBox) continue;
                HealthComponent targetHealthComponent = hurtBox.healthComponent;
                if (!targetHealthComponent || healthComponent == targetHealthComponent) continue;
                CharacterBody targetCharacterBody = targetHealthComponent.body;
                if (!targetCharacterBody) continue;
                bool isEnemy = targetCharacterBody.teamComponent ? TeamManager.IsTeamEnemy(targetCharacterBody.teamComponent.teamIndex, GetTeam()) : true;
                if (!isEnemy) continue;
                validColliders.Add(collider);
                if (!targetCharacterBody.HasBuff(BishopEvents.Stagger)) continue;
                validAndStaggeredColliders.Add(collider);
                if (!atleastOneStaggered) atleastOneStaggered = true;
            }
            Collider nearestCollider = CaeliImperiumUtils.GetClosestColliderOnLine(atleastOneStaggered ? validAndStaggeredColliders : validColliders, ray.direction, ray.origin);
            if (nearestCollider)
            {
                target = nearestCollider.GetComponent<HurtBox>();
                targetIsStaggered = atleastOneStaggered;
            }
        }
        if (target)
        {
            Vector3 vector3 = target.transform.position - transform.position;
            moveDirection = vector3.normalized;
            flyToTargetSpeed = Mathf.Max(flyToTargetSpeed, vector3.magnitude / duration);
        }
        else
        {
            moveDirection = Vector3.zero;
        }
    }
    public virtual bool canFly => targetIsStaggered || hasRequiredStockAndDelay;
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isAuthority) return;
        AuthorityFixedUpdate();
    }
    public virtual void AuthorityFixedUpdate()
    {
        UpdateBulletAttack();
        FireBulletAttack();
        FlyToTarget();
        if (fixedAge < (duration + addDuration)) return;
        outer.SetNextStateToMain();
    }
    public virtual void FlyToTarget()
    {
        if (target && moveDirection != Vector3.zero && !stopMoving && canFly)
        {
            this.SetVelocity(Vector3.zero);
            this.AddRootMotion(moveDirection * flyToTargetSpeed * Time.fixedDeltaTime);
        }
    }
    public virtual void CreateBulletAttack()
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
            damageType = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource(DamageSource.Primary)),
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
            hitMask = LayerIndex.entityPrecise.mask,
            hitCallback = HitCallback,
            filterCallback = FilterCallback
        };
        bulletAttack.SetIgnoreHitTargets(true);
        bulletAttack.AddModdedDamageType(BishopEvents.BypassStaggerInvincibilityDamageType);
    }
    public virtual bool FilterCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
    {
        if (hitConfirmed || (target && target.healthComponent && hitInfo.hitHurtBox && hitInfo.hitHurtBox.healthComponent && hitInfo.hitHurtBox.healthComponent != target.healthComponent)) return false;
        return BulletAttack.DefaultFilterCallbackImplementation(bulletAttack, ref hitInfo);
    }
    public virtual bool HitCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
    {
        if (hitConfirmed) return false;
        HurtBox hurtBox = hitInfo.hitHurtBox;
        if (!hurtBox) return BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
        HealthComponent healthComponent1 = hurtBox.healthComponent;
        if (!healthComponent1 || !healthComponent1.body) return BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
        PreMeleeHit(bulletAttack, ref hitInfo, healthComponent1);
        bool hit = BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
        if (hit && target && healthComponent1)
        {
            if (healthComponent1.body.teamComponent && TeamManager.IsTeamEnemy(healthComponent1.body.teamComponent.teamIndex, GetTeam())) OnMeleeHit(bulletAttack, ref hitInfo, healthComponent1);
        }
        return hit;
    }
    public virtual void PreMeleeHit(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo, HealthComponent victimHealthComponent)
    {
        wasStaggeredOnHit = victimHealthComponent.body.HasBuff(BishopEvents.Stagger);
    }
    public virtual void OnMeleeHit(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo, HealthComponent victimHealthComponent)
    {
        stopMoving = true;
        hitConfirmed = true;
        Vector3 backoff = moveDirection * -1f * backoffVelocity;
        this.SetVelocity(backoff);
        if (wasStaggeredOnHit)
        {
            if (giveEmpowermentOnExecute) (this as IBishopState).RechargeSpecialSpearStocks();
        }
        else if (hasRequiredStockAndDelay)
        {
            activatorSkillSlot.DeductStock(1);
        }
    }
    public virtual void UpdateBulletAttack()
    {
        if (bulletAttack == null) return;
        ray = GetAimRay();
        moveDirection = target ? (target.transform.position - transform.position).normalized : Vector3.zero;
        bulletAttack.origin = ray.origin;
        bulletAttack.aimVector = moveDirection == Vector3.zero ? ray.direction : moveDirection;
    }
    public virtual void FireBulletAttack()
    {
        if (bulletAttack == null) return;
        bulletAttack.Fire();
    }
    public override InterruptPriority GetMinimumInterruptPriority() => fixedAge > duration ? InterruptPriority.Skill : InterruptPriority.PrioritySkill;
}
