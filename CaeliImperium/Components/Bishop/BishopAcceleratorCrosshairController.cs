using CaeliImperiumScriptableObjects;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CaeliImperiumComponents.Bishop;
public class BishopAcceleratorCrosshairController : MonoBehaviour
{
    public HudElement hudElement;
    public Image fillBar;
    public float maxCharge = 100f;
    public Color unchargedColor = Color.white;
    public Color superChargedColor = Color.red;
    public void Awake()
    {
        if (!hudElement) hudElement = GetComponent<HudElement>();
    }
    public void Update()
    {
        if (!hudElement || !fillBar) return;
        CharacterBody characterBody = hudElement.targetCharacterBody;
        if (!characterBody) return;
        SkillLocator skillLocator = characterBody.skillLocator;
        if (!skillLocator) return;
        float totalCharge = BishopAcceleratorSkillDef.GetTotalCharge(skillLocator);
        fillBar.fillAmount = totalCharge / 100f;
        fillBar.color = totalCharge >= 100f ? superChargedColor : unchargedColor;
    }
}
