using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
public class ProjectileSetTransformPositionToOwnerModelChildPosition : MonoBehaviour
{
    public ProjectileController projectileController;
    public string childName;
    public void Update()
    {
        if (!projectileController) return;
        if (!projectileController.owner) return;
        CharacterBody characterBody = projectileController.owner.GetComponent<CharacterBody>();
        if (!characterBody) return;
        ChildLocator childLocator = characterBody.GetModelChildLocator();
        if (!childLocator) return;
        Transform childTransform = childLocator.FindChild(childName);
        if (!childTransform) return;
        transform.position = childTransform.position;
    }
}
