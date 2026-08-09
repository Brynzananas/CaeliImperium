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
                    addHook = true;
                }
                if (value <= 0 && addHook)
                {
                    MusicController.pickTrackHook -= MusicController_pickTrackHook;
                    addHook = false;
                }
                _count = value;
            }
        }

        private static void MusicController_pickTrackHook(MusicController musicController, ref MusicTrackDef newTrack)
        {
            newTrack = null;
        }

        public void OnEnable()
        {
            count++;
            Camera camera = Camera.main;
            if (!camera) return;
            Util.PlaySound("Play_SuperSecretScream", camera.gameObject);
        }
        public void OnDisable()
        {
            count--;
        }
    }
}
