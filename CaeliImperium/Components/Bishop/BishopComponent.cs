using BrynzaAPI;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components.Bishop;
public class BishopComponent : MonoBehaviour
{
    public static int enableCount {  get; private set; }
    public float maxMoveSpeed = 15f;
    public int maxJumpCount = 2;
    public GenericSkill dashSkill;
    public CharacterBody characterBody;
    public ShakeEmitter shakeEmitter;
    public bool mantle = true;
    public bool mantleOnlyWhenFalling = true;
    public float mantleHeight = 2f;
    public float mantleMaxAngle = 30f;
    public float mantleCooldown = 0.5f;
    public float healFraction = 1f;
    public float reservedHeal;
    public float leftHandPunchTimer;
    public event Action<BishopComponent, MantleInfo> onMantle;
    [HideInInspector] public float previousMoveSpeed;
    [HideInInspector] public int previousMaxJumpCount;
    private bool removeEvent;
    private CharacterMotor characterMotor;
    private InputBankTest inputBankTest;
    private Animator animator;
    private float mantleStopwatch;
    public void Awake()
    {
        if (!characterBody) characterBody = GetComponent<CharacterBody>();
        if (!shakeEmitter) shakeEmitter = GetComponent<ShakeEmitter>();
        if (characterBody)
        {
            characterMotor = characterBody.characterMotor;
            inputBankTest = characterBody.inputBank;
            if (characterBody.modelLocator)
            {
                Transform transform = characterBody.modelLocator.modelTransform;
                if (transform) animator = transform.GetComponent<Animator>();
            }
        }
    }
    public void OnEnable()
    {
        enableCount++;
        if (!characterBody) return;
        removeEvent = true;
        characterBody.onRecalculateStats += CharacterBody_onRecalculateStats;
    }
    public void OnDisable()
    {
        enableCount--;
        if (!characterBody) return;
        if (removeEvent) characterBody.onRecalculateStats -= CharacterBody_onRecalculateStats;
        removeEvent = false;
    }
    public void FixedUpdate()
    {
        if (mantleCooldown > 0f) mantleCooldown -= Time.fixedDeltaTime;
        if (mantle) HandleMantle();
    }
    public void HandleMantle()
    {
        if (mantleStopwatch > 0f || !characterBody || !characterBody.hasEffectiveAuthority || !characterMotor || !inputBankTest || characterMotor.isGrounded) return;
        if (mantleOnlyWhenFalling && characterMotor.velocity.y > 0f) return;
        CapsuleCollider capsuleCollider = characterMotor.capsuleCollider;
        if (!capsuleCollider) return;
        Vector3 vector3 = inputBankTest.aimDirection;
        vector3.y = 0f;
        vector3.Normalize();
        if (vector3 == Vector3.zero) return;
        Vector3 rayPosition = (capsuleCollider.bounds.center + transform.up * (capsuleCollider.height / 2f + mantleHeight)) + (vector3 * (capsuleCollider.radius + 0.1f));
        Vector3 rayDirection = transform.up * -1f;
        float rayDistance = capsuleCollider.height / 2f + mantleHeight;
        if (!Physics.Raycast(rayPosition, rayDirection, out RaycastHit hitInfo, rayDistance, characterMotor.Motor.CollidableLayers)) return;
        float angle = Vector3.Angle(hitInfo.normal, transform.up);
        if (angle < mantleMaxAngle) return;
        float currentY = capsuleCollider.bounds.center.y;
        float targetY = hitInfo.point.y;
        float deltaY = (targetY - currentY) + (capsuleCollider.height);
        if (deltaY < 0f) return;
        float jumpPower = Trajectory.CalculateInitialYSpeedForHeight(deltaY);
        characterMotor.velocity.y += jumpPower;
        mantleStopwatch = mantleCooldown;
        if (animator)
        {
            int layerIndex = animator.GetLayerIndex("Body");
            if (layerIndex >= 0)
            {
                animator.Play("Mantle", layerIndex);
            }
        }
        if (onMantle != null)
        {
            MantleInfo mantleInfo = new MantleInfo
            {
                mantleDistance = deltaY,
                hitInfo = hitInfo
            };
            onMantle.Invoke(this, mantleInfo);
        }
    }
    public void Update()
    {
        if (leftHandPunchTimer > 0f) leftHandPunchTimer -= Time.deltaTime;
    }
    public void Shake(float duration, float radius, float frequency, float amplitude)
    {
        if (!shakeEmitter) return;
        shakeEmitter.duration = duration;
        shakeEmitter.stopwatch = 0f;
        shakeEmitter.wave.frequency = frequency;
        shakeEmitter.wave.amplitude = amplitude;
        shakeEmitter.radius = radius;
    }
    private void CharacterBody_onRecalculateStats(CharacterBody obj)
    {
        float maxMoveSpeed = Mathf.Max(obj.baseMoveSpeed, this.maxMoveSpeed);
        previousMoveSpeed = obj.moveSpeed;
        obj.moveSpeed = Mathf.Min(obj.moveSpeed, maxMoveSpeed);
        int maxJumpCount = Mathf.Max(obj.baseJumpCount, this.maxJumpCount);
        previousMaxJumpCount = obj.maxJumpCount;
        obj.maxJumpCount = Mathf.Min(obj.maxJumpCount, maxJumpCount);
        if (dashSkill)
        {
            dashSkill.cooldownScale = 1f / (previousMoveSpeed / obj.moveSpeed);
            dashSkill.SetBonusStockFromBody(previousMaxJumpCount - obj.maxJumpCount);
        }
    }
    public struct MantleInfo
    {
        public float mantleDistance;
        public RaycastHit hitInfo;
    }
}
