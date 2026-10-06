using CaeliImperium;
using CaeliImperium.Bodies;
using EntityStates;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace CaeliImperiumEntityStates.Bishop;
public class SpearSlam : BishopState
{
    public static float damageCoefficient = 7f;
    public static float procCoefficient = 1f;
    public static float force = 300f;
    public static float radius = 24f;
    public static AttackerFiltering attackerFiltering = AttackerFiltering.Default;
    public static BlastAttack.FalloffModel falloffModel = BlastAttack.FalloffModel.HalfLinear;
    public static BlastAttack.LoSType loSType = BlastAttack.LoSType.None;
    public static PhysForceFlags physForceFlags = PhysForceFlags.respectKnockbackImmuneFlag;
    public static DamageType damageType = DamageType.Stun1s;
    public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
    public static float moveVectorSmoothTime = 0.05f;
    public float damage;
    public static float speedCoefficient = 0.5f;
    public static float baseMaxDuration = 2f;
    public static float baseMinDuration = 0.5f;
    public static float addHeight = 24f;
    public static float jumpSpeedCoefficient = 3f;
    public static float maxDistance = 64f;
    public static float castSpeed = 24f;
    public static float castGravityMultiplier = 1f;
    public float minDuration;
    public float speed;
    public float jumpSpeed;
    public float targetY;
    public GameObject indicator;
    public Vector3 endPosition;
    public Vector3 previousEndPosition;
    public CapsuleCollider capsuleCollider;
    public SphereCollider sphereCollider;
    public bool noCollider;
    public LayerMask layerMask;
    public override void OnEnter()
    {
        base.OnEnter();
        (this as IBishopState).PlayFirstPersonCrossfade("LeftArm, Override", "SpearThrow", "leftArm.playbackRate", attackSpeedStat, 0.05f, true);
        Util.PlaySound("Play_DoomTDA_Spear_Slam_Enter", gameObject);
        SetValues();
        if (NetworkServer.active && !characterBody.HasBuff(DLC3Content.Buffs.DrifterFallProtection))
        {
            characterBody.AddBuff(DLC3Content.Buffs.DrifterFallProtection);
        }
        if (characterMotor)
        {
            capsuleCollider = characterMotor.capsuleCollider;
            layerMask = characterMotor.Motor.CollidableLayers;
        }
        else
        {
            layerMask = LayerIndex.world.mask; // I should get layer mask of gameobject but I am lazy to figure it out yet
        }
        if (!capsuleCollider) capsuleCollider = GetComponent<CapsuleCollider>();
        if (!capsuleCollider) sphereCollider = GetComponent<SphereCollider>();
        if (!sphereCollider) noCollider = true;
        if (!isAuthority) return;
        CalculateEndPosition();
        previousEndPosition = endPosition;
        if (characterMotor)
        {
            characterMotor.walkSpeedPenaltyCoefficient = speedCoefficient; // I don't like this
            if (characterMotor.isGrounded && characterMotor.Motor) characterMotor.Motor.ForceUnground();
        }
        if (!characterBody.isPlayerControlled) return;
        indicator = GameObject.Instantiate(CaeliImperiumAssets.HuntressArrowRainIndicator);
        indicator.transform.localScale = radius.ToVector3();
        indicator.transform.position = endPosition;
    }
    public override void OnExit()
    {
        base.OnExit();
        if (indicator) Destroy(indicator);
        if (!isAuthority) return;
        if (characterMotor)
        {
            characterMotor.walkSpeedPenaltyCoefficient = 1f; // Why Hopoo didn't make this an event?
        }
    }
    public void Fire()
    {
        outer.SetNextStateToMain();
        BlastAttack blastAttack = new BlastAttack
        {
            attacker = gameObject,
            attackerFiltering = attackerFiltering,
            baseDamage = damage,
            baseForce = force,
            crit = RollCrit(),
            falloffModel = falloffModel,
            inflictor = gameObject,
            losType = loSType,
            physForceFlags = physForceFlags,
            damageColorIndex = DamageColorIndex.Default,
            damageType = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource(DamageSource.Special)),
            position = endPosition,
            procCoefficient = procCoefficient,
            radius = radius,
            teamIndex = GetTeam()
        };
        blastAttack.Fire();
        EffectData effectData = new EffectData
        {
            origin = blastAttack.position,
            scale = blastAttack.radius
        };
        EffectManager.SpawnEffect(BishopEvents.SpearSlamExplosionEffect.index, effectData, true);
        if (characterMotor)
        {
            characterMotor.Motor.SetPosition(endPosition);
        }
        else if (rigidbody)
        {
            rigidbody.position = endPosition;
        }
        else
        {
            transform.position = endPosition;
        }

    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        SetValuesFixedUpdate();
        StartAimMode();
        if (!isAuthority) return;
        CalculateEndPosition();
        Vector3 velocity = this.GetVelocity();
        velocity.y = 0f;
        this.SetVelocity(velocity);
        float deltaY = targetY - transform.position.y;
        this.AddRootMotion(new Vector3(0f, deltaY * jumpSpeed, 0f) * GetDeltaTime());
        if (fixedAge > baseMaxDuration || (inputBank && fixedAge > minDuration && !inputBank.skill3.down))
        {
            Fire();
        }
    }
    public override void Update()
    {
        base.Update();
        if (!indicator) return;
        float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        Vector3 interpolatedPosition = Vector3.Lerp(previousEndPosition, endPosition, interpolationFactor);
        previousEndPosition = endPosition;
        indicator.transform.position = interpolatedPosition;
    }
    public void CalculateEndPosition()
    {
        Ray ray = GetAimRay();
        Vector3 gravityVector = characterMotor ? new Vector3(0f, Physics.gravity.y, 0f) * castGravityMultiplier : Physics.gravity * castGravityMultiplier;
        Vector3 velocityVector = ray.direction * castSpeed;
        if (capsuleCollider)
        {
            Vector3 vector3 = transform.up * (capsuleCollider.height / 2f);
            Vector3 point1 = capsuleCollider.bounds.center + vector3;
            Vector3 point2 = capsuleCollider.bounds.center - vector3;
            if (Physics.CapsuleCast(point1, point2, capsuleCollider.radius, ray.direction, out RaycastHit hitInfo, maxDistance, layerMask))
            {
                float num = Mathf.Max(capsuleCollider.height, capsuleCollider.radius);
                endPosition = hitInfo.point + (hitInfo.normal * num);
            }
            else
            {
                endPosition = capsuleCollider.bounds.center + (ray.direction * maxDistance);
                point1 = endPosition + vector3;
                point2 = endPosition - vector3;
                if (CaeliImperiumUtils.ParabolicCapsuleCast(point1, point2, capsuleCollider.radius, velocityVector, out RaycastHit hitInfo1, layerMask, gravityVector))
                {
                    float num = Mathf.Max(capsuleCollider.height, capsuleCollider.radius);
                    endPosition = hitInfo1.point + (hitInfo1.normal * num);
                }
            }
        }
        else if (sphereCollider)
        {
            if (Physics.SphereCast(sphereCollider.bounds.center, sphereCollider.radius, ray.direction, out RaycastHit hitInfo, maxDistance, layerMask))
            {
                endPosition = hitInfo.point + (hitInfo.normal * sphereCollider.radius);
            }
            else
            {
                endPosition = sphereCollider.bounds.center + (ray.direction * maxDistance);
                if (CaeliImperiumUtils.ParabolicSphereCast(endPosition, sphereCollider.radius, velocityVector, out RaycastHit hitInfo1, layerMask, gravityVector))
                {
                    endPosition = hitInfo1.point + (hitInfo1.normal * sphereCollider.radius);
                }
            }
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance, layerMask))
            {
                endPosition = hitInfo.point;
            }
            else
            {
                endPosition = ray.origin + (ray.direction * maxDistance);
                if (CaeliImperiumUtils.ParabolicRayCast(ray.origin, velocityVector, out RaycastHit hitInfo1, layerMask, gravityVector))
                {
                    endPosition = hitInfo.point;
                }
            }
        }
    }
    public void SetValues()
    {
        minDuration = baseMinDuration / characterBody.attackSpeed;
        speed = speedCoefficient * characterBody.moveSpeed;
        targetY = transform.position.y + addHeight;
        jumpSpeed = jumpSpeedCoefficient * characterBody.attackSpeed;
        damage = damageCoefficient * characterBody.damage;
    }
    public void SetValuesFixedUpdate()
    {
        speed = speedCoefficient * characterBody.moveSpeed;
        damage = damageCoefficient * characterBody.damage;
    }
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
}
