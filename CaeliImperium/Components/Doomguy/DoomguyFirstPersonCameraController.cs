using CaeliImperium.Components.FirstPerson;
using CaeliImperium.ScriptableObjects;
using RoR2;
using RoR2.HudOverlay;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components.Doomguy;
public class DoomguyFirstPersonCameraController : MonoBehaviour
{
    public static DoomguyFirstPersonCameraController instance;
    public FirstPersonCameraController firstPersonCameraController;
    public Transform rightWeaponHolder;
    public Vector3 cameraOffset;
    private DoomguyWeaponController currentRightHandWeapon;
    private DoomguyWeaponController previousRightHandWeaponPrefab;
    private CharacterBody currentCharacterBody;
    private OverlayController overlayController;
    public void Awake()
    {
        instance = this;
        if (!firstPersonCameraController) firstPersonCameraController = GetComponent<FirstPersonCameraController>();
    }
    public void Shoot()
    {
        if (!currentRightHandWeapon) return;
        currentRightHandWeapon.StartMuzzleParticles();
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
                    DoomguyWeaponController bishopWeaponController = null;
                    foreach (GenericSkill skill in genericSkills)
                    {
                        if (!skill || !skill.skillDef || skill.skillDef is not DoomguySkillDef bishopSkillDef || !bishopSkillDef.rightHandWeapon) continue;
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
    public void SetNewWeapon(DoomguyWeaponController newWeaponPrefab)
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
        if (firstPersonCameraController && firstPersonCameraController.animator)
        {
            firstPersonCameraController.animator.SetInteger("rightWeaponIndex", newWeaponPrefab.rightWeaponIndex);
            firstPersonCameraController.animator.Play(newWeaponPrefab.loopAnimationString, 1);
        }
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
        DoomguyWeaponController bishopWeaponController = currentRightHandWeapon.GetComponent<DoomguyWeaponController>();
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
