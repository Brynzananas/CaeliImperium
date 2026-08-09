using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    public class LassoJointComponent : MonoBehaviour
    {
        public Renderer renderer;
        public Rigidbody rigidbody;
        public static List<LassoJointComponent> instances = [];
        public event Action<Collision, bool> OnHit;
        public Collider[] colliders;
        public void OnEnable()
        {
            instances.Add(this);
        }
        public void OnDisable()
        {
            instances.Remove(this);
        }
        public void OnCollisionEnter(Collision collision)
        {
            if (!collision.collider) return;
            HurtBox hurtBox = collision.collider.GetComponent<HurtBox>();
            if (!hurtBox)
            {
                ChangeLassoColor(Color.red);
                OnHit.Invoke(collision, false);
                return;
            }
            ChangeLassoColor(Color.green);
            OnHit.Invoke(collision, true);
            Destroy(gameObject);
        }
        public static void ChangeLassoColor(Color color)
        {
            foreach (LassoJointComponent lassoJointComponent in instances)
            {
                if (!lassoJointComponent || !lassoJointComponent.renderer || !lassoJointComponent.renderer.material) continue;
                lassoJointComponent.renderer.material.color = color;
            }
        }
        public void IgnoreCollisionsWithBody(GameObject bodyObject, bool shouldIgnore)
        {
            if (colliders == null || colliders.Length == 0) return;
            if (!bodyObject) return;
            ModelLocator modelLocator = bodyObject.GetComponent<ModelLocator>();
            if (!modelLocator) return;
            Transform modelTransform = modelLocator.modelTransform;
            if (!modelTransform) return;
            HurtBoxGroup hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
            if (!hurtboxGroup) return;
            HurtBox[] hurtBoxes = hurtboxGroup.hurtBoxes;
            for (int i = 0; i < hurtBoxes.Length; i++)
            {
                List<Collider> gameObjectComponents = GetComponentsCache<Collider>.GetGameObjectComponents(hurtBoxes[i].gameObject);
                int j = 0;
                int count = gameObjectComponents.Count;
                while (j < count)
                {
                    Collider collider = gameObjectComponents[j];
                    for (int k = 0; k < colliders.Length; k++)
                    {
                        Collider collider2 = colliders[k];
                        Physics.IgnoreCollision(collider, collider2, shouldIgnore);
                    }
                    j++;
                }
                GetComponentsCache<Collider>.ReturnBuffer(gameObjectComponents);
            }
        }
    }
}
