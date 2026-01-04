using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium
{
    public class FireSkullGun : BaseState
    {
        public int stack;
        public override void OnEnter()
        {
            base.OnEnter();
            Fire();
            if (isAuthority) outer.SetNextStateToMain();
        }
        public void Fire()
        {
            Ray ray = GetAimRay();
            Util.PlaySound(EntityStates.Commando.CommandoWeapon.FirePistol2.firePistolSoundString, base.gameObject);
            if (EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab)
            {
                EffectData effectData = new EffectData
                {
                    origin = ray.origin,
                    rotation = Quaternion.LookRotation(ray.direction),
                };
                EffectManager.SpawnEffect(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, effectData, true);
            }

            if (isAuthority)
            {
                new BulletAttack
                {
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = ray.origin,
                    aimVector = ray.direction,
                    minSpread = 0f,
                    maxSpread = 0f,
                    damage = damageStat * stack,
                    force = 0f,
                    tracerEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab,
                    //muzzleName = targetMuzzle,
                    hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                    isCrit = RollCrit(),
                    radius = 0.1f,
                    smartCollision = true,
                    trajectoryAimAssistMultiplier = EntityStates.Commando.CommandoWeapon.FirePistol2.trajectoryAimAssistMultiplier,
                    damageType = DamageTypeCombo.GenericPrimary
                }.Fire();
            }
        }
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(stack);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            stack = reader.ReadInt32();
        }
    }
    public class FireAtomicBeam : BaseState
    {
        public static float maxDamageMultiplier = 50f;
        public static float dotDuration = 5f;
        public int stack;
        public float charge;
        public void Fire()
        {
            Ray ray = GetAimRay();
            if (isAuthority)
            {
                new BulletAttack
                {
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = ray.origin,
                    aimVector = ray.direction,
                    minSpread = 0f,
                    maxSpread = 0f,
                    damage = characterBody.damage * Utils.ConvertAmplificationPercentageIntoReductionPercentage(charge, maxDamageMultiplier),
                    force = 0f,
                    tracerEffectPrefab = Assets.fireSnipeSuperTracer,
                    //muzzleName = targetMuzzle,
                    hitEffectPrefab = null,
                    isCrit = RollCrit(),
                    radius = 2f,
                    smartCollision = true,
                    trajectoryAimAssistMultiplier = EntityStates.Commando.CommandoWeapon.FirePistol2.trajectoryAimAssistMultiplier,
                    damageType = DamageTypeCombo.GenericSpecial,
                    hitCallback = IrradiateOnHit,

                }.Fire();
                bool IrradiateOnHit(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
                {
                    if (hitInfo.hitHurtBox)
                    {
                        InflictDotInfo dotInfo = new InflictDotInfo()
                        {
                            attackerObject = gameObject,
                            victimObject = hitInfo.hitHurtBox.healthComponent.gameObject,
                            totalDamage = characterBody.damage,
                            damageMultiplier = stack,
                            duration = dotDuration,
                            dotIndex = Buffs.IrradiatedDotIndex,

                        };
                        DotController.InflictDot(ref dotInfo);
                    }
                    return BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
                }
            }
        }
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(stack);
            writer.Write(charge);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            stack = reader.ReadInt32();
            charge = reader.ReadSingle();
        }
    }
}
