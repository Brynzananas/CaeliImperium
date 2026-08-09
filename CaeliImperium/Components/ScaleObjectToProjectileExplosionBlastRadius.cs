using BrynzaAPI;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    [RequireComponent(typeof(ProjectileGhostController))]
    public class ScaleObjectToProjectileExplosionBlastRadius : MonoBehaviour
    {
        public ProjectileGhostController projectileGhostController;
        public Transform objectToScale;
        public bool scaleObjectOnStart;
        public Transform timerTransform;
        public bool scaleTimerOnStart;
        public float smoothTime;
        public float timerSmoothTime;
        public ProjectileImpactCapsuleExplosion.ScaleEffectByAxis scaleEffectByAxis = ProjectileImpactCapsuleExplosion.ScaleEffectByAxis.Y;
        public Vector3 addRotationForCapsuleExplosion = new Vector3(90f, 0f, 0f);
        private ProjectileExplosion _projectileExplosion;
        private ProjectileImpactExplosion _projectileImpactExplosion;
        private ProjectileImpactCapsuleExplosion _projectileImpactCapsuleExplosion;
        private bool isCapsuled;
        private Quaternion previousRotation;
        private ProjectileExplosion projectileExplosion
        {
            get
            {
                if (_projectileExplosion == null)
                {
                    if (!projectileGhostController) return _projectileExplosion;
                    Transform transform = projectileGhostController.predictionTransform ?? projectileGhostController.authorityTransform;
                    if (!transform) return _projectileExplosion;
                    ProjectileExplosion projectileExplosion = transform.GetComponent<ProjectileExplosion>();
                    if (!projectileExplosion) return _projectileExplosion;
                    _projectileExplosion = projectileExplosion;
                }
                return _projectileExplosion;
            }
        }
        private ProjectileImpactExplosion projectileImpactExplosion
        {
            get
            {
                if (_projectileImpactExplosion == null) _projectileImpactExplosion = projectileExplosion ? projectileExplosion as ProjectileImpactExplosion : null;
                return _projectileImpactExplosion;
            }
        }
        private ProjectileImpactCapsuleExplosion projectileImpactCapsuleExplosion
        {
            get
            {
                if (_projectileImpactCapsuleExplosion == null) _projectileImpactCapsuleExplosion = projectileExplosion ? projectileExplosion as ProjectileImpactCapsuleExplosion : null;
                return _projectileImpactCapsuleExplosion;
            }
        }
        private float smoothTimeVelocity;
        private float timerSmoothTimeVelocity;
        public void Awake()
        {
            if (!projectileGhostController) projectileGhostController = GetComponent<ProjectileGhostController>();
            if (objectToScale) previousRotation = objectToScale.rotation;
        }
        public void Start()
        {
            timerSmoothTimeVelocity = 0f;
            smoothTimeVelocity = 0f;
            if (objectToScale && scaleObjectOnStart)
            {
                objectToScale.localScale = Vector3.zero;
            }
            if (timerTransform && scaleTimerOnStart)
            {
                timerTransform.localScale = Vector3.zero;
            }
        }
        public void LateUpdate()
        {
            if (objectToScale && projectileExplosion)
            {
                float x = Mathf.SmoothDamp(objectToScale.localScale.x, projectileExplosion.blastRadius, ref smoothTimeVelocity, smoothTime, float.MaxValue, Time.deltaTime);
                if (projectileImpactCapsuleExplosion)
                {
                    Vector3 endPositionAdd = projectileImpactCapsuleExplosion.endPositionAdd;
                    if (projectileImpactCapsuleExplosion.endPositionAddSpace == ProjectileImpactCapsuleExplosion.Space.Local) endPositionAdd = projectileExplosion.transform.rotation * endPositionAdd;
                    Vector3 endPosition = projectileExplosion.transform.position + endPositionAdd;
                    Vector3 positionAdd = projectileImpactCapsuleExplosion.positionAdd;
                    if (projectileImpactCapsuleExplosion.positionAddSpace == ProjectileImpactCapsuleExplosion.Space.Local) positionAdd = projectileExplosion.transform.rotation * positionAdd;
                    Vector3 position = projectileExplosion.transform.position + positionAdd;
                    Vector3 vector3 = endPosition - position;
                    objectToScale.rotation = RoR2.Util.QuaternionSafeLookRotation(vector3) * Quaternion.Euler(projectileImpactCapsuleExplosion.addRotationToEffect);
                    objectToScale.localScale = projectileImpactCapsuleExplosion ? new Vector3(projectileImpactCapsuleExplosion.scaleEffectByAxis == ProjectileImpactCapsuleExplosion.ScaleEffectByAxis.X ? projectileImpactCapsuleExplosion.GetDistanceBetweenPositions() : x, projectileImpactCapsuleExplosion.scaleEffectByAxis == ProjectileImpactCapsuleExplosion.ScaleEffectByAxis.Y ? projectileImpactCapsuleExplosion.GetDistanceBetweenPositions() : x, projectileImpactCapsuleExplosion.scaleEffectByAxis == ProjectileImpactCapsuleExplosion.ScaleEffectByAxis.Z ? projectileImpactCapsuleExplosion.GetDistanceBetweenPositions() : x) : x.ToVector3();
                }
                else
                {
                    objectToScale.rotation = previousRotation;
                    objectToScale.localScale = x.ToVector3();
                }
            }
            if (timerTransform && projectileImpactExplosion)
            {
                float x;
                if (projectileImpactExplosion.timerAfterImpact)
                {
                    x = projectileImpactExplosion.stopwatchAfterImpact / projectileImpactExplosion.lifetimeAfterImpact;
                }
                else
                {
                    x = projectileImpactExplosion.stopwatch / projectileImpactExplosion.lifetime;
                }
                x = Mathf.SmoothDamp(timerTransform.localScale.x, x, ref timerSmoothTimeVelocity, timerSmoothTime, float.MaxValue, Time.deltaTime);
                timerTransform.localScale = projectileImpactCapsuleExplosion ? new Vector3(x, 1f, x) : x.ToVector3();
            }
        }
    }
}
