using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ItemBehaviours
{
    public class DamageAllEnemiesBehaviour : CharacterBody.ItemBehavior
    {
        public static float procCoefficient = 0f;
        public static float characterDamageMultiplier = 10f;
        public static float receivedDamageMultiplier = 10f;
        public static float outcomingDamageMultiplier = 1f;
        public float receivedDamage;
        public float outcomingDamage;
        public void DamageAll()
        {
            if (body == null) return;
            float damage = body.damage * characterDamageMultiplier + receivedDamage * receivedDamageMultiplier + outcomingDamage * outcomingDamageMultiplier;
            receivedDamage = 0f;
            outcomingDamage = 0f;
            foreach (CharacterBody characterBody in CharacterBody.readOnlyInstancesList)
            {
                HealthComponent healthComponent = characterBody.healthComponent;
                if (!healthComponent) continue;
                if (body.teamComponent)
                {
                    TeamComponent teamComponent = characterBody.teamComponent;
                    if (!teamComponent || teamComponent.teamIndex == TeamIndex.None || teamComponent.teamIndex == body.teamComponent.teamIndex) continue;
                }
                DamageInfo damageInfo = new DamageInfo
                {
                    attacker = gameObject,
                    crit = Util.CheckRoll(body.crit),
                    damage = damage,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageTypeCombo.Generic,
                    inflictor = gameObject,
                    position = characterBody.corePosition,
                    procCoefficient = procCoefficient
                };
                damageInfo.position = characterBody.corePosition;
                healthComponent.TakeDamageProcess(damageInfo);
            }
        }
    }
}
