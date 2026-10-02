using CaeliImperium.ScriptableObjects;
using HG;
using KinematicCharacterController;
using R2API;
using RoR2;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace CaeliImperium.Components;
public class FirstPersonCameraController : MonoBehaviour
{
    public static FixedConditionalWeakTable<CameraRigController, FirstPersonCameraController> keyValuePairs = [];
    public static FirstPersonCameraController instance;
    public Animator animator;
    public Transform shakeTransform;
    public float shakeIntensity = 0.1f;
    public static event Action<FirstPersonCameraController> onFirstPersonCameraControllerEnable;
    public static event Action<FirstPersonCameraController> onFirstPersonCameraControllerDisable;
    [HideInInspector] public Camera sceneCam;
    [HideInInspector] public SceneCamera sceneCamera;
    [HideInInspector] public CameraRigController cameraRigController;
    [HideInInspector] public Vector3 defailtShakeTransformLocalPosition;
    private bool init;
    public void Awake()
    {
        instance = this;
        if (!animator) animator = GetComponentInChildren<Animator>();
        
    }
    public void OnEnable()
    {
        if (!init) return;
        onFirstPersonCameraControllerEnable?.Invoke(this);
    }
    public void OnDisable()
    {
        if (!init) return;
        onFirstPersonCameraControllerDisable?.Invoke(this);
    }
    public void Start()
    {
        if (!shakeTransform) return;
        defailtShakeTransformLocalPosition = shakeTransform.localPosition;
    }
    public void FixedUpdate()
    {
        if (!cameraRigController) Destroy(gameObject);
    }
    public static void Init(CameraRigController cameraRigController)
    {
        if (keyValuePairs.TryGetValue(cameraRigController, out FirstPersonCameraController firstPersonCameraController))
        {
            keyValuePairs.Remove(cameraRigController);
            GameObject.Destroy(firstPersonCameraController.gameObject);
        }
        if (!cameraRigController.targetBody) return;
        FirstPersonCameraTargetParams firstPersonCameraTargetParams = cameraRigController.targetBody.GetComponent<FirstPersonCameraTargetParams>();
        if (!firstPersonCameraTargetParams) return;
        FirstPersonCharacterCameraParams firstPersonCharacterCameraParams = firstPersonCameraTargetParams.firstPersonCharacterCameraParams;
        if (!firstPersonCharacterCameraParams) return;
        FirstPersonCameraController firstPersonCameraController1 = firstPersonCameraTargetParams.firstPersonCharacterCameraParams.firstPersonCameraController;
        if (!firstPersonCameraController1) return;
        firstPersonCameraController = GameObject.Instantiate(firstPersonCameraController1);
        keyValuePairs.Add(cameraRigController, firstPersonCameraController);
        firstPersonCameraController1.init = true;
        firstPersonCameraController.cameraRigController = cameraRigController;
        if (!firstPersonCameraController.cameraRigController) return;
        firstPersonCameraController.sceneCam = cameraRigController.sceneCam;
        if (firstPersonCameraController.sceneCam)
        {
            firstPersonCameraController.transform.SetParent(firstPersonCameraController.sceneCam.transform, false);
            firstPersonCameraController.sceneCamera = firstPersonCameraController.sceneCam.GetComponent<SceneCamera>();
        }
        firstPersonCameraController.OnEnable();

    }
    public static void PlayCrossfade(string layerName, string animationStateName, string playbackRateParam, float duration, float crossfadeDuration)
    {
        if (!instance || !instance.animator) return;
        instance.animator.PlayCrossfade(layerName, animationStateName, playbackRateParam, duration, crossfadeDuration);
    }
    public static void PlayCrossfade(string layerName, int animationStateNameHash, int playbackRateParamHash, float duration, float crossfadeDuration)
    {
        if (!instance || !instance.animator) return;
        instance.animator.PlayCrossfade(layerName, animationStateNameHash, playbackRateParamHash, duration, crossfadeDuration);
    }
    public static void PlayCrossfade(string layerName, string animationStateName, float crossfadeDuration)
    {
        if (!instance || !instance.animator) return;
        instance.animator.PlayCrossfade(layerName, animationStateName, crossfadeDuration);
    }
    public static void PlayCrossfade(string layerName, int animationStateHash, float crossfadeDuration)
    {
        if (!instance || !instance.animator) return;
        instance.animator.PlayCrossfade(layerName, animationStateHash, crossfadeDuration);
    }
}
