using BrynzaAPI;
using CaeliImperium;
using CaeliImperium.Bodies;
using EntityStates;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates.Victor
{
    public class BloodTribute : BaseCaeliImperiumState
    {
        public static float damageCoefficient = 0.2f;
        public static float minHealthPercentageDamage = 1f;
        public static float maxHealthPercentageDamage = 5f;
        public static float timeToFullyCharge = 15f;
        public static float procCoefficient = 1f;
        public static float fireRateCoefficient = 3f;
        public static float force = 0f;
        public static float timeBeforeCanCancel = 0.3f;
        public static DamageType damageType = DamageType.Generic;
        public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
        public static BlastAttack.FalloffModel falloffModel = BlastAttack.FalloffModel.None;
        public static PhysForceFlags physForceFlags;
        public static AttackerFiltering attackerFiltering = AttackerFiltering.AlwaysHit;
        public static float radius = 9f;
        public float damage;
        public float fireRate;
        public float stopwatch;
        public float charge;
        public VictorBlastAttack victorBlastAttack;
        private uint soundId;
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_Deadlock_Victor_Aura_Start", gameObject);
            Util.PlaySound("Play_Deadlock_Victor_Aura_Loop", gameObject);
            soundId = Util.PlaySound("Play_Deadlock_Victor_Aura_Ramup", gameObject);
            GlobalEventManager.onClientDamageNotified += GlobalEventManager_onClientDamageNotified;
            if (!NetworkServer.active) return;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }

        private void GlobalEventManager_onClientDamageNotified(DamageDealtMessage obj)
        {
            if (!obj.attacker || obj.attacker != gameObject || !obj.HasModdedDamageType(VictorEvents.SummonDeathAuraDamageType)) return;
            CaeliImperiumPlugin.Log.LogMessage("Working");
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            if (!obj.attacker || obj.attacker != gameObject || !(obj.damageInfo is VictorDamageInfo victorDamageInfo)) return;
            CaeliImperiumPlugin.Log.LogMessage(obj.victim.name + " got hit with victor value: " + victorDamageInfo.reduceReviveCooldown);
        }

        public override void OnExit()
        {
            base.OnExit();
            Util.PlaySound("Play_Deadlock_Victor_Aura_End", gameObject);
            Util.PlaySound("Stop_Deadlock_Victor_Aura_Loop", gameObject);
            Util.PlaySound("Stop_Deadlock_Victor_Aura_Ramup", gameObject);
            GlobalEventManager.onClientDamageNotified -= GlobalEventManager_onClientDamageNotified;
            if (!NetworkServer.active) return;
            GlobalEventManager.onServerDamageDealt -= GlobalEventManager_onServerDamageDealt;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float deltaTime = Time.fixedDeltaTime;
            stopwatch += deltaTime;
            float duration = 1f / fireRate;
            if (stopwatch >= duration)
            {
                int count = 0;
                while (stopwatch >= duration)
                {
                    count++;
                    stopwatch -= duration;
                }
                UpdateValues();
                Fire(count);
            }
            if (fixedAge > timeBeforeCanCancel && isAuthority && (skillLocator.GetSprintSkill() && skillLocator.GetSprintSkill() == activatorSkillSlot ? inputBank.sprint.justPressed : IsKeyJustPressedAuthority())) outer.SetNextStateToMain();
        }
        public override void UpdateValues()
        {
            base.UpdateValues();
            fireRate = fireRateCoefficient * characterBody.attackSpeed;
            damage = damageCoefficient * characterBody.damage;
            float coof = fixedAge / timeToFullyCharge;
            if (healthComponent)
            {
                damage += (Mathf.Lerp(minHealthPercentageDamage, maxHealthPercentageDamage, coof) / 100f * healthComponent.fullCombinedHealth);
            }
            if (victorBlastAttack == null)
            {
                victorBlastAttack = new VictorBlastAttack
                {
                    attacker = gameObject,
                    attackerFiltering = attackerFiltering,
                    baseDamage = damage,
                    baseForce = force,
                    crit = RollCrit(),
                    damageType = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource()),
                    falloffModel = falloffModel,
                    inflictor = gameObject,
                    losType = BlastAttack.LoSType.None,
                    physForceFlags = physForceFlags,
                    radius = radius,
                    position = transform.position,
                    procCoefficient = procCoefficient,
                    teamIndex = GetTeam(),
                    reduceReviveCooldown = 10f
                };
                victorBlastAttack.AddModdedDamageType(VictorEvents.SummonDeathAuraDamageType);
            }
            else
            {
                victorBlastAttack.baseDamage = damage;
                victorBlastAttack.baseForce = force;
                victorBlastAttack.crit = RollCrit();
                victorBlastAttack.radius = radius;
                victorBlastAttack.position = transform.position;
                victorBlastAttack.teamIndex = GetTeam();
                victorBlastAttack.reduceReviveCooldown = 10f;
            }
            AkSoundEngine.SetRTPCValueByPlayingID("Volume_SFX", coof * 100f, soundId);
        }
        public void Fire(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!isAuthority) continue;
                victorBlastAttack.Fire();
                EffectData effectData = new EffectData
                {
                    origin = victorBlastAttack.position,
                    scale = victorBlastAttack.radius
                };
                EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.igniteOnKillExplosionEffectPrefab, effectData, true);
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
