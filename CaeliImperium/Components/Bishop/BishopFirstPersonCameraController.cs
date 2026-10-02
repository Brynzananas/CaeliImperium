using CaeliImperium.ScriptableObjects;
using RoR2;
using RoR2.HudOverlay;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components.Bishop;
public class BishopFirstPersonCameraController : MonoBehaviour
{
    public FirstPersonCameraController firstPersonCameraController;
    public Transform rightWeaponHolder;
    public Vector3 cameraOffset;
    private BishopWeaponController currentRightHandWeapon;
    private BishopWeaponController previousRightHandWeaponPrefab;
    private CharacterBody currentCharacterBody;
    private OverlayController overlayController;
    public void Awake()
    {
        if (!firstPersonCameraController) firstPersonCameraController = GetComponent<FirstPersonCameraController>();
    }
    public void FixedUpdate()
    {
        currentCharacterBody = firstPersonCameraController && firstPersonCameraController.cameraRigController ? firstPersonCameraController.cameraRigController.targetBody : null;
        if (!currentCharacterBody) return;
        if (currentCharacterBody.skillLocator)
        {
            if (rightWeaponHolder)
            {
                GenericSkill[] genericSkills = currentCharacterBody.skillLocator.AllSkills;
                if (genericSkills != null)
                {
                    BishopWeaponController bishopWeaponController = null;
                    foreach (GenericSkill skill in genericSkills)
                    {
                        if (!skill || !skill.skillDef || skill.skillDef is not BishopSkillDef bishopSkillDef || !bishopSkillDef.rightHandWeapon) continue;
                        bishopWeaponController = bishopSkillDef.rightHandWeapon;
                    }
                    if (bishopWeaponController && bishopWeaponController != previousRightHandWeaponPrefab)
                    {
                        SetNewWeapon(bishopWeaponController);
                    }
                }
            }
        }
    }
    public void OnDestroy()
    {
        if (overlayController != null)
        {
            HudOverlayManager.RemoveOverlay(overlayController);
            overlayController = null;
        }
    }
    public void SetNewWeapon(BishopWeaponController newWeaponPrefab)
    {
        if (currentRightHandWeapon) Destroy(currentRightHandWeapon);
        if (overlayController != null)
        {
            HudOverlayManager.RemoveOverlay(overlayController);
            overlayController = null;
        }
        if (!newWeaponPrefab) return;
        currentRightHandWeapon = Instantiate(newWeaponPrefab, rightWeaponHolder);
        previousRightHandWeaponPrefab = newWeaponPrefab;
        if (newWeaponPrefab.crosshairPrefab)
        {
            OverlayCreationParams overlayCreationParams = new OverlayCreationParams
            {
                prefab = newWeaponPrefab.crosshairPrefab,
                childLocatorEntry = "CrosshairExtras"
            };
            overlayController = HudOverlayManager.AddOverlay(currentCharacterBody.gameObject, overlayCreationParams);
        }
        ChildLocator childLocator = currentCharacterBody.GetModelChildLocator();
        if (!childLocator) return;
        BishopWeaponController bishopWeaponController = currentRightHandWeapon.GetComponent<BishopWeaponController>();
        if (bishopWeaponController)
        {
            childLocator.ReplaceChild("Muzzle", bishopWeaponController.muzzle);
        }
        else
        {
            Transform baseMuzzle = childLocator.FindChild("BaseMuzzle");
            childLocator.ReplaceChild("Muzzle", baseMuzzle);
        }
    }
}
