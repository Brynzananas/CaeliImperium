using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.Commando.CommandoWeapon;

namespace CaeliImperiumEntityStates
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
            Util.PlaySound(FirePistol2.firePistolSoundString, base.gameObject);
            if (FirePistol2.muzzleEffectPrefab)
            {
                EffectData effectData = new EffectData
                {
                    origin = ray.origin,
                    rotation = Quaternion.LookRotation(ray.direction),
                };
                EffectManager.SpawnEffect(FirePistol2.muzzleEffectPrefab, effectData, true);
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
                    tracerEffectPrefab = FirePistol2.tracerEffectPrefab,
                    //muzzleName = targetMuzzle,
                    hitEffectPrefab = FirePistol2.hitEffectPrefab,
                    isCrit = RollCrit(),
                    radius = 0.1f,
                    smartCollision = true,
                    trajectoryAimAssistMultiplier = FirePistol2.trajectoryAimAssistMultiplier,
                    damageType = DamageTypeCombo.GenericPrimary
                }.Fire();
            }
        }
        /*public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(stack);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            stack = reader.ReadInt32();
        }*/
    }
}
