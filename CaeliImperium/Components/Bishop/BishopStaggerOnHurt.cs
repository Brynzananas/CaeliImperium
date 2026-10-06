using CaeliImperium;
using CaeliImperium.Bodies;
using RoR2;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumComponents.Bishop;
[DisallowMultipleComponent]
public class BishopStaggerOnHurt : MonoBehaviour, IOnTakeDamageServerReceiver
{
    public static FixedConditionalWeakTable<DamageReport, BishopStaggerOnHurt> keyValuePairs = [];
    public int staggerAmount;
    public float staggerDuration;
    public float staggerInvincibilityDuration;
    public float wasStaggeredDuration;
    public float superStunDuration;
    public bool overrideGloryKillHeal;
    public float overrideGloryKillHealFraction;
    public bool overrideGloryKillHealFromReserve;
    public float overrideGloryKillHealFromReserveFraction;
    private float nextHealthPercentageUntilStagger;
    private int staggerCount;
    public void Start()
    {
        RecalculateNextHealthPercentageUntilStagger();
    }
    public void OnTakeDamageServer(DamageReport damageReport)
    {
        HealthComponent victimHealthComponent = damageReport.victim;
        if (!victimHealthComponent || !victimHealthComponent.alive) return;
        if (victimHealthComponent.healthFraction > nextHealthPercentageUntilStagger) return;
        CharacterBody victimCharacterBody = victimHealthComponent.body;
        if (!victimCharacterBody || victimCharacterBody.HasBuff(BishopEvents.Stagger)) return;
        staggerCount++;
        RecalculateNextHealthPercentageUntilStagger();
        victimCharacterBody.AddStagger(staggerDuration, staggerInvincibilityDuration, wasStaggeredDuration);
        if (superStunDuration > 0f) victimCharacterBody.SuperStun(superStunDuration);
        if (!keyValuePairs.ContainsKey(damageReport)) keyValuePairs.Add(damageReport, this);
    }
    public void RecalculateNextHealthPercentageUntilStagger()
    {
        if (staggerAmount <= 0) return;
        nextHealthPercentageUntilStagger = 1 - (float)(staggerCount + 1) / (float)(staggerAmount + 1);
    }
}
