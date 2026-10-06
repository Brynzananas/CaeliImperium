using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
public class FirstPersonCameraViewbob : MonoBehaviour
{
    public FirstPersonCameraController firstPersonCameraController;
    public Transform viewBobTransform;
    public float viewbobFrequency = 6f;
    public float viewbobHorizontalAmount = 0.05f;
    public float viewbobVerticalAmount = 0.04f;
    public float viewbobSmoothing = 10f;
    public bool breathe = true;
    public AnimationCurve breatheAnimationCurve = CaeliImperiumUtils.CreateAnimationCurveEaseInOutPingPong(0f, 0f, 1f, 1f);
    public float breathAmount = 0.02f;
    public float breathFrequency = 0.5f;
    private Vector3 defaultLocalPosition;
    private float viewbobTimer;
    private float breathTimer;
    public void Awake()
    {
        if (!firstPersonCameraController) firstPersonCameraController = GetComponent<FirstPersonCameraController>();
    }
    public void Start()
    {
        if (viewBobTransform) defaultLocalPosition = viewBobTransform.localPosition;
    }
    public void HandleViewBob()
    {
        if (!viewBobTransform || !firstPersonCameraController || !firstPersonCameraController.cameraRigController) return;
        CharacterBody characterBody = firstPersonCameraController.cameraRigController.targetBody;
        if (!characterBody) return;
        Vector3 horizontalVelocity;
        if (characterBody.characterMotor)
        {
            horizontalVelocity = characterBody.characterMotor.velocity;
            horizontalVelocity.y = 0f;
        }
        else if (characterBody.rigidbody)
        {
            horizontalVelocity = characterBody.rigidbody.velocity;
            horizontalVelocity.y = 0f;
        }
        else
        {
            return;
        }
        Vector3 targetPosition = defaultLocalPosition;
        if (horizontalVelocity.sqrMagnitude > 0.001f && (characterBody.characterMotor ? characterBody.characterMotor.isGrounded : true))
        {
            viewbobTimer += Time.deltaTime * viewbobFrequency;
            float horizontalOffset = Mathf.Sin(viewbobTimer) * viewbobHorizontalAmount;
            float verticalOffset = Mathf.Sin(viewbobTimer * 2.0f) * -viewbobVerticalAmount;
            targetPosition += new Vector3(horizontalOffset, verticalOffset, 0);
        }
        else
        {
            viewbobTimer = 0f;
        }
        if (breathe && breatheAnimationCurve != null)
        {
            breathTimer += Time.deltaTime * breathFrequency;
            if (breathTimer >= 2f) breathTimer -= 2f;
            float breatheOffset = breatheAnimationCurve.Evaluate(breathTimer) * breathAmount;
            targetPosition += new Vector3(0f, breatheOffset, 0f);
        }
        viewBobTransform.localPosition = Vector3.Lerp(viewBobTransform.localPosition, targetPosition, Time.deltaTime * viewbobSmoothing);
    }
    public void LateUpdate()
    {
        HandleViewBob();
    }
}
