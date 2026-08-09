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
using UnityEngine.UIElements;

namespace CaeliImperiumEntityStates.BomberWisp
{
    public class SpawnPillar : BaseSkillState
    {
        public static float damageCoefficient => BomberWispConfigs.SpawnPillarDamageCoefficient.Value;
        public static float procCoefficient => BomberWispConfigs.SpawnPillarProcCoefficient.Value;
        public static float force => BomberWispConfigs.SpawnPillarForce.Value;
        public static float baseDuration => BomberWispConfigs.SpawnPillarBaseDuration.Value;
        public static bool customAIAiming => BomberWispConfigs.SpawnPillarCustomAIAiming.Value;
        public static int minProjectilesForCustomAIAiming => BomberWispConfigs.SpawnPillarMinProjectilesForCustomAIAiming.Value;
        public static float maxRaycastDistance = 56f;
        public static int maxRepositionAttempts = 30;
        public static float distanceMultiplier = 1f;
        public float duration;
        private Animator animator;
        private int shakeLayerIndex = -1;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            animator = GetModelAnimator();
            if (animator)
            {
                animator.AddInteger("chargeCount");
                animator.AddInteger("shakeCount");
                shakeLayerIndex = animator.GetLayerIndex("Shake");
                if (shakeLayerIndex >= 0)
                {
                    animator.SetLayerWeight(shakeLayerIndex, 0f);
                }
            }
            if (!isAuthority) return;
            Vector3 shootPos = transform.position;
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
                List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
                int count = 0;
                List<Vector3> positions = [];
                foreach (ProjectileController instance in instancesList)
                {
                    if (instance.catalogIndex == BomberWisp2Events.BombProjectileIndex)
                    {
                        count++;
                        Vector3 position = instance.transform.position;
                        position.y = 0f;
                        positions.Add(position);
                    }
                }
                if (count >= minProjectilesForCustomAIAiming)
                {
                    CharacterMotor characterMotor = enemyObject.GetComponent<CharacterMotor>();
                    if (characterMotor)
                    {
                        shootPos = characterMotor.transform.position + characterMotor.GetPositionDelta();
                    }
                    else
                    {
                        shootPos = enemyObject.transform.position;
                    }
                    ProjectileExplosion projectileExplosion = BomberWisp2Events.BombProjectile.GetComponent<ProjectileExplosion>();
                    if (projectileExplosion)
                    {
                        float radius = projectileExplosion.blastRadius * distanceMultiplier * (count - 1);
                        shootPos = new Vector3(
                            UnityEngine.Random.Range(shootPos.x + radius, shootPos.x - radius),
                    shootPos.y,
                    UnityEngine.Random.Range(shootPos.z + radius, shootPos.z - radius)
                            );
                    }
                }
                else
                {
                    shootPos = enemyObject.transform.position;
                }
            }
            else
            {
                Ray ray = GetAimRay();
                if (Physics.Raycast(ray, out RaycastHit raycastHit, maxRaycastDistance, LayerIndex.world.mask))
                {
                    shootPos = raycastHit.point;
                }
                else
                {
                    shootPos = ray.origin + (ray.direction * maxRaycastDistance);
                }
            }
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = BomberWisp2Events.BombProjectile,
                crit = RollCrit(),
                damage = damageCoefficient * damageStat,
                damageTypeOverride = new DamageTypeCombo(DamageType.Generic, DamageTypeExtended.Generic, this.GetDamageSource()),
                force = force,
                owner = gameObject,
                position = shootPos,
                rotation = Quaternion.identity,
                speedOverride = 0
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (animator && shakeLayerIndex >= 0)
            {
                animator.SetLayerWeight(shakeLayerIndex, fixedAge / duration);
            }
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }
        public static Vector3 TryGetPosition(Vector3 position, List<Vector3> positions, float distance, int maxPositionAttempts, float addRadius)
        {
            if (IsValidPosition(position, positions, distance)) return position;
            float addRadius2 = addRadius;
            for (int i = 0; i < maxPositionAttempts; i++)
            {
                addRadius2 += addRadius;
                Vector3 newPosition = new Vector3(
                    UnityEngine.Random.Range(position.x + addRadius2, position.x - addRadius2),
                    position.y,
                    UnityEngine.Random.Range(position.z + addRadius2, position.z - addRadius2)
                );
                if (IsValidPosition(newPosition, positions, distance)) return newPosition;
            }
            return position;
        }
        public static bool IsValidPosition(Vector3 position, List<Vector3> positions, float distance)
        {
            float sqrDistance = distance * distance;
            foreach (Vector3 vector3 in positions)
            {
                float sqrMagnitude = (vector3 - position).sqrMagnitude;
                if (sqrMagnitude < sqrDistance) return false;
            }
            return true;
        }
        public override void OnExit()
        {
            base.OnExit();
            if (animator)
            {
                animator.SubstractInteger("chargeCount");
                animator.SubstractInteger("shakeCount");
            }
        }
    }
}
