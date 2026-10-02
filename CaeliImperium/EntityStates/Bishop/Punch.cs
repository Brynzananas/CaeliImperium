using BrynzaAPI;
using CaeliImperium;
using CaeliImperium.Bodies;
using CaeliImperium.NetworkMessages;
using EntityStates;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates.Bishop;
public class Punch : BishopState
{
    public static float baseDamageCoefficient = 1f;
    public static float damageCoefficientIncreasePerCombo = 3f;
    public static float procCoefficient = 1f;
    public static float minSpread = 0f;
    public static float maxSpread = 0f;
    public static float baseDuration = 0.4f;
    public static bool allowTrajectoryAimAssist = false;
    public static uint bulletCount = 1;
    public static DamageType comboDamageType = DamageType.Stun1s;
    public static DamageType damageType = DamageType.Generic;
    public static DamageTypeExtended comboDamageTypeExtended = DamageTypeExtended.Generic;
    public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
    public static BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.None;
    public static float force = 300f;
    public static float radius = 2f;
    public static float trajectoryAimAssistMultiplier = 0f;
    public static bool smartCollision = true;
    public static float maxDistance = 2f;
    public static PhysForceFlags physForceFlags = PhysForceFlags.None;
    public static float seekDistance = 24f;
    public static float seekAngle = 180f;
    public static float backoffVelocity = 24f;
    public static float flyToTargetSpeed = 96f;
    public static float leftPunchTimerSet = 1f;
    public BulletAttack bulletAttack;
    public BullseyeSearch bullseyeSearch;
    public Ray ray;
    public float damage;
    public float duration;
    public bool stopMoving;
    public HurtBox target;
    public bool hitConfirmed;
    public override void OnEnter()
    {
        base.OnEnter();
        SetValues();
        if (!isAuthority) return;
        bool leftHandPunch = false;
        if ((this as IBishopState).bishopComponent)
        {
            if ((this as IBishopState).bishopComponent.leftHandPunchTimer > 0f)
            {
                (this as IBishopState).bishopComponent.leftHandPunchTimer = 0f;
                leftHandPunch = true;
            }
            else
            {
                (this as IBishopState).bishopComponent.leftHandPunchTimer = leftPunchTimerSet;
            }
        }
        (this as IBishopState).PlayFirstPersonCrossfade(leftHandPunch ? "LeftArm, Override" : "RightArm, Override", "PunchStart", leftHandPunch ? "leftArm.playbackRate" : "rightArm.playbackRate", attackSpeedStat, 0.05f, true);
        (this as IBishopState).PlayFirstPersonCrossfade(!leftHandPunch ? "LeftArm, Override" : "RightArm, Override", "Idle", !leftHandPunch ? "leftArm.playbackRate" : "rightArm.playbackRate", attackSpeedStat, 0.05f, true);
        FindTarget();
        CreateBulletAttack();
    }
    /*public override void OnExit()
    {
        base.OnExit();
        if (isAuthority) return;
        Vector3 backoff = ray.direction * -1f * backoffVelocity;
        this.SetVelocity(backoff);
    }*/
    public void SetValues()
    {
        damage = baseDamageCoefficient * characterBody.damage + (characterBody.damage * damageCoefficientIncreasePerCombo * characterBody.GetBuffCount(BishopEvents.MeleeCombo));
        duration = baseDuration / characterBody.attackSpeed;
        ray = GetAimRay();
    }
    public void FindTarget()
    {
        bullseyeSearch = new BullseyeSearch
        {
            teamMaskFilter = TeamMask.all,
            filterByLoS = true,
            searchOrigin = ray.origin,
            searchDirection = ray.direction,
            sortMode = BullseyeSearch.SortMode.Angle,
            maxDistanceFilter = seekDistance,
            maxAngleFilter = seekAngle,
        };
        bullseyeSearch.teamMaskFilter.RemoveTeam(GetTeam());
        bullseyeSearch.RefreshCandidates();
        bullseyeSearch.FilterOutGameObject(gameObject);
        target = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isAuthority) return;
        UpdateBulletAttack();
        Fire();
        if (target && !stopMoving)
        {
            Vector3 velocity = (target.transform.position - transform.position).normalized * flyToTargetSpeed;
            this.SetVelocity(Vector3.zero);
            this.AddRootMotion(velocity * Time.fixedDeltaTime);
        }
        if (fixedAge < duration) return;
        outer.SetNextStateToMain();
    }
    public void CreateBulletAttack()
    {
        bool isCombo = activatorSkillSlot ? activatorSkillSlot.stock > 0 : false;
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
            damageType = new DamageTypeCombo(isCombo ? comboDamageType : damageType, isCombo ? comboDamageTypeExtended : damageTypeExtended, this.GetDamageSource(DamageSource.Primary)),
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
    public bool FilterCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
    {
        if (hitConfirmed || (target && target.healthComponent && hitInfo.hitHurtBox && hitInfo.hitHurtBox.healthComponent && hitInfo.hitHurtBox.healthComponent != target.healthComponent)) return false;
        return BulletAttack.DefaultFilterCallbackImplementation(bulletAttack, ref hitInfo);
    }
    public bool HitCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
    {
        if (hitConfirmed) return false;
        HurtBox hurtBox = hitInfo.hitHurtBox;
        if (!hurtBox) return BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
        HealthComponent healthComponent1 = hurtBox.healthComponent;
        if (!healthComponent1 || !healthComponent1.body) return BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
        bool isStaggered = healthComponent1.body.HasBuff(BishopEvents.Stagger);
        bool hit = BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
        if (target && healthComponent1)
        {
            if (healthComponent1.body.teamComponent && TeamManager.IsTeamEnemy(healthComponent1.body.teamComponent.teamIndex, GetTeam()))
            {
                stopMoving = true;
                hitConfirmed = true;
                Vector3 backoff = ray.direction * -1f * backoffVelocity;
                Util.PlaySound("Play_DoomTDA_Punch", gameObject);
                this.SetVelocity(backoff);
                if (!isStaggered)
                {
                    if (activatorSkillSlot && activatorSkillSlot.stock > 0)
                    {
                        AddTimeBuffAndResetTimerForAllStacksNetMessage.Send(characterBody, BishopEvents.MeleeCombo.buffIndex, BishopEvents.MeleeComboDuration);
                        activatorSkillSlot.DeductStock(1);
                    }
                }

            }
        }
        return hit;
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
