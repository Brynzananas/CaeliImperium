using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates.MonsterChest
{
    public class Spew : MonsterChestBaseState
    {
        public static float duration = 1f;
        public static float dropUpVelocityStrength = 16f;
        public static float dropForwardVelocityStrength = 16f;
        private static int SpewStateHash = Animator.StringToHash("Spew");
        private static int SpewParamHash = Animator.StringToHash("Spew.playbackRate");
        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Base", SpewStateHash, SpewParamHash, duration);
            if (NetworkServer.active)
            {
                if (monsterChestController && monsterChestController.expectItem)
                {
                    UniquePickup uniquePickup = new UniquePickup(new PickupIndex(PickupCatalog.FindPickupIndex((ItemIndex)monsterChestController.expectedItemIndex).value));
                    GenericPickupController.CreatePickupInfo createPickupInfo = new GenericPickupController.CreatePickupInfo
                    {
                        position = transform.position,
                        pickup = uniquePickup
                    };
                    PickupDropletController.CreatePickupDroplet(createPickupInfo, createPickupInfo.position, Vector3.up * dropUpVelocityStrength + transform.forward * dropForwardVelocityStrength);
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > duration)
            {
                GameObject.Destroy(gameObject);
            }
        }
    }
}
