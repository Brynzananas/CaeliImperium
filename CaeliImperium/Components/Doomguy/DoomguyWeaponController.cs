using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components.Doomguy;
public class DoomguyWeaponController : MonoBehaviour
{
    public GameObject crosshairPrefab;
    public Transform muzzle;
    public string loopAnimationString;
    public int rightWeaponIndex;
    public ParticleSystem[] muzzleParticleSystems;
    public void StartMuzzleParticles()
    {
        if (muzzleParticleSystems == null) return;
        foreach (var muzzle in muzzleParticleSystems) muzzle.Play();
    }
}
