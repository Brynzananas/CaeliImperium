using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
public class CameraOverride : MonoBehaviour, ICameraStateProvider
{
    public delegate void GetCameraStateDelegate(CameraRigController cameraRigController, ref CameraState state);
    public GetCameraStateDelegate getCameraStateDelegate;
    public bool isHudAllowed;
    public bool isUserControlAllowed;
    public bool isUserLookAllowed;
    public CharacterBody characterBody;
    public float cameraUpdateLerpDuration;
    public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState) => getCameraStateDelegate?.Invoke(cameraRigController, ref cameraState);
    public bool IsHudAllowed(CameraRigController cameraRigController) => isHudAllowed;
    public bool IsUserControlAllowed(CameraRigController cameraRigController) => isUserControlAllowed;
    public bool IsUserLookAllowed(CameraRigController cameraRigController) => isUserLookAllowed;
    public void Start() => UpdateCameras(characterBody);
    public void OnDestroy() => UpdateCameras(null);
    public void UpdateCameras(CharacterBody characterBody)
    {
        ReadOnlyCollection<CameraRigController> readOnlyInstancesList = CameraRigController.readOnlyInstancesList;
        for (int i = 0; i < readOnlyInstancesList.Count; i++)
        {
            CameraRigController cameraRigController = readOnlyInstancesList[i];
            if (characterBody && cameraRigController.target == characterBody.gameObject)
            {
                cameraRigController.SetOverrideCam(this, cameraUpdateLerpDuration);
            }
            else if (cameraRigController.IsOverrideCam(this))
            {
                cameraRigController.SetOverrideCam(null, cameraUpdateLerpDuration);
            }
        }
    }
}
