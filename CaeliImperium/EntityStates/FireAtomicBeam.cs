using CaeliImperium;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates
{
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
                    tracerEffectPrefab = CaeliImperium.CaeliImperiumAssets.fireSnipeSuperTracer,
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
                            dotIndex = CaeliImperiumContent.Buffs.IrradiatedDotIndex,

                        };
                        DotController.InflictDot(ref dotInfo);
                    }
                    return BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
                }
            }
        }
        /*public override void OnSerialize(NetworkWriter writer)
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
        }*/
    }
}
