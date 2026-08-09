using CaeliImperium;
using CaeliImperium.Bodies;
using EntityStates;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.Victor
{
    public class FireSpear : BaseCaeliImperiumState
    {
        public static GameObject hitEffectPrefab;
        public static GameObject tracerEffectPrefab;
        public static float damageCoefficient = 9f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.3f;
        public static DamageType damageType = DamageType.Generic;
        public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
        public static BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.None;
        public static float force = 100f;
        public static float maxDistance = 1024f;
        public static float maxSpread = 0f;
        public static float minSpread = 0f;
        public static PhysForceFlags physForceFlags;
        public static float radius = 0.5f;
        public static uint bulletCount = 1;
        public static float spreadBloom = 1.5f;
        public float damage;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            Fire(1);
        }
        public override void UpdateValues()
        {
            base.UpdateValues();
            duration = baseDuration / attackSpeedStat;
            damage = damageCoefficient * damageStat;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority || fixedAge < duration) return;
            outer.SetNextState(new AimDownSpear());
        }
        public void Fire(uint count)
        {
            Util.PlaySound("Play_Deadlock_Venator_Crossbow_Shoot", gameObject);
            if (!isAuthority) return;
            Ray ray = GetAimRay();
            BulletAttack bulletAttack = new BulletAttack
            {
                bulletCount = count * bulletCount,
                aimVector = ray.direction,
                allowTrajectoryAimAssist = true,
                damage = damage,
                damageType = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource(DamageSource.Secondary)),
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
                tracerEffectPrefab = tracerEffectPrefab,
                sniper = true
            };
            bulletAttack.hitCallback += OnHit;
            bulletAttack.Fire();
            characterBody.AddSpreadBloom(spreadBloom);
        }
        private bool OnHit(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            if (hitInfo.isSniperHit)
            {
                Util.PlaySound("Play_Deadlock_Venator_Crossbow_Hit_Headshot", gameObject);
            }
            else
            {
                Util.PlaySound("Play_Deadlock_Venator_Crossbow_Hit", gameObject);
            }
            return false;
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
