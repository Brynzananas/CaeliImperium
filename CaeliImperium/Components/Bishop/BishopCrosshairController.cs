using CaeliImperium.ScriptableObjects;
using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaeliImperium.Components.Bishop;
public class BishopCrosshairController : MonoBehaviour
{
    public CrosshairController crosshairController;
    public HudElement hudElement;
    public GameObject sprintSkillHolder;
    public TextMeshProUGUI sprintSkillStocksCounter;
    public Image sprintSkillCooldownMeter;
    public GameObject specialSkillHolder;
    public Image specialSkillStocksMeter;
    public int specialSkillStocksMeterMaxStocks;
    public Transform weaponCrosshairHolder;
    private GameObject currentWeaponCrosshair;
    private GameObject previousWeaponCrosshairPrefab;
    private OverlayController overlayController;

    public void Awake()
    {
        if (!crosshairController) crosshairController = GetComponent<CrosshairController>();
        if (!hudElement) hudElement = GetComponent<HudElement>();
    }
    public void Update()
    {
        if (!hudElement) return;
        GameObject targetBodyObject = hudElement.targetBodyObject;
        if (!targetBodyObject) return;
        SkillLocator skillLocator = targetBodyObject.GetComponent<SkillLocator>();
        BishopComponent bishopComponent = targetBodyObject.GetComponent<BishopComponent>();
        if (bishopComponent && sprintSkillHolder)
        {
            GenericSkill dashSkill = bishopComponent.dashSkill;
            if (dashSkill)
            {
                if (sprintSkillStocksCounter) sprintSkillStocksCounter.text = bishopComponent.dashSkill.stock.ToString();
                if (sprintSkillCooldownMeter)
                {
                    float fill = dashSkill.stock >= dashSkill.maxStock ? 1f : dashSkill.rechargeStopwatch / dashSkill.finalRechargeInterval;
                    sprintSkillCooldownMeter.fillAmount = fill;
                }
                if (!sprintSkillHolder.activeSelf) sprintSkillHolder.SetActive(true);
            }
            else
            {
                if (sprintSkillHolder.activeSelf) sprintSkillHolder.SetActive(false);
            }

        }
        if (skillLocator)
        {
            /*if (weaponCrosshairHolder)
            {
                GenericSkill[] genericSkills = skillLocator.AllSkills;
                if (genericSkills != null)
                {
                    GameObject crosshairPrefab = null;
                    foreach (GenericSkill skill in genericSkills)
                    {
                        if (!skill || !skill.skillDef || skill.skillDef is not BishopSkillDef bishopSkillDef || !bishopSkillDef.rightHandWeapon || !bishopSkillDef.rightHandWeapon.crosshairPrefab) continue;
                        crosshairPrefab = bishopSkillDef.rightHandWeapon.crosshairPrefab;
                    }
                    if (crosshairPrefab && crosshairPrefab != previousWeaponCrosshairPrefab)
                    {
                        if (currentWeaponCrosshair) Destroy(currentWeaponCrosshair);
                        currentWeaponCrosshair = Instantiate(crosshairPrefab, weaponCrosshairHolder);
                        previousWeaponCrosshairPrefab = crosshairPrefab;
                    }
                }
            }*/
            GenericSkill specialSkill = skillLocator.special;
            if (specialSkillHolder)
            {
                if (specialSkill && specialSkill.skillDef)
                {
                    if (specialSkillStocksMeter)
                    {
                        int maxStocks = specialSkillStocksMeterMaxStocks == 0 ? specialSkill.maxStock : specialSkillStocksMeterMaxStocks * specialSkill.skillDef.requiredStock;
                        float fill = (float)specialSkill.stock / (float)maxStocks;
                        specialSkillStocksMeter.fillAmount = fill;
                    }
                    if (!specialSkillHolder.activeSelf) specialSkillHolder.SetActive(true);
                }
                else
                {
                    if (specialSkillHolder.activeSelf) specialSkillHolder.SetActive(false);
                }
            }
        }
    }
}
