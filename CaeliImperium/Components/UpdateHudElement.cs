using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
[RequireComponent(typeof(HudElement))]
public class UpdateHudElement : MonoBehaviour
{
    public HudElement hudElement;
    private HUD hud;
    public void Awake()
    {
        if (!hudElement) hudElement = GetComponent<HudElement>();
        hud = GetComponentInParent<HUD>();
    }
    public void Update()
    {
        if (!hudElement || !hud) return;
        hudElement.hud = hud;
    }
}
