using CaeliImperium;
using EntityStates;
using Rewired;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.Victor
{
    public class FireSyringes : BaseCaeliImperiumState
    {
        public static GameObject hitEffectPrefab;
        public static GameObject tracerEffectPrefab;
        public static float damageCoefficient = 0.5f;
        public static float procCoefficient = 1f;
        public static float fireRateCoefficient = 6f;
        public static DamageType damageType = DamageType.Generic;
        public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
        public static BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.DefaultBullet;
        public static float force = 100f;
        public static float maxDistance = 1024f;
        public static float maxSpread = 0f;
        public static float minSpread = 0f;
        public static PhysForceFlags physForceFlags;
        public static float radius = 0.5f;
        public static uint bulletCount = 1;
        public static float spreadBloom = .5f;
        public float damage;
        public float fireRate;
        public float stopwatch;

        public override void OnEnter()
        {
            base.OnEnter();
            Fire(1);
        }
        public override void UpdateValues()
        {
            base.UpdateValues();
            fireRate = fireRateCoefficient * characterBody.attackSpeed;
            damage = damageCoefficient * characterBody.damage;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float deltaTime = GetDeltaTime();
            stopwatch += deltaTime;
            float duration = 1f / fireRate;
            if (stopwatch >= duration)
            {
                uint count = 0;
                while (stopwatch >= duration)
                {
                    count++;
                    stopwatch -= duration;
                }
                Fire(count);
                UpdateValues();
                if (isAuthority && !IsKeyDownAuthority()) outer.SetNextStateToMain();
            }
        }
        public void Fire(uint count)
        {
            Util.PlaySound("Play_TF2_Medic_Syringe_Shot", gameObject);
            if (!isAuthority) return;
            Ray ray = GetAimRay();
            BulletAttack bulletAttack = new BulletAttack
            {
                bulletCount = count * bulletCount,
                aimVector = ray.direction,
                allowTrajectoryAimAssist = true,
                damage = damage,
                damageType = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource(DamageSource.Primary)),
                falloffModel = falloffModel,
                force = force,
                hitEffectPrefab = hitEffectPrefab,
                isCrit = RollCrit(),
                maxDistance = maxDistance,
                maxSpread = maxSpread,
                minSpread = minSpread,
                origin = ray.origin,
                owner = gameObject,
                physForceFlags = physForceFlags,
                procCoefficient = procCoefficient,
                radius = radius,
                tracerEffectPrefab = tracerEffectPrefab
            };
            bulletAttack.Fire();
            characterBody.AddSpreadBloom(spreadBloom);
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
    }
}
