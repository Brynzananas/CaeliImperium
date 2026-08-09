using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.Victor
{
    public class AimSpear : BaseSkillState
    {
        public static float velocitySmoothTime = 0.2f;
        public static float priority = 60f;
        public static float transitionDuration = 0.2f;
        public static CharacterCameraParams characterCameraParams;
        private CameraTargetParams.CameraParamsOverrideHandle cameraParamsOverrideHandle;
        private static Vector3 velocityVelocity;
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_Deadlock_Venator_Crossbow_Up", gameObject);
            Util.PlaySound("Play_Deadlock_Venator_Crossbow_Loop", gameObject);
            this.cameraParamsOverrideHandle = base.cameraTargetParams.AddParamsOverride(new CameraTargetParams.CameraParamsOverrideRequest
            {
                cameraParamsData = characterCameraParams.data,
                priority = priority
            }, transitionDuration);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            if (!IsKeyDownAuthority())
            {
                outer.SetNextState(new AimDownSpear());
            }
            if (inputBank && inputBank.skill1.justPressed)
            {
                activatorSkillSlot?.DeductStock(1);
                outer.SetNextState(new FireSpear { activatorSkillSlot = activatorSkillSlot });
            }
            if (characterMotor)
            {
                characterMotor.velocity = Vector3.Lerp(characterMotor.velocity, Vector3.zero, fixedAge / velocitySmoothTime);
            }else if (rigidbody)
            {
                rigidbody.velocity = Vector3.Lerp(characterMotor.velocity, rigidbody.velocity, fixedAge / velocitySmoothTime);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            Util.PlaySound("Stop_Deadlock_Venator_Crossbow_Up", gameObject);
            Util.PlaySound("Stop_Deadlock_Venator_Crossbow_Loop", gameObject);
            if (cameraParamsOverrideHandle.isValid)
            {
                cameraTargetParams.RemoveParamsOverride(cameraParamsOverrideHandle, transitionDuration);
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
}
