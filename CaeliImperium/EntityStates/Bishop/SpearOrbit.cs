using BrynzaAPI;
using CaeliImperium;
using CaeliImperium.Bodies;
using CaeliImperium.Components;
using EntityStates;
using R2API.Networking;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates.Bishop;
public class SpearOrbit : BishopMainState
{
    public static float baseDuration = 3f;
    public static float baseDurationAddition = 7f;
    public static float cameraUpdateLerpDuration = 0.1f;
    public static float lockOnDistance = 12f;
    public static float pullSpeedCoefficient = 2f;
    public static float orbitSpeedCoefficient = 2f;
    public static float strafeSpeedCoefficient = 2f;
    public static float distanceToTargetUntilExit = 2f;
    public static float distanceBetweenTargetWhenLockon = 24f;
    public static float speedAtWhichYouFlyTowardsThatDistance = 24f;
    public NetworkInstanceId hitBodyNetId;
    public CharacterBody targetBody;
    public HurtBox targetHurtbox;
    public float duration;
    public float durationAddition;
    public bool startedLockOn;
    public bool orbitToTheRight;
    public bool finishedDeserializing;
    public Vector3 previousLookVector;
    public float pullSpeed;
    public float strafeSpeed;
    public float orbitSpeed;
    public Vector3? finalVector;
    public CameraOverride cameraOverride;
    public override void OnEnter()
    {
        base.OnEnter();
        SetValues();
        Util.PlaySound("Play_DoomTDA_Spear_ThrowOrbit_Impact", gameObject);
        characterBody.AddClientBuff(RoR2Content.Buffs.Nullified);
    }
    public void SetValues()
    {
        duration = baseDuration;
        pullSpeed = characterBody.moveSpeed * pullSpeedCoefficient;
        strafeSpeed = characterBody.moveSpeed * strafeSpeedCoefficient;
        orbitSpeed = characterBody.moveSpeed * orbitSpeedCoefficient;
        durationAddition = baseDurationAddition;
        finishedDeserializing = true;
        targetBody = hitBodyNetId.GetCharacterBody();
        if (!targetBody) return;
        targetHurtbox = targetBody.mainHurtBox;
    }
    public override void OnExit()
    {
        base.OnExit();
        characterBody.RemoveClientBuff(RoR2Content.Buffs.Nullified);
        if (!isAuthority) return;
        EndLockOn();
        if (finalVector.HasValue) this.SetVelocity(finalVector.Value);
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        Vector3? pullVector = null;
        Vector3? normalizedPullVector = null;
        Vector3 right = Vector3.zero;
        Vector3 up = Vector3.zero;
        if (targetHurtbox)
        {
            pullVector = targetHurtbox.collider.GetColliderCenter() - transform.position;
            normalizedPullVector = pullVector.Value.normalized;
            if (characterDirection && characterDirection.targetTransform)
            {
                characterDirection.forward = pullVector.Value.normalized;
                right = characterDirection.targetTransform.right;
                up = characterDirection.targetTransform.up;
            }
            else
            {
                transform.forward = pullVector.Value.normalized;
                right = transform.right;
                up = transform.up;
            }
            Vector3 evctor3 = targetHurtbox.collider.ClosestPoint(transform.position) - transform.position;
            if (!startedLockOn && evctor3.sqrMagnitude < distanceToTargetUntilExit * distanceToTargetUntilExit)  outer.SetNextStateToMain();
        }
        if (!isAuthority) return;
        if (fixedAge > duration) outer.SetNextStateToMain();
        if (!finishedDeserializing) return;
        if (!targetBody || !targetHurtbox)
        {
            outer.SetNextStateToMain();
            return;
        }
        Vector3? moveVector = normalizedPullVector.HasValue ? startedLockOn ? orbitToTheRight ? right : right * -1f : normalizedPullVector.Value : null;
        Vector3 straveVector = Vector3.zeroVector;
        if (inputBank)
        {
            if (inputBank.skill2.justPressed)
            {
                StartLockOn();
            }
            if (inputBank.skill2.justReleased)
            {
                EndLockOn();
            }
            if (inputBank.rawMoveData != Vector2.zero)
            {
                straveVector += (up * inputBank.rawMoveData.y) + (right * inputBank.rawMoveData.x);
                if (inputBank.rawMoveData.y > 0f && characterMotor.isGrounded && characterMotor.Motor) characterMotor.Motor.ForceUnground();
            }
            if (inputBank.jump.justPressed)
            {
                outer.SetNextStateToMain();
            }
        }
        if (!moveVector.HasValue)
        {
            finalVector = null;
            return;
        }
        finalVector = (moveVector.Value * (startedLockOn ? orbitSpeed : pullSpeed)) + (straveVector * strafeSpeed);
        if (startedLockOn)
        {
            Vector3 vector3 = targetHurtbox.collider.GetColliderCenter() + (normalizedPullVector.Value * -1f * distanceBetweenTargetWhenLockon);
            Vector3 vector31 = vector3 - transform.position;
            finalVector += vector31;
        }
        this.SetVelocity(Vector3.zero);
        this.AddRootMotion(finalVector.Value * Time.fixedDeltaTime);
    }
    public void StartLockOn()
    {
        if (startedLockOn) return;
        startedLockOn = true;
        characterBody.AddClientBuff(RoR2Content.Buffs.NoCooldowns);
        duration += durationAddition;
        if (!cameraOverride)
        {
            cameraOverride = gameObject.AddComponent<CameraOverride>();
            cameraOverride.cameraUpdateLerpDuration = cameraUpdateLerpDuration;
            cameraOverride.characterBody = characterBody;
            cameraOverride.isHudAllowed = true;
            cameraOverride.isUserControlAllowed = true;
            cameraOverride.isUserLookAllowed = false;
            cameraOverride.getCameraStateDelegate = GetCameraState;
        }
        if (inputBank && inputBank.rawMoveRight.down)
        {
            orbitToTheRight = true;
        }
        else
        {
            orbitToTheRight = false;
        }
    }
    public void EndLockOn()
    {
        if (!startedLockOn) return;
        startedLockOn = false;
        characterBody.RemoveClientBuff(RoR2Content.Buffs.NoCooldowns);
        duration -= durationAddition;
        if (cameraOverride) Destroy(cameraOverride);
    }
    public override void OnSerialize(NetworkWriter writer)
    {
        base.OnSerialize(writer);
        writer.Write(hitBodyNetId);
    }
    public override void OnDeserialize(NetworkReader reader)
    {
        base.OnDeserialize(reader);
        finishedDeserializing = true;
        hitBodyNetId = reader.ReadNetworkId();
    }
    public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
    {
        if (!targetHurtbox) return;
        Collider collider = targetHurtbox.collider;
        Vector3 hurtboxCenter = collider.GetColliderCenter();
        Vector3 lookVector = (hurtboxCenter - cameraState.position).normalized;
        if (previousLookVector == Vector3.zero) previousLookVector = lookVector;
        float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        Vector3 interpolatedLookVector = Vector3.Lerp(previousLookVector, lookVector, interpolationFactor);
        previousLookVector = lookVector;
        if (inputBank) inputBank.aimDirection = interpolatedLookVector;
        cameraState.rotation = Util.QuaternionSafeLookRotation(interpolatedLookVector);
    }
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
}
