using BrynzaAPI;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;

[RequireComponent(typeof(ProjectileGhostController))]
public class PlaceObjectOnProjectileCapsuleExposionLine : MonoBehaviour
{
    public ProjectileGhostController projectileGhostController;
    public Transform objectToPlace;
    public float smoothTime;
    private ProjectileImpactCapsuleExplosion _projectileImpactCapsuleExplosion;
    private ProjectileImpactCapsuleExplosion projectileImpactCapsuleExplosion
    {
        get
        {
            if (_projectileImpactCapsuleExplosion == null)
            {
                if (!projectileGhostController) return _projectileImpactCapsuleExplosion;
                Transform transform = projectileGhostController.predictionTransform ?? projectileGhostController.authorityTransform;
                if (!transform) return _projectileImpactCapsuleExplosion;
                ProjectileImpactCapsuleExplosion projectileExplosion = transform.GetComponent<ProjectileImpactCapsuleExplosion>();
                _projectileImpactCapsuleExplosion = projectileExplosion;
            }
            return _projectileImpactCapsuleExplosion;
        }
    }
    private PlaceObjectOnProjectileCapsuleExposionLineRunAction placeObjectOnProjectileCapsuleExposionLineRunAction;
    private Vector3 smoothVelocity;
    private Vector3 originalObjectPosition;
    public void Awake()
    {
        if (!projectileGhostController) projectileGhostController = GetComponent<ProjectileGhostController>();
        if (placeObjectOnProjectileCapsuleExposionLineRunAction == null) placeObjectOnProjectileCapsuleExposionLineRunAction = new PlaceObjectOnProjectileCapsuleExposionLineRunAction(this);
        if (objectToPlace) originalObjectPosition = objectToPlace.localPosition;
    }
    public void OnEnable()
    {
        smoothVelocity = Vector3.zero;
        if (objectToPlace) objectToPlace.localPosition = originalObjectPosition;
        CaeliImperiumExpansionRunComponent.caeliImperiumRunActions.Add(placeObjectOnProjectileCapsuleExposionLineRunAction);
    }
    public void OnDisable()
    {
        CaeliImperiumExpansionRunComponent.caeliImperiumRunActions.Remove(placeObjectOnProjectileCapsuleExposionLineRunAction);
    }
    public class PlaceObjectOnProjectileCapsuleExposionLineRunAction : CaeliImperiumRunAction
    {
        public PlaceObjectOnProjectileCapsuleExposionLine placeObjectOnProjectileCapsuleExposionLine;
        public PlaceObjectOnProjectileCapsuleExposionLineRunAction(PlaceObjectOnProjectileCapsuleExposionLine placeObjectOnProjectileCapsuleExposionLine)
        {
            this.placeObjectOnProjectileCapsuleExposionLine = placeObjectOnProjectileCapsuleExposionLine;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!placeObjectOnProjectileCapsuleExposionLine || !placeObjectOnProjectileCapsuleExposionLine.objectToPlace) return;
            if (!currentCharacterBody)
            {
                if (placeObjectOnProjectileCapsuleExposionLine.objectToPlace.gameObject.activeSelf) placeObjectOnProjectileCapsuleExposionLine.objectToPlace.gameObject.SetActive(false);
                return;
            }
            else
            {
                if (!placeObjectOnProjectileCapsuleExposionLine.objectToPlace.gameObject.activeSelf) placeObjectOnProjectileCapsuleExposionLine.objectToPlace.gameObject.SetActive(true);
            }
            ProjectileImpactCapsuleExplosion projectileImpactCapsuleExplosion = placeObjectOnProjectileCapsuleExposionLine.projectileImpactCapsuleExplosion;
            if (!projectileImpactCapsuleExplosion) return;
            Vector3 endPositionAdd = projectileImpactCapsuleExplosion.endPositionAdd;
            if (projectileImpactCapsuleExplosion.endPositionAddSpace == ProjectileImpactCapsuleExplosion.Space.Local) endPositionAdd = placeObjectOnProjectileCapsuleExposionLine.transform.rotation * endPositionAdd;
            Vector3 endPosition = placeObjectOnProjectileCapsuleExposionLine.transform.position + endPositionAdd;
            Vector3 positionAdd = projectileImpactCapsuleExplosion.positionAdd;
            if (projectileImpactCapsuleExplosion.positionAddSpace == ProjectileImpactCapsuleExplosion.Space.Local) positionAdd = placeObjectOnProjectileCapsuleExposionLine.transform.rotation * positionAdd;
            Vector3 position = placeObjectOnProjectileCapsuleExposionLine.transform.position + positionAdd;
            Vector3 vector3 = endPosition - position;
            Vector3 newPosition = BrynzaAPI.Utils.NearestPointOnLine(position, vector3, currentCharacterBody.transform.position);
            if (placeObjectOnProjectileCapsuleExposionLine.smoothTime == 0f)
            {
                placeObjectOnProjectileCapsuleExposionLine.objectToPlace.position = newPosition;
            }
            else
            {
                placeObjectOnProjectileCapsuleExposionLine.objectToPlace.position = Vector3.SmoothDamp(placeObjectOnProjectileCapsuleExposionLine.objectToPlace.position, newPosition, ref placeObjectOnProjectileCapsuleExposionLine.smoothVelocity, placeObjectOnProjectileCapsuleExposionLine.smoothTime, float.MaxValue, Time.fixedDeltaTime);
            }
        }
    }
}
