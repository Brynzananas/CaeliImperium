using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;

public class LayDownSword : MonoBehaviour
{
    public Transform startTransform;
    public Transform endTransform;
    public Transform transformToRotate;
    public float addPosition;
    public float castDistance;
    public float castRadius;
    public float smoothTime;
    private Vector3 velocity;
    private Vector3 targetVector;
    private float sqrDistance;
    public void Start()
    {
        if (!startTransform || !endTransform) return;
        Vector3 vector3 = endTransform.position - startTransform.position;
        sqrDistance = vector3.sqrMagnitude;
    }
    public void LateUpdate()
    {
        if (!startTransform || !endTransform || !transformToRotate) return;
        
        if (Physics.CapsuleCast(startTransform.position, endTransform.position, castRadius, Physics.gravity.normalized, out RaycastHit hitInfo, castDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
        {
            Vector3 vector3 = ((hitInfo.point + (Physics.gravity.normalized * -addPosition)) - transformToRotate.position).normalized;
            Vector3 vector32 = startTransform.position + Physics.gravity.normalized * hitInfo.distance;
            Vector3 vector33 = vector3 - vector32;
            float coof = vector33.sqrMagnitude / sqrDistance;
            //if (smoothTime <= 0)
            //{
            //    targetVector = vector3;
            //}
            //else
            //{
            //    targetVector = Vector3.SmoothDamp(targetVector, vector3, ref velocity, smoothTime, float.MaxValue, Time.deltaTime);
            //}
            float localZ = transformToRotate.localEulerAngles.z;
            transformToRotate.forward = Vector3.Lerp(transformToRotate.forward, vector3, coof);
            Vector3 vector31 = transformToRotate.localEulerAngles;
            vector31.z = localZ;
            transformToRotate.localEulerAngles = vector31;
        }
    }
}
