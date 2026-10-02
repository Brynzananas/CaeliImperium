using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumComponents;
public class SetLightToAmbientSettings : MonoBehaviour
{
    public Light light;
    public void Awake()
    {
        if (!light) light = GetComponent<Light>();
    }
    public void Start()
    {
        if (!light) return;
        light.color = RenderSettings.ambientSkyColor;
        light.intensity = RenderSettings.ambientIntensity;
    }
}
