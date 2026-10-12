using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
public class ParriableProjectileController : MonoBehaviour
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
        if (projectileDamage.damageColorIndex == CaeliImperium.Bodies.BishopEvents.ParriableDamageColor)
        {
            parriableProjectileGhostColorChanger.ApplyParriableMaterial();
            RoR2.Util.PlaySound("Play_DoomTDA_ParryableProjectile_Loop", gameObject);
            playedAudio = true;
        }
        else
        {
            parriableProjectileGhostColorChanger.ApplyDefaultMaterial();
        }
    }
    public void OnDestroy()
    {
        if (playedAudio) RoR2.Util.PlaySound("Stop_DoomTDA_ParryableProjectile_Loop", gameObject);
    }
}
