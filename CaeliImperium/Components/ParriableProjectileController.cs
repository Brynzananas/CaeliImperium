using CaeliImperium.Components.Doomguy;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
public class ParriableProjectileController : MonoBehaviour, IProjectileImpactBehavior
{
    public ProjectileController projectileController;
    public ProjectileDamage projectileDamage;
    private bool playedAudio;
    public void Start()
    {
        if (!projectileController || !projectileDamage) return;
        ProjectileGhostController projectileGhostController = projectileController.ghost;
        if (!projectileGhostController) return;
        ParriableProjectileGhostColorChanger parriableProjectileGhostColorChanger = projectileGhostController.GetComponent<ParriableProjectileGhostColorChanger>();
        if (!parriableProjectileGhostColorChanger) return;
        if (projectileDamage.damageColorIndex == CaeliImperium.Bodies.DoomguyEvents.ParriableDamageColor)
        {
            parriableProjectileGhostColorChanger.ApplyParriableMaterial();
            RoR2.Util.PlaySound("Play_DoomTDA_ParryableProjectile_Loop", gameObject);
            playedAudio = true;
        }
        else if (CaeliImperiumUtils.IsDoom())
        {
            parriableProjectileGhostColorChanger.ApplyNonParriableMaterial();
        }
        else
        {
            parriableProjectileGhostColorChanger.ApplyDefaultMaterial();
        }
    }
    public void OnDestroy()
    {
        StopSound();
    }
    public void StopSound()
    {
        if (playedAudio)
        {
            RoR2.Util.PlaySound("Stop_DoomTDA_ParryableProjectile_Loop", gameObject);
            playedAudio = false;
        }
        
    }
    public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
    {
        StopSound();
    }
}
