using CaeliImperiumEntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ItemBehaviours
{
    public class FireBulletOnPrimarySkillBehaviour : CharacterBody.ItemBehavior
    {
        public static float windUpTime = 0.5f;
        public static float fireRate = 0.2f;
        public float stopwatch = 0f;
        public bool firing;
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
            if (!body.inputBank) return;
            if (body.inputBank.skill1.down)
            {
                if (stopwatch > 0f) stopwatch -= Time.fixedDeltaTime;
                if (stopwatch <= 0f)
                {
                    entityStateMachine.SetNextState(new FireSkullGun { stack = this.stack });
                    stopwatch = fireRate;
                }
            }
            else
            {
                stopwatch = windUpTime;
            }

        }
        public void OnDisable()
        {
            if (entityStateMachine) Destroy(entityStateMachine);
        }
    }
}
