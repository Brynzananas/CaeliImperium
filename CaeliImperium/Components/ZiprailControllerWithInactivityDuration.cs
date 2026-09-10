using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
public class ZiprailControllerWithInactivityDuration : ZiprailController
{
    public float inactivityDuration = 0.2f;
    public float duration;
    public void Start()
    {
        SetInactivityDuration();
    }
    public void FixedUpdate()
    {
        if (duration > 0) duration -= Time.fixedDeltaTime;
    }
    public void SetInactivityDuration()
    {
        duration = inactivityDuration;
    }
}
