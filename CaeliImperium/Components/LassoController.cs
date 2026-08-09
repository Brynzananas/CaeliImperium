using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    public class LassoController : MonoBehaviour
    {
        public HurtBox hitHurtbox {  get; private set; }
        public Collider hitCollider {  get; private set; }
        public Vector3 endAngle { get; private set; }
        public float updatesPerSecond;
        public int maxUpdates;
        public Vector3 origin;
        public Vector3 target;
        public Vector3 direction;
        public float speed;
        public float angle;
        public GameObject owner;
        private LineRenderer trailRenderer;
        private GameObject lassoEffect;
        private float stopwatch;
        public void Awake()
        {
            lassoEffect = Instantiate(CaeliImperiumAssets.LassoEffect);
            trailRenderer = lassoEffect.GetComponent<LineRenderer>();
        }
        public void OnDestroy()
        {
            if (lassoEffect) Destroy(lassoEffect);
        }
        public void FixedUpdate()
        {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= 1f / updatesPerSecond)
            {
                Vector3 newOrigin = origin;
                Vector3 vector31 = direction;
                List<Vector3> vectors = [];
                vectors.Add(newOrigin);
                bool hit = false;
                for (int i = 0; i < maxUpdates; i++)
                {
                    Vector3 vector3 = target - newOrigin;
                    vector31 = Vector3.RotateTowards(vector31, vector3.normalized, angle * 0.01745329f / (origin - target).magnitude, 0f);
                    if (Physics.Raycast(newOrigin, vector31, out RaycastHit hitInfo, speed, LayerIndex.entityPrecise.mask | LayerIndex.world.mask))
                    {
                        HurtBox box = hitInfo.collider.GetComponent<HurtBox>();
                        if (box && box.healthComponent && box.healthComponent.gameObject != owner)
                        {
                            trailRenderer.startColor = Color.green;
                            trailRenderer.endColor = Color.green;
                            hitHurtbox = box;
                            hit = true;
                        }
                        hitCollider = hitInfo.collider;
                        vectors.Add(hitInfo.point);
                        break;
                    }
                    vectors.Add(newOrigin += vector31);
                }
                if (!hit)
                {
                    trailRenderer.startColor = Color.red;
                    trailRenderer.endColor = Color.red;
                    hitHurtbox = null;
                }
                Vector3[] vector3s = vectors.ToArray();
                if (vector3s.Length > 2)
                {
                    endAngle = (vector3s[vector3s.Length - 2] - vector3s[vector3s.Length - 1]).normalized;
                }
                else
                {
                    endAngle = direction;
                }
                trailRenderer.positionCount = vector3s.Length;
                trailRenderer.SetPositions(vector3s);
                stopwatch = 0f;
            }
        }
    }
}
