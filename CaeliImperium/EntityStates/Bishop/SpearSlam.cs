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

namespace CaeliImperiumEntityStates.Bishop;
public class SpearSlam : BishopState
{
    public static float damageCoefficient = 10f;
    public static float procCoefficient = 1f;
    public static float force = 300f;
    public static float radius = 24f;
    public static AttackerFiltering attackerFiltering = AttackerFiltering.Default;
    public static BlastAttack.FalloffModel falloffModel = BlastAttack.FalloffModel.HalfLinear;
    public static BlastAttack.LoSType loSType = BlastAttack.LoSType.None;
    public static PhysForceFlags physForceFlags = PhysForceFlags.respectKnockbackImmuneFlag;
    public static DamageType damageType = DamageType.Stun1s;
    public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
    public static float baseDuration = 10f;
    public static float speedCoefficient = 3f;
    public static float extraGravityCoefficient = 0.5f;
    public static float antigravityBoost = 3f;
    public static float moveVectorSmoothTime = 0.05f;
    public float damage;
    public float duration;
    public float speed;
    public override void OnEnter()
    {
        base.OnEnter();
        (this as IBishopState).PlayFirstPersonCrossfade("LeftArm, Override", "SpearThrow", "leftArm.playbackRate", attackSpeedStat, 0.05f, true);
        Util.PlaySound("Play_DoomTDA_Spear_Slam_Enter", gameObject);
        if (NetworkServer.active && !characterBody.HasBuff(DLC3Content.Buffs.DrifterFallProtection))
        {
            characterBody.AddBuff(DLC3Content.Buffs.DrifterFallProtection);
        }
        if (!isAuthority) return;
        if (characterMotor)
        {
            characterMotor.onHitGroundAuthority += CharacterMotor_onHitGroundAuthority;
            characterMotor.walkSpeedPenaltyCoefficient = speedCoefficient; // I don't like this
            if (characterMotor.isGrounded && characterMotor.Motor) characterMotor.Motor.ForceUnground();
            characterMotor.velocity = new Vector3(characterMotor.velocity.x, Physics.gravity.y * -1f * antigravityBoost, characterMotor.velocity.z);
        }
        else
        {
            outer.SetNextStateToMain();
        }
    }
    public override void OnExit()
    {
        base.OnExit();
        if (!isAuthority) return;
        if (characterMotor)
        {
            characterMotor.onHitGroundAuthority -= CharacterMotor_onHitGroundAuthority;
            characterMotor.walkSpeedPenaltyCoefficient = 1f; // Why Hopoo didn't make this an event?
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        SetValuesFixedUpdate();
        if (!isAuthority) return;
        this.AddVelocity(Vector3.up * Physics.gravity.y * extraGravityCoefficient);
    }
    public void SetValues()
    {
        damage = damageCoefficient * characterBody.damage;
        duration = baseDuration;
        speed = speedCoefficient * characterBody.moveSpeed;
    }
    public void SetValuesFixedUpdate()
    {
        damage = damageCoefficient * characterBody.damage;
        speed = speedCoefficient * characterBody.moveSpeed;
    }
    private void CharacterMotor_onHitGroundAuthority(ref CharacterMotor.HitGroundInfo hitGroundInfo)
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
            position = hitGroundInfo.position,
            procCoefficient = procCoefficient,
            radius = radius,
            teamIndex = GetTeam()
        };
        blastAttack.Fire();
        EffectData effectData = new EffectData
        {
            origin = blastAttack.position,
            scale = blastAttack.radius,
            rotation = Quaternion.identity
        };
        EffectManager.SpawnEffect(BishopEvents.SpearSlamExplosionEffect.index, effectData, true);
    }
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
}
