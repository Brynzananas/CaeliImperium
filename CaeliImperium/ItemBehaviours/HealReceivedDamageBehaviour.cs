using CaeliImperium.Items;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ItemBehaviours
{
    public class HealReceivedDamageBehaviour : CharacterBody.ItemBehavior
    {
        public List<HealReceivedDamage> healReceivedDamageBits = [];
        public class HealReceivedDamage : IDisposable
        {
            public HealReceivedDamage(float healAmount, float healTime)
            {
                healRate = healAmount / healTime;
                this.healTime = healTime;
            }
            public float healRate;
            public float healTime;
            public void Dispose()
            {
            }
        }
        public void OnEnable()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }
        private void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            if (!body || obj.victimBody != body) return;
            float time = HealReceivedDamageEvents.HealReceivedDamageTime;
            for (int i = 0; i < stack; i++) time -= time * (HealReceivedDamageEvents.HealReceivedDamageStackTimeReduction / 100f);
            float coefficient = stack.Stack(HealReceivedDamageEvents.HealReceivedHealCoefficient, HealReceivedDamageEvents.HealReceivedHealCoefficientPerStack);
            AddHealReceivedDamageBit(obj.damageDealt * coefficient, time);
        }
        public void OnDisable()
        {
            GlobalEventManager.onServerDamageDealt -= GlobalEventManager_onServerDamageDealt;
        }
        public void AddHealReceivedDamageBit(float healAmount, float time)
        {
            HealReceivedDamage healReceivedDamageBit = new(healAmount, time);
            healReceivedDamageBits.Add(healReceivedDamageBit);
        }
        public void FixedUpdate()
        {
            if (!body) return;
            HealthComponent healthComponent = body.healthComponent;
            for (int i = 0; i < healReceivedDamageBits.Count; i++)
            {
                HealReceivedDamage healReceivedDamageBit = healReceivedDamageBits[i];
                if (healthComponent && healthComponent.health < healthComponent.body.maxHealth) healthComponent.Networkhealth += healReceivedDamageBit.healRate * Time.fixedDeltaTime;
                healReceivedDamageBit.healTime -= Time.fixedDeltaTime;
                if (healReceivedDamageBit.healTime <= 0)
                {
                    healReceivedDamageBits.Remove(healReceivedDamageBit);
                    healReceivedDamageBit.Dispose();
                }
            }
        }
    }
}
