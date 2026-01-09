using CaeliImperium.Components;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.ItemBehaviours
{
    public class DrawSpeedPathBehaviour : CharacterBody.ItemBehavior
    {
        private static GameObject _globalPathsHolder;
        public static Transform globalPathsHolder
        {
            get
            {
                if (_globalPathsHolder == null)
                {
                    _globalPathsHolder = new GameObject("GlobalPathsHolder");
                }
                return _globalPathsHolder.transform;
            }
        }
        public static float groundingDistance = 3f;
        public static int maxPaths = 10000;
        public static float seacrhRadius = 2f;
        public static float createDistance = 3f;
        public static float noCreateDistance = 6f;
        private GlobalSpeedPath _globalSpeedPath;
        public GlobalSpeedPath globalSpeedPath
        {
            get
            {
                if (_globalSpeedPath == null)
                {
                    _globalSpeedPath = Instantiate(CaeliImperiumAssets.GlobalSpeedPathPrefab, globalPathsHolder).GetComponent<GlobalSpeedPath>();
                    _globalSpeedPath.speedPathDrawerComponent = this;
                    globalSpeedPaths.Add(_globalSpeedPath);
                }
                return _globalSpeedPath;
            }
        }
        public List<GlobalSpeedPath> globalSpeedPaths = [];
        public bool foundPath;
        public bool nearestPath;
        public bool nearestFirstPath;
        public List<SpeedPathComponent> speedPathComponents = [];
        public Vector3 previousPosition;
        public float velocity;
        private int previousBuffCount;
        private SpeedPathComponent previousSpeedPathComponent;
        public void Start()
        {
            previousPosition = transform.position;
        }
        public void FixedUpdate()
        {
            if (!body) return;
            int buffCount = FindNearestSpeedPath(transform.position);
            if (foundPath && _globalSpeedPath)
            {
                _globalSpeedPath.Disconect();
                _globalSpeedPath = null;
            }
            if (buffCount != previousBuffCount)
            {
                if (NetworkServer.active) body.SetBuffCount(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex, buffCount);
                previousBuffCount = buffCount;
            }
            if (nearestPath && !_globalSpeedPath)
            {
                previousPosition = transform.position;
            }
            if (!nearestFirstPath) SetPaths(transform.position);
        }
        public void SetPaths(Vector3 position)
        {
            Vector3 vector3 = previousPosition - position;
            float sqrDistance = noCreateDistance * noCreateDistance;
            while (vector3.sqrMagnitude > sqrDistance)
            {
                vector3 -= vector3.normalized * createDistance;
                SetPositionAndCreateOrb(position + vector3);
            }
        }
        public int FindNearestSpeedPath(Vector3 position)
        {
            foundPath = false;
            nearestPath = false;
            nearestFirstPath = false;
            Collider[] colliders = Physics.OverlapSphere(position, noCreateDistance, LayerIndex.pickups.mask, QueryTriggerInteraction.Collide);
            if (colliders == null) return 0;
            float searchDistance = seacrhRadius * seacrhRadius;
            float nearDistance = float.MaxValue;
            foreach (Collider collider in colliders)
            {
                if (!collider.name.StartsWith("SpeedPath")) continue;
                Vector3 vector3 = collider.transform.position - position;
                float sqrMagn = vector3.sqrMagnitude;
                if (!nearestPath && sqrMagn < nearDistance)
                {
                    nearestPath = true;
                    nearDistance = sqrMagn;
                    if (!collider.name.EndsWith("Charged")) nearestFirstPath = true;
                }
                if (!foundPath && sqrMagn < searchDistance)
                {
                    searchDistance = sqrMagn;
                    foundPath = true;
                }
                if (nearestPath && foundPath) break;
            }
            if (foundPath) return stack;
            return 0;
        }
        public void OnDestroy()
        {
            for (int i = 0; i < globalSpeedPaths.Count; i++)
            {
                GlobalSpeedPath globalSpeedPath = globalSpeedPaths[i];
                if (!globalSpeedPath) continue;
                Destroy(globalSpeedPath.gameObject);
            }
            if (!body) return;
            if (NetworkServer.active) body.SetBuffCount(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex, 0);
        }
        public void SetPositionAndCreateOrb(Vector3 orbPoision)
        {
            if (body)
            {
                if (body.characterMotor)
                {
                    velocity = body.characterMotor.velocity.magnitude;
                }
                else if (body.rigidbody)
                {
                    velocity = body.rigidbody.velocity.magnitude;
                }
            }
            //if (!_globalSpeedPath) orbPoision = FindNearestSpeedPathAndConnect(orbPoision);
            Vector3 vector3 = previousPosition;
            previousPosition = orbPoision;
            GameObject gameObject = Instantiate(CaeliImperiumAssets.SpeedPathPrefab, globalSpeedPath.transform);
            gameObject.transform.position = (orbPoision + vector3) / 2f;
            SpeedPathComponent speedPatchComponent = gameObject.GetComponent<SpeedPathComponent>();
            if (!speedPatchComponent) return;
            speedPatchComponent.globalSpeedPath = globalSpeedPath;
            speedPatchComponent.speedPathDrawerComponent = this;
            speedPatchComponent.Charge(previousSpeedPathComponent);
            speedPathComponents.Add(speedPatchComponent);
            if (speedPathComponents.Count >= maxPaths)
            {
                SpeedPathComponent speedPatchComponent1 = speedPathComponents[0];
                speedPathComponents.RemoveAt(0);
                Destroy(speedPatchComponent1.gameObject);
            }
            previousSpeedPathComponent = speedPatchComponent;
        }
        public bool GroundCheck()
        {
            if ((body.characterMotor ? body.characterMotor.isGrounded : true) || Physics.SphereCast(new Ray(transform.position, Physics.gravity.normalized), body.radius, groundingDistance, LayerIndex.GetLayerIndex(LayerMask.LayerToName(gameObject.layer)).collisionMask)) return true;
            return false;
        }
        public Vector3 FindNearestSpeedPathAndConnect(Vector3 position)
        {
            Vector3 vector31 = previousPosition;
            Collider[] colliders = Physics.OverlapSphere(position, noCreateDistance, LayerIndex.pickups.mask, QueryTriggerInteraction.Collide);
            if (colliders == null) return vector31;
            float nearDistance = float.MaxValue;
            foreach (Collider collider in colliders)
            {
                if (!collider.name.StartsWith("SpeedPath")) continue;
                Vector3 vector3 = collider.transform.position - position;
                float sqrMagn = vector3.sqrMagnitude;
                if (sqrMagn < nearDistance)
                {
                    nearDistance = sqrMagn;
                    vector31 = collider.transform.position;
                }
            }
            return vector31;
        }
    }
}
