using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    public class ExtraEquipmentSlotBehaviour : MonoBehaviour
    {
        public CharacterBody characterBody;
        public CharacterMaster characterMaster;
        public bool canExecuteEquipment = false;
        public bool opened = false;
        public EquipmentIndex[] equipments = new EquipmentIndex[] { };
        public Inventory inventory;
        public EquipmentSlot equipmentSlot;
        public int itemCount = 0;
        public int previousArraySize = 0;
        public bool useEquipment = true;
        public InputBankTest inputBankTest;
        public static EquipmentPicker equipmentPicker;

        public void Awake()
        {
            characterMaster = GetComponent<CharacterMaster>();
            characterMaster.onBodyStart += CharacterMaster_onBodyStart;
            GetBodyComponents();
            if (equipmentPicker == null) equipmentPicker = Utils.CreateEquipmentPicker();
            if (equipmentPicker) equipmentPicker.extraEquipmentSlotBehaviour = this;
        }
        public void Start()
        {

        }
        private void CharacterMaster_onBodyStart(CharacterBody obj)
        {
            GetBodyComponents();
        }

        public void Destroy()
        {
            characterMaster.onBodyStart -= CharacterMaster_onBodyStart;
        }
        public void GetBodyComponents()
        {
            characterBody = characterMaster ? characterMaster.GetBody() : null;
            inventory = characterBody ? characterBody.inventory : null;
            if (inventory != null)
            {
                //inventory.onInventoryChanged += Inventory_onInventoryChanged;
                itemCount = characterBody.inventory.GetItemCountEffective(CaeliImperiumContent.Items.ExtraEquipmentSlot);
            }
            equipmentSlot = characterBody ? characterBody.equipmentSlot : null;
            inputBankTest = characterBody ? characterBody.inputBank : null;
        }
        public void FixedUpdate()
        {
            itemCount = inventory.GetItemCountEffective(CaeliImperiumContent.Items.ExtraEquipmentSlot);
            //if (inputBankTest)
            //{
            //    if (inputBankTest.ping.down)
            //    {
            //        if (inputBankTest.skill1.justPressed) MoveEquipmentUpwards();
            //        if (inputBankTest.skill2.justPressed) MoveEquipmentDownwards();
            //    }

            //}
            if (itemCount != equipments.Length)
            {
                if (equipments.Length < itemCount)
                {
                    int toChange = equipments.Length;
                    Array.Resize(ref equipments, itemCount);
                    for (int i = 0; i < (itemCount - toChange); i++)
                    {
                        equipments.SetValue(EquipmentIndex.None, equipments.Length - (i + 1));

                    }
                }
                else
                {
                    for (int i = 0; i < (equipments.Length - itemCount); i++)
                    {
                        EquipmentIndex pickupIndex = (EquipmentIndex)equipments.GetValue(equipments.Length - (i + 1));
                        if (pickupIndex != EquipmentIndex.None)
                        {
                            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(pickupIndex), characterBody.corePosition, Physics.gravity * -1f);

                        }

                    }
                    Array.Resize(ref equipments, itemCount);
                }
                equipmentPicker.UpdatePicker();
            }
            previousArraySize = equipments.Length;
        }
        private void Inventory_onInventoryChanged()
        {
            itemCount = characterBody ? characterBody.inventory.GetItemCountEffective(CaeliImperiumContent.Items.ExtraEquipmentSlot) : 0;
        }
        public void MoveEquipmentUpwards()
        {
            EquipmentIndex equipmentDef = equipments[0];
            Array.Copy(equipments, 1, equipments, 0, equipments.Length - 1);
            EquipmentIndex equipmentIndex = inventory.GetEquipmentIndex();
            equipments[equipments.Length - 1] = equipmentIndex;
            inventory.SetEquipmentIndex(equipmentDef);
            useEquipment = false;
        }
        public void MoveEquipmentDownwards()
        {
            Array.Reverse(equipments);
            EquipmentIndex equipmentDef = equipments[0];
            Array.Copy(equipments, 1, equipments, 0, equipments.Length - 1);
            EquipmentIndex equipmentIndex = inventory.GetEquipmentIndex();
            equipments[equipments.Length - 1] = equipmentIndex;
            Array.Reverse(equipments);
            inventory.SetEquipmentIndex(equipmentDef);
            useEquipment = false;
        }
        public EquipmentIndex SelectEquipment(int index)
        {
            EquipmentIndex equipmentDef = equipments[index];
            EquipmentIndex equipmentIndex = inventory.GetEquipmentIndex();
            equipments[index] = equipmentIndex;
            inventory.SetEquipmentIndex(equipmentDef);
            return equipmentIndex;
        }
    }
}
