using CaeliImperium;
using CaeliImperium.Components;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.Test
{
    public class LassoTest : BaseSkillState
    {
        public static float lassoUpdatePerSecond = 60;
        public static float targetUpdatePerSecond = 60;
        public static float maxDistance = 60f;
        public static float targetMaxAngle = 60f;
        public static int lassoMaxUpdates = 60;
        public static float lassoSpeed = 1f;
        public static float lassoAngle = 90f;
        public static float lassoPull = 1f;
        public BullseyeSearch bullseyeSearch;
        public LassoController lassoController;
        public HurtBox hurtBox;
        public override void OnEnter()
        {
            base.OnEnter();
            Ray ray = GetAimRay();
            bullseyeSearch = new BullseyeSearch
            {
                maxDistanceFilter = maxDistance,
                maxAngleFilter = targetMaxAngle,
                searchDirection = ray.direction,
                searchOrigin = ray.origin,
                sortMode = BullseyeSearch.SortMode.Angle,
                teamMaskFilter = TeamMask.all,
            };
            bullseyeSearch.teamMaskFilter.RemoveTeam(GetTeam());
            bullseyeSearch.RefreshCandidates();
            bullseyeSearch.FilterOutGameObject(gameObject);
            hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
            if (!hurtBox)
            {
                if (isAuthority) outer.SetNextStateToMain();
                return;
            }
            lassoController = gameObject.AddComponent<LassoController>();
            lassoController.angle = lassoAngle;
            lassoController.speed = lassoSpeed;
            lassoController.origin = ray.origin;
            lassoController.direction = ray.direction;
            lassoController.target = hurtBox.transform.position;
            lassoController.maxUpdates = lassoMaxUpdates;
            lassoController.updatesPerSecond = lassoUpdatePerSecond;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!lassoController || !hurtBox)
            {
                if (isAuthority) outer.SetNextStateToMain();
                return;
            }
            Ray ray = GetAimRay();
            lassoController.direction = ray.direction;
            lassoController.origin = ray.origin;
            lassoController.target = hurtBox.transform.position;
            if (!isAuthority || IsKeyDownAuthority()) return;
            outer.SetNextStateToMain();
        }
        public override void OnExit()
        {
            base.OnExit();
            if (lassoController)
            {
                if (lassoController.hitHurtbox)
                {
                    CharacterBody characterBody = lassoController.hitHurtbox.healthComponent?.body;
                    if (characterBody)
                    {
                        if (characterBody.characterMotor)
                        {
                            characterBody.characterMotor.ApplyForceImpulse(CreatePhysForceInfo());
                        }
                        else if (characterBody.rigidbody)
                        {
                            characterBody.rigidbody.AddForceWithInfo(CreatePhysForceInfo());
                        }
                    }
                }
                Destroy(lassoController);
            }
        }
        public PhysForceInfo CreatePhysForceInfo()
        {
            Ray ray = GetAimRay();
            return new PhysForceInfo
            {
                disableAirControlUntilCollision = true,
                force = lassoPull * (Vector3.Angle(-ray.direction, lassoController.endAngle) * (ray.direction + lassoController.endAngle).normalized),
                massIsOne = true,
                resetVelocity = true,
                respectKnockupImmune = true,
                ignoreGroundStick = true,
                doNotExceed = false
            };
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
    }
}
