using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
public class HealthReductionZone : MonoBehaviour
{
    public AnimationCurve damageFalloff;
    public float distance;
    [SerializeField] private Transform _damageCenter;
    public Transform damageCenter => _damageCenter ?? transform; 
    public float percentageDamage;
    public float baseDamage;
    public void OnTriggerStay(Collider collider)
    {
        CharacterBody characterBody = collider.GetComponent<CharacterBody>();
        if (!characterBody) return;
        HealthComponent healthComponent = characterBody.healthComponent;
        if (!healthComponent || !healthComponent.alive) return;
        float distance = Vector3.Distance(collider.transform.position, damageCenter.position);
        float coof = damageFalloff == null ? 1f : damageFalloff.Evaluate(distance / this.distance);
        float damage = (characterBody.maxHealth * percentageDamage / 100f + baseDamage) * coof;
        healthComponent.Networkhealth -= Time.fixedDeltaTime * damage;
    }
    
}
