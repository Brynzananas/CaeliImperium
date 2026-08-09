using CaeliImperium.Bodies;
using CaeliImperium.ScriptableObjects;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaeliImperium.Components
{
    public class VictorCrosshair : MonoBehaviour
    {
        public static float smoothTime = 0.2f;
        public CrosshairController crosshairController;
        public TextMeshProUGUI chargeCounter;
        public Image utilityMeter;
        private float currentCharge;
        private float chargeVelocity;
        public TextMeshProUGUI reviveChargeCounter;
        public void LateUpdate()
        {
            SkillLocator skillLocator = crosshairController?.hudElement?.targetCharacterBody?.skillLocator;
            if (!skillLocator) return;
            float charge = 0f;
            float recharge = 0f;
            float rechargeInterval = 0f;
            float liveSecondsRemaining = 0f;
            foreach (GenericSkill genericSkill in skillLocator.allSkills)
            {
                if (!genericSkill) continue;
                VictorSkillDef.InstanceData instanceData = genericSkill.skillInstanceData != null ? genericSkill.skillInstanceData as VictorSkillDef.InstanceData : null;
                if (instanceData != null)
                {
                    if (recharge < genericSkill.rechargeStopwatch)
                    {
                        recharge = genericSkill.rechargeStopwatch;
                        rechargeInterval = genericSkill.finalRechargeInterval;
                    }
                    charge += instanceData.charge;
                }
                if (genericSkill.skillDef && genericSkill.skillDef is PassiveItemSkillDef passiveItemSkillDef && passiveItemSkillDef.passiveItem == VictorEvents.ImmortalPassive)
                {
                    if (liveSecondsRemaining < genericSkill.cooldownRemaining) liveSecondsRemaining = genericSkill.cooldownRemaining;
                }
            }
            currentCharge = Mathf.SmoothDamp(currentCharge, charge, ref chargeVelocity, smoothTime, float.MaxValue, Time.deltaTime);
            chargeCounter?.text = (Mathf.Min(currentCharge / 10f, 100f)).ToString("000.0");
            utilityMeter?.fillAmount = recharge / rechargeInterval;
            reviveChargeCounter?.text = liveSecondsRemaining.ToString("0.0");
        }
    }
}
