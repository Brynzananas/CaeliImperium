using RoR2;
using System;
using System.Collections.ObjectModel;
using UnityEngine;
namespace CaeliImperium.Components.GooseRunner;
public class GooseRunnerCameraController : MonoBehaviour, ICameraStateProvider
{
    public static GooseRunnerCameraController instance { get; private set; }
    public Vector3 offset = new Vector3(0f, 5f, -7f);
    public float upOffset = 1.5f;
    public float smoothTime = 0.15f;
    public float fov = 60f;
    public bool hudAllowed;
    public bool userLookAllowed;
    public bool userControlAllowed;
    [Range(0f, 1f)] public float lateralFollowStrength = 0.6f;
    private Vector3 velocity;
    public void Awake()
    {
        instance = this;
    }
    public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
    {
        cameraState = new CameraState
        {
            position = transform.position,
            rotation = transform.rotation,
            fov = fov
        };
    }
    public bool IsHudAllowed(CameraRigController cameraRigController) => hudAllowed;
    public bool IsUserControlAllowed(CameraRigController cameraRigController) => userControlAllowed;
    public bool IsUserLookAllowed(CameraRigController cameraRigController) => userLookAllowed;
    public void LateUpdate()
    {
        if (!GooseRunnerPlayerController.instance) return;
        if (GooseRunnerPlayerController.instance.characterBody) UpdateCameras(GooseRunnerPlayerController.instance.characterBody.gameObject);
        Transform targetTransform = GooseRunnerPlayerController.instance.transform;
        Vector3 desired = new Vector3(
            targetTransform.position.x * lateralFollowStrength + offset.x,
            targetTransform.position.y + offset.y,
            targetTransform.position.z + offset.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime, float.MaxValue, Time.deltaTime);
        transform.LookAt(targetTransform.position + Vector3.up * upOffset);
    }
    public void UpdateCameras(GameObject characterBodyObject)
    {
        ReadOnlyCollection<CameraRigController> readOnlyInstancesList = CameraRigController.readOnlyInstancesList;
        for (int i = 0; i < readOnlyInstancesList.Count; i++)
        {
            CameraRigController cameraRigController = readOnlyInstancesList[i];
            if (characterBodyObject && cameraRigController.target == characterBodyObject)
            {
                cameraRigController.SetOverrideCam(this, 0f);
            }
            else if (cameraRigController.IsOverrideCam(this))
            {
                cameraRigController.SetOverrideCam(null, 0.05f);
            }
        }
    }
    public void OnDisable()
    {
        UpdateCameras(null);
    }

}
