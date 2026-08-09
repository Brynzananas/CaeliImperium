using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    public class GutsTempPickupDecay : MonoBehaviour
    {
        public PickupDropletController pickupDropletController;
        public void Start()
        {
            if (!pickupDropletController) pickupDropletController = GetComponent<PickupDropletController>();
        }
        public void FixedUpdate()
        {
            if (!pickupDropletController) return;
            GenericPickupController.CreatePickupInfo createPickupInfo = pickupDropletController.createPickupInfo;
            UniquePickup uniquePickup = createPickupInfo.pickup;
            float decay = uniquePickup.decayValue;
            decay -= 1f / 80f * Time.fixedDeltaTime;
            if (decay <= 0f)
            {
                Destroy(gameObject);
                return;
            }
            uniquePickup.decayValue = decay;
            createPickupInfo.pickup = uniquePickup;
            pickupDropletController.createPickupInfo = createPickupInfo;
        }
    }
}
