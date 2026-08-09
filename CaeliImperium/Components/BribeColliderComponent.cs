using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    [RequireComponent(typeof(EntityLocator))]
    [RequireComponent(typeof(SphereCollider))]
    public class BribeColliderComponent : MonoBehaviour
    {
        public EntityLocator entityLocator;
        public SphereCollider sphereCollider;
        public CharacterBody characterBody;
        public void FixedUpdate()
        {
            if (!characterBody) return;
            sphereCollider.radius = characterBody.bestFitActualRadius;
        }
    }
}
