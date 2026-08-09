using BrynzaAPI;
using CaeliImperium;
using CaeliImperium.Bodies;
using CaeliImperium.Configs;
using EntityStates;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.XR;

namespace CaeliImperiumEntityStates.BomberWisp
{
    public class FireBomb : BaseSkillState
    {
        public static float damageCoefficient => BomberWispConfigs.FireBombDamageCoefficient.Value;
        public static float procCoefficient => BomberWispConfigs.FireBombProcCoefficient.Value;
        public static float force => BomberWispConfigs.FireBombForce.Value;
        public static float baseDuration => BomberWispConfigs.FireBombBaseDuration.Value;
        public static float projectileSpeed => BomberWispConfigs.FireBombProjectileSpeed.Value;
        public static float timeToTarget => BomberWispConfigs.FireBombTimeToTarget.Value;
        public static bool customAIAiming => BomberWispConfigs.FireBombCustomAIAiming.Value;
        public static int minProjectilesForCustomAIAiming => BomberWispConfigs.FireBombMinProjectilesForCustomAIAiming.Value;
        public static float minSpread = 25f;
        public static float maxSpread = 50f;
        public static float baseSpreadCoefficient = 0.2f;
        public static float maxProjectileDistance = 36f;
        public static float projectileDistanceMaxCoefficient = 1f;
        public static float projectileDistanceMinCoefficient = 0f;
        public static float maxTargetDistance = 24f;
        public static float targetDistanceCoefficient = 0.2f;
        public static float predictionMinCoefficient = 0f;
        public static float predictionMaxCoefficient = 0.2f;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Util.PlayAttackSpeedSound(EntityStates.Wisp1Monster.FireEmbers.attackString, base.gameObject, this.attackSpeedStat);
            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay);
            base.PlayAnimation("Fire", "Fire", "FireAttack1.playbackRate", this.duration);
            if (!isAuthority) return;
            Vector3 shootDir;
            float magnitude;
            GameObject enemyObject = null;
            if (customAIAiming && !characterBody.isPlayerControlled && characterBody.master)
            {
                BaseAI[] baseAIs = characterBody.master.aiComponents;
                if (baseAIs != null && baseAIs.Length != 0)
                {
                    BaseAI baseAI = baseAIs[0];
                    if (baseAI && baseAI.currentEnemy != null && baseAI.currentEnemy.gameObject)
                    {
                        enemyObject = baseAI.currentEnemy.gameObject;
                    }
                }
            }
            if (enemyObject)
            {
                Vector3 vector31 = enemyObject.transform.position;
                Vector3 extraVector = Vector3.zero;
                List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
                int count = 0;
                float spread = baseSpreadCoefficient;
                float projectileDistance = maxProjectileDistance * maxProjectileDistance;
                foreach (ProjectileController instance in instancesList)
                {
                    if (instance.catalogIndex == BomberWisp2Events.BombProjectileIndex)
                    {
                        count++;
                        Vector3 vector3 = vector31 - instance.transform.position;
                        if (vector3.sqrMagnitude < projectileDistance)
                        {
                            projectileDistance = vector3.sqrMagnitude;
                        }
                    }
                }
                if (count >= minProjectilesForCustomAIAiming)
                {
                    CharacterMotor characterMotor = enemyObject.GetComponent<CharacterMotor>();
                    if (characterMotor != null)
                    {
                        ProjectileImpactExplosion projectileImpactExplosion = BomberWisp2Events.BombProjectile.GetComponent<ProjectileImpactExplosion>();
                        if (projectileImpactExplosion != null)
                        {
                            extraVector = characterMotor.GetPositionDelta() * (timeToTarget + projectileImpactExplosion.lifetimeAfterImpact + projectileImpactExplosion.blastRadius / 2f);
                            extraVector *= Mathf.Lerp(predictionMinCoefficient, predictionMaxCoefficient, UnityEngine.Random.Range(0f, 1f));
                        }
                    }
                }
                shootDir = (aimRay.direction * Trajectory.CalculateGroundSpeed(timeToTarget, (vector31 - aimRay.origin).magnitude)) + (Physics.gravity.normalized * Trajectory.CalculateInitialYSpeedForFlightDuration(timeToTarget, Physics.gravity.magnitude));
                shootDir += extraVector;
                magnitude = shootDir.magnitude;
                shootDir.Normalize();
                float distance = (vector31 - aimRay.origin).magnitude * targetDistanceCoefficient;
                spread *= Mathf.Lerp(projectileDistanceMaxCoefficient, projectileDistanceMinCoefficient, Mathf.Sqrt(projectileDistance / (maxProjectileDistance * maxProjectileDistance)));
                shootDir = Util.ApplySpread(shootDir, minSpread * spread / distance, maxSpread * spread / distance, 1f, 1f);
            }
            else if (Util.CharacterRaycast(gameObject, aimRay, out RaycastHit raycastHit, 1000f, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
            {
                shootDir = (aimRay.direction * Trajectory.CalculateGroundSpeed(timeToTarget, raycastHit.distance)) + (Physics.gravity.normalized * Trajectory.CalculateInitialYSpeedForFlightDuration(timeToTarget, Physics.gravity.magnitude));
                magnitude = shootDir.magnitude;
                shootDir.Normalize();
            }
            else
            {
                shootDir = aimRay.direction;
                magnitude = projectileSpeed;
            }
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = BomberWisp2Events.BombProjectile,
                crit = RollCrit(),
                damage = damageCoefficient * damageStat,
                damageTypeOverride = new DamageTypeCombo(DamageType.Generic, DamageTypeExtended.Generic, this.GetDamageSource()),
                force = force,
                owner = gameObject,
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(shootDir),
                speedOverride = magnitude
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }
    }
}
