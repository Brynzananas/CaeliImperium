using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components;

[RequireComponent(typeof(ProjectileController))]
public class DestroyProjectileOnOwnerDeath : MonoBehaviour
{
    public ProjectileController projectileController;
    private HealthComponent healthComponent;
    public void Awake()
    {
        if (!projectileController) projectileController = GetComponent<ProjectileController>();
    }
    public void FixedUpdate()
    {
        if (!NetworkServer.active) return;
        if (!projectileController)
        {
            Destroy(gameObject);
            return;
        }
        if (projectileController.owner)
        {
            if (!healthComponent || healthComponent.gameObject != projectileController.owner)
            {
                healthComponent = projectileController.owner.GetComponent<HealthComponent>();
            }
            if (!healthComponent || !healthComponent.alive)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
