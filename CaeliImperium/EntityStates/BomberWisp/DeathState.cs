using CaeliImperium;
using CaeliImperium.Bodies;
using CaeliImperium.Configs;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates.BomberWisp
{
    public class DeathState : BaseSkillState
    {
        public static float minExplosionDamageCoefficient => BomberWispConfigs.DeathStateMinExplosionDamageCoefficient.Value;
        public static float maxExplosionDamageCoefficient => BomberWispConfigs.DeathStateMaxExplosionDamageCoefficient.Value;
        public static float projectileDamageCoefficient = 2f;
        public static float procCoefficient => BomberWispConfigs.DeathStateProcCoefficient.Value;
        public static float minExplosionForce => BomberWispConfigs.DeathStateMinExplosionForce.Value;
        public static float maxExplosionForce => BomberWispConfigs.DeathStateMaxExplosionForce.Value;
        public static float projectileForce = 3000f;
        public static float minRadius => BomberWispConfigs.DeathStateMinRadius.Value;
        public static float maxRadius => BomberWispConfigs.DeathStateMaxRadius.Value;
        public static float projectileVelocity = 15f;
        public static BlastAttack.FalloffModel falloffModel = BlastAttack.FalloffModel.Linear;
        public override void OnEnter()
        {
            base.OnEnter();
            if (base.modelLocator)
            {
                if (base.modelLocator.modelBaseTransform)
                {
                    EntityState.Destroy(base.modelLocator.modelBaseTransform.gameObject);
                }
                if (base.modelLocator.modelTransform)
                {
                    EntityState.Destroy(base.modelLocator.modelTransform.gameObject);
                }
            }
            if (!EffectManager.ShouldUsePooledEffect(DeathState.initialExplosion))
            {
                global::UnityEngine.Object.Instantiate<GameObject>(DeathState.initialExplosion, base.transform.position, base.transform.rotation);
            }
            else
            {
                EffectManager.SpawnEffect(DeathState.initialExplosion, new EffectData
                {
                    origin = base.transform.position,
                    rotation = base.transform.rotation
                }, false);
            }
            if (NetworkServer.active)
            {
                EntityState.Destroy(base.gameObject);
            }
            if (!isAuthority) return;
            GameObject owner = healthComponent.lastHitAttacker ?? gameObject;
            int count = 0;
            float charge = 0f;
            if (skillLocator && skillLocator.allSkills != null)
                foreach (GenericSkill genericSkill in skillLocator.allSkills)
                {
                    if (!genericSkill) continue;
                    EntityStateMachine entityStateMachine = genericSkill.stateMachine;
                    if (!entityStateMachine || entityStateMachine.state == null) continue;
                    if (entityStateMachine.state is ChargeBomb chargeBomb)
                    {
                        count++;
                        charge += chargeBomb.fixedAge / chargeBomb.duration;
                    }
                    if (entityStateMachine.state is SpawnPillar spawnPillar)
                    {
                        count++;
                        charge += spawnPillar.fixedAge / spawnPillar.duration;
                    }
                }
            if (count > 0)
            {
                BlastAttack blastAttack = new BlastAttack
                {
                    attacker = owner,
                    attackerFiltering = AttackerFiltering.Default,
                    baseDamage = damageStat * Mathf.LerpUnclamped(minExplosionDamageCoefficient, maxExplosionDamageCoefficient, charge),
                    baseForce = Mathf.LerpUnclamped(minExplosionForce, maxExplosionForce, charge),
                    crit = characterBody.RollCrit(),
                    damageColorIndex = DamageColorIndex.Default,
                    falloffModel = falloffModel,
                    inflictor = characterBody.gameObject,
                    position = transform.position,
                    damageType = new DamageTypeCombo(DamageType.Stun1s, DamageTypeExtended.Generic, this.GetDamageSource()),
                    procCoefficient = procCoefficient,
                    radius = Mathf.LerpUnclamped(minRadius, maxRadius, charge),
                    teamIndex = TeamComponent.GetObjectTeam(owner)
                };
                blastAttack.Fire();
                EffectData effectData = new EffectData
                {
                    origin = blastAttack.position,
                    scale = blastAttack.radius
                };
                CaeliImperiumPlugin.Log.LogMessage("Spawning the effect without Scale");
                EffectManager.SpawnEffect(BomberWisp2Events.Explosion.index, effectData, true);
            }
            else
            {
                return;
                Vector3 velocity;
                if (characterMotor)
                {
                    velocity = characterMotor.velocity.normalized;
                }
                else if (rigidbody)
                {
                    velocity = rigidbody.velocity.normalized;
                }
                else
                {
                    velocity = transform.forward * -1f;
                }
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = BomberWisp2Events.BombProjectile,
                    crit = RollCrit(),
                    damage = projectileDamageCoefficient * damageStat,
                    damageTypeOverride = new DamageTypeCombo(DamageType.Stun1s, DamageTypeExtended.Generic, this.GetDamageSource()),
                    force = projectileForce,
                    owner = owner,
                    position = transform.position,
                    rotation = Util.QuaternionSafeLookRotation(velocity),
                    speedOverride = projectileVelocity
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
        public static GameObject initialExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Wisp/WispDeath.prefab").WaitForCompletion();
    }
}
