using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    [RequireComponent(typeof(EffectComponent))]
    public class ScaleEffectTime : MonoBehaviour
    {
        public EffectComponent effectComponent;
        public List<ParticleSystem> particleSystems;
        public void Awake()
        {
            if (!effectComponent) effectComponent = GetComponent<EffectComponent>();
        }
        public void Start()
        {
            if (!effectComponent) return;
            EffectData effectData = effectComponent.effectData;
            if (effectData == null || effectData.genericFloat <= 0f) return;
            if (particleSystems != null)
                foreach (ParticleSystem p in particleSystems)
                {
                    if (!p) continue;
                    ParticleSystem.MainModule main = p.main;
                    main.simulationSpeed = 1f / effectData.genericFloat;
                }
        }
    }
}
