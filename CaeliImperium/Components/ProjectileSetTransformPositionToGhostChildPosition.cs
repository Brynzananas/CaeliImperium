using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
public class ProjectileSetTransformPositionToGhostChildPosition : MonoBehaviour
{
    public ProjectileController projectileController;
    public string childName;
    public void Update()
    {
        if (!projectileController) return;
        if (!projectileController.ghost) return;
        ChildLocator childLocator = projectileController.ghost.GetComponent<ChildLocator>();
        if (!childLocator) return;
        Transform childTransform = childLocator.FindChild(childName);
        if (!childTransform) return;
        transform.position = childTransform.position;
    }
}
