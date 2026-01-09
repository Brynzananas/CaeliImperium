using CaeliImperiumEntityStates;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ItemBehaviours
{
    public class ChargeAtomicBeamOnSpecialSkillBehaviour : CharacterBody.ItemBehavior
    {
        public static float timeToFire = 1f;
        public static float maxDamageMultiplier = 100f;
        public static float dotDuration = 5f;
        public float stopwatch = 0f;
        public float charge = 0f;
        public bool fire = false;
        public EntityStateMachine entityStateMachine;
        public void OnEnable()
        {
            BrynzaAPI.Assets.EntityStateMachineAdditionInfo entityStateMachineAdditionInfo = new BrynzaAPI.Assets.EntityStateMachineAdditionInfo
            {
                entityStateMachineName = "balls"
            };
            entityStateMachine = BrynzaAPI.Utils.AddEntityStateMachine(body, entityStateMachineAdditionInfo);
        }
        public void FixedUpdate()
        {
            if (!body || !body.hasEffectiveAuthority) return;
            charge += Time.fixedDeltaTime * stack;
            if (body.inputBank && body.inputBank.skill4.justPressed)
            {
                stopwatch = timeToFire / body.attackSpeed;
                fire = true;
            }
            if (stopwatch > 0f && fire) stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0f && fire)
            {
                FireLaser();
            }
        }
        public void OnDisable()
        {
            if (entityStateMachine) Destroy(entityStateMachine);
        }
        public void FireLaser()
        {
            entityStateMachine.SetNextState(new FireAtomicBeam { stack = this.stack, charge = this.charge });
            charge = 0f;
            fire = false;
        }
    }
}
