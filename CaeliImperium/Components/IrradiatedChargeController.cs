using CaeliImperium.Items;
using RoR2;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static CaeliImperium.Items.InflictIrradiatedOnHitEvents;

namespace CaeliImperium.Components
{
    public class IrradiatedChargeController : MonoBehaviour
    {
        private static FixedConditionalWeakTable<CharacterBody, IrradiatedChargeController> keyValuePairs = [];
        private CharacterBody characterBody;
        private float totalDamage;
        private int previousBuffCount;
        public static IrradiatedChargeController FindIrradiatedChargeController(CharacterBody characterBody)
        {
            if (!keyValuePairs.TryGetValue(characterBody, out IrradiatedChargeController irradiatedChargeController))
            {
                irradiatedChargeController = characterBody.GetOrAddComponent<IrradiatedChargeController>();
                irradiatedChargeController.characterBody = characterBody;
                keyValuePairs.Add(characterBody, irradiatedChargeController);
            }
            return irradiatedChargeController;
        }
        public void AddDamage(DamageReport damageReport)
        {
            if (!characterBody) return;
            CharacterBody attackerBody = damageReport.attackerBody;
            if (!attackerBody) return;
            totalDamage += damageReport.damageDealt / attackerBody.baseDamage;
            int stacks = 0;
            while (totalDamage >= totalDamageThreshold)
            {
                stacks++;
                totalDamage -= totalDamageThreshold;
            }
            int buffCount = (int)Mathf.Floor(totalDamage / totalDamageThreshold * 100f);
            previousBuffCount = buffCount;
            characterBody.SetBuffCount(Charge.buffIndex, buffCount);
            if (stacks <= 0) return;
            InflictIrradiated(characterBody, attackerBody, dotDamageCoefficient, dotDuration, damageReport.damageInfo.inflictedHurtbox, stacks);
        }
        public void OnDestroy()
        {
            if (!characterBody) return;
            if (keyValuePairs.ContainsKey(characterBody))
            {
                keyValuePairs.Remove(characterBody);
            }
        }
    }
}
