using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.ItemBehaviours
{
    public class PeriodicDamageIncreaseBehaviour : CharacterBody.ItemBehavior
    {
        public static float cooldown = 15f;
        public float stopwatch = cooldown;
        public void FixedUpdate()
        {
            if (stopwatch > 0f) stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0f)
            {
                if (NetworkServer.active) body.AddTimedBuff(CaeliImperiumContent.Buffs.IncreaseDamagePereodically, 2f);
                stopwatch = cooldown / stack;
            }
        }
    }
}
