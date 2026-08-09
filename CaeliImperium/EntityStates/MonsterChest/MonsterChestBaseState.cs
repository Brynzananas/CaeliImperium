using CaeliImperium.Components;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates.MonsterChest
{
    public class MonsterChestBaseState : BaseState
    {
        public virtual bool enableInteraction
        {
            get
            {
                return true;
            }
        }
        public override void OnEnter()
        {
            base.OnEnter();
            this._impatient = false;
            this.pickupPickerController = base.GetComponent<PickupPickerController>();
            this.monsterChestController = base.GetComponent<MonsterChestController>();
            if (NetworkServer.active)
            {
                this.pickupPickerController.SetAvailable(this.enableInteraction);
            }
        }
        public PickupPickerController pickupPickerController;
        public MonsterChestController monsterChestController;
        public bool _impatient;
    }
}
