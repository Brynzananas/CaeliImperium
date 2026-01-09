using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    public class WormholeComponent : MonoBehaviour
    {
        public float timer = 10f;
        public float stopwatch = 0f;
        public bool isRecharged
        {
            get { return stopwatch >= timer; }
            set { stopwatch = timer; }

        }
        public void FixedUpdate()
        {
            if (stopwatch < timer) stopwatch += Time.fixedDeltaTime;
        }
        public void Teleport(CharacterBody characterBody)
        {
            TeleportHelper.TeleportBody(characterBody, transform.position, false);
        }
    }
}
