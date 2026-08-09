using BrynzaAPI;
using CaeliImperium;
using CaeliImperium.Bodies;
using EntityStates;
using R2API;
using RoR2;
using RoR2.Navigation;
using System;
using UnityEngine;
using UnityEngine.Networking;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;

namespace CaeliImperiumEntityStates.Victor
{
    public class SlashBlade : BaseCaeliImperiumState
    {
        public static GameObject hitEffectPrefab;
        public static GameObject tracerEffectPrefab;
        public static float damageCoefficient = 2f;
        public static float procCoefficient = 1f;
        public static DamageType damageType = DamageType.Generic | DamageType.Stun1s | DamageType.BleedOnHit;
        public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
        public static BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.None;
        public static float force = 100f;
        public static float maxDistance = 3f;
        public static float maxSpread = 0f;
        public static float minSpread = 0f;
        public static PhysForceFlags physForceFlags;
        public static float radius = 3f;
        public static uint bulletCount = 1;
        public static float speedCoefficient = 9f;
        public static float baseDuration = 0.5f;
        public static AnimationCurve speedCurve = CaeliImperium.CaeliImperiumUtils.QuadraticOut(0f, 0f, 1f, 1f);
        public static float rechargeCooldown = 2f;
        public static float rechargeCooldownReductionPerHit = 2f;
        public BulletAttack bulletAttack;
        public Vector3 dashDirection;
        public float speed;
        public float duration;
        public float damage;
        public int originalLayer;
        public float rechargeReduction;
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active) characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            originalLayer = gameObject.layer;
            rechargeReduction = rechargeCooldown;
            gameObject.layer = LayerIndex.GetAppropriateFakeLayerForTeam(GetTeam()).intVal;
            if (characterMotor)
            {
                characterMotor.velocity = Vector3.zero;
                if (characterMotor.Motor)
                {
                    characterMotor.Motor.RebuildCollidableLayers();
                    characterMotor.Motor.ForceUnground();
                }
            }
            else if (rigidbody)
            {
                rigidbody.velocity = Vector3.zero;
            }
            if (!isAuthority) return;
            duration = baseDuration / attackSpeedStat;
            speed = speedCoefficient * attackSpeedStat * moveSpeedStat;
            Ray ray = GetAimRay();
            dashDirection = ray.direction;
            bulletAttack = new BulletAttack
            {
                bulletCount = bulletCount,
                aimVector = ray.origin,
                allowTrajectoryAimAssist = false,
                damage = damage,
                damageType = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource(DamageSource.Utility)),
                falloffModel = falloffModel,
                force = force,
                hitEffectPrefab = hitEffectPrefab,
                isCrit = RollCrit(),
                maxDistance = maxDistance,
                maxSpread = maxSpread,
                minSpread = minSpread,
                origin = ray.origin,
                owner = gameObject,
                weapon = gameObject,
                smartCollision = true,
                physForceFlags = physForceFlags,
                procCoefficient = procCoefficient,
                radius = radius,
                tracerEffectPrefab = tracerEffectPrefab,
                stopperMask = LayerIndex.ui.mask,
                hitMask = LayerIndex.entityPrecise.mask,
            };
            bulletAttack.hitCallback += HitCallBack;
            bulletAttack.AddModdedDamageType(VictorEvents.GainGutsDamageType);
            bulletAttack.SetIgnoreHitTargets(true);
            bulletAttack.Fire();
            Util.PlaySound("Play_Deadlock_Shiv_Dash", gameObject);
        }

        private bool HitCallBack(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            if (activatorSkillSlot)
            {
                activatorSkillSlot.rechargeStopwatch += rechargeReduction;
                rechargeReduction /= rechargeCooldownReductionPerHit;
            }
            return false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            UpdateValues();
            bulletAttack.Fire();
            float coof = speedCurve.Evaluate(fixedAge / duration);
            if (characterMotor)
            {
                characterMotor.rootMotion += dashDirection * speed * GetDeltaTime() * coof;
                characterMotor.velocity = Vector3.zero;
            }
            else if (rigidbody)
            {
                rigidbody.velocity = dashDirection * speed * coof;
            }
            if (fixedAge >= duration) outer.SetNextStateToMain();
        }
        public override void OnExit()
        {
            base.OnExit();
            if (NetworkServer.active) characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            gameObject.layer = originalLayer;
            if (characterMotor)
            {
                characterMotor.velocity = dashDirection * characterMotor.walkSpeed;
                if (characterMotor.Motor) characterMotor.Motor.RebuildCollidableLayers();
            }
            else if (rigidbody)
            {
                rigidbody.velocity = dashDirection * characterBody.moveSpeed;
            }
        }
        public override void UpdateValues()
        {
            base.UpdateValues();
            damage = damageCoefficient * characterBody.damage;
            if (bulletAttack != null)
            {
                bulletAttack.damage = damage;
                bulletAttack.origin = characterBody.aimOrigin;
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
