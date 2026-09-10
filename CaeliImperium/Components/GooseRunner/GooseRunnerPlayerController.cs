using Rewired;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components.GooseRunner;

[RequireComponent(typeof(CharacterController))]
public class GooseRunnerPlayerController : MonoBehaviour
{
    public static GooseRunnerPlayerController instance {  get; private set; }
    public CharacterController characterController;
    [HideInInspector] public PlayerCharacterMasterController playerCharacterMasterController;
    public float laneChangeSpeed = 14f;
    public float jumpVelocity = 9f;
    public float gravity = -30f;
    public float slideDuration = 0.7f;
    public float standingHeight = 2f;
    public float slidingHeight = 1f;
    public Vector3 standingCenter = new Vector3(0, 1f, 0);
    public Vector3 slidingCenter = new Vector3(0, 0.5f, 0);
    public CharacterMaster characterMaster => playerCharacterMasterController ? playerCharacterMasterController.master : null;
    public CharacterBody characterBody => playerCharacterMasterController ? playerCharacterMasterController.body : null;
    public InputBankTest inputBankTest => playerCharacterMasterController ? playerCharacterMasterController.bodyInputs : null;
    public NetworkUser networkUser => playerCharacterMasterController ? playerCharacterMasterController.networkUser : null;
    public Player player => networkUser ? networkUser.inputPlayer : null;
    public bool IsSliding { get; private set; }
    public bool IsAirborne => !characterController.isGrounded;
    private int currentLane = 1;
    private float verticalVelocity;
    private Coroutine slideRoutine;
    private bool awaitNextMoveInput;
    private Vector3? holdBodyPosition;

    public void Awake()
    {
        instance = this;
        if (!characterController) characterController = GetComponent<CharacterController>();
        ReadOnlyCollection<PlayerCharacterMasterController> playerCharacterMasterControllers = PlayerCharacterMasterController.instances;
        playerCharacterMasterController = CaeliImperiumUtils.GetPlayerCharacterMasterController();
    }

    public void FixedUpdate()
    {
        if (!GooseRunnerManager.instance || GooseRunnerManager.instance.gameOver) return;
        LockBody();
        if (!playerCharacterMasterController)
        {
            GooseRunnerManager.instance.GameOver();
            return;
        }
        HandleInputFixedUpdate();
        Move(Time.fixedDeltaTime);
    }
    public void LockBody()
    {
        if (!characterBody) return;
        if (holdBodyPosition.HasValue)
        {
            if (characterBody.characterMotor)
            {
                if (characterBody.characterMotor.Motor) characterBody.characterMotor.Motor.SetPosition(holdBodyPosition.Value, true);
            }
            else if (characterBody.rigidbody)
            {
                characterBody.rigidbody.position = holdBodyPosition.Value;
            }
            else
            {
                characterBody.transform.position = holdBodyPosition.Value;
            }
        }
        else
        {
            holdBodyPosition = characterBody.transform.position;
        }
            
    }
    public void Update()
    {
        if (!GooseRunnerManager.instance || GooseRunnerManager.instance.gameOver) return;
        HandleInputUpdate();
    }
    public void HandleInputUpdate()
    {
        if (!inputBankTest) return;
        if (inputBankTest.rawMoveLeft.justPressed) ChangeLane(-1);
        if (inputBankTest.rawMoveRight.justPressed) ChangeLane(1);
        if (inputBankTest.rawMoveUp.justPressed) Jump();
        if (inputBankTest.rawMoveDown.justPressed) Slide();
    }
    public void HandleInputFixedUpdate()
    {
        if (!inputBankTest) return;
        if (inputBankTest.jump.justPressed) Jump();
    }
    public void ChangeLane(int direction)
    {
        currentLane = GooseRunnerManager.instance.ClampLane(currentLane + direction);
    }
    public void Jump()
    {
        if (!characterController.isGrounded || IsSliding) return;
        verticalVelocity = jumpVelocity;
    }
    public void Slide()
    {
        if (IsSliding || !characterController.isGrounded) return;
        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(SlideRoutine());
    }
    public IEnumerator SlideRoutine()
    {
        IsSliding = true;
        Vector3 vector3 = transform.localScale;
        Vector3 vector32 = vector3;
        vector32.y /= 2f;
        transform.localScale = vector32;
        yield return new WaitForSeconds(slideDuration);
        transform.localScale = vector3;
        IsSliding = false;
    }
    public void Move(float deltaTime)
    {
        if (!GooseRunnerManager.instance) return;
        float targetX = GooseRunnerManager.instance.GetLaneX(currentLane);
        float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * deltaTime);
        float deltaX = newX - transform.position.x;
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * deltaTime;
        }
        float forwardSpeed = GooseRunnerManager.instance.currentSpeed;
        Vector3 motion = new Vector3(deltaX, verticalVelocity * deltaTime, forwardSpeed * deltaTime);
        characterController.Move(motion);
    }
    public void OnDeath()
    {
        if (!GooseRunnerManager.instance) return;
        GooseRunnerManager.instance.GameOver();
    }
}

