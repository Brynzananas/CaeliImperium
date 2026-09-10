using CaeliImperium.Configs;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    public class SuperSecretScreamComponent : MonoBehaviour
    {
        
        public AnimationCurve postProcessAnimateAlpha;
        public float postProcessDuration = 1f;
        private static bool addHook;
        private static int _count;
        public static int count
        {
            get => _count;
            set
            {
                if (value > 0 && !addHook)
                {
                    MusicController.pickTrackHook += MusicController_pickTrackHook;
                    CaeliImperiumExpansionRunComponent.getDeathValue += CaeliImperiumExpansionRunComponent_getDeathValue;
                    addHook = true;
                }
                if (value <= 0 && addHook)
                {
                    MusicController.pickTrackHook -= MusicController_pickTrackHook;
                    CaeliImperiumExpansionRunComponent.getDeathValue -= CaeliImperiumExpansionRunComponent_getDeathValue;
                    addHook = false;
                }
                _count = value;
            }
        }
        private float age;

        private static void CaeliImperiumExpansionRunComponent_getDeathValue(ref float deathScream)
        {
            deathScream += 1f;
        }

        private static void MusicController_pickTrackHook(MusicController musicController, ref MusicTrackDef newTrack)
        {
            newTrack = null;
        }
        public void OnEnable()
        {
            count++;
            age = 0f;
            GlitchHUDController.globalUpdateGlitchValues += GlitchHUDController_globalUpdateGlitchValues;
            Camera camera = Camera.main;
            if (!camera) return;
            Util.PlaySound("Play_DeathScream", camera.gameObject);
        }
        public void Update()
        {
            age += Time.deltaTime;
        }

        private void GlitchHUDController_globalUpdateGlitchValues(GlitchHUDController glitchHUDController, ref GlitchHUDController.GlitchValues glitchValues)
        {
            if (postProcessAnimateAlpha == null) return;
            glitchValues.enableCount++;
            glitchValues.postProcessWeight += postProcessAnimateAlpha.Evaluate(age / postProcessDuration);
        }

        public void OnDisable()
        {
            count--;
            GlitchHUDController.globalUpdateGlitchValues -= GlitchHUDController_globalUpdateGlitchValues;
        }
    }
}
