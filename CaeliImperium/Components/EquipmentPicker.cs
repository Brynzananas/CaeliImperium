using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    public class EquipmentPicker : MonoBehaviour
    {
        public Transform grid;
        [HideInInspector] public ExtraEquipmentSlotBehaviour extraEquipmentSlotBehaviour;
        [HideInInspector] public bool blockEquipmentUse;
        public void OnDisable()
        {
            blockEquipmentUse = false;
        }
        public void UpdatePicker()
        {
            foreach (Transform slot in grid) Destroy(slot.gameObject);
            for (int i = 0; i < extraEquipmentSlotBehaviour.equipments.Length; i++)
            {
                EquipmentPickerSlot equipmentPickerSlot = Instantiate(CaeliImperiumAssets.EquipmentPickerSlot, grid).GetComponent<EquipmentPickerSlot>();
                EquipmentIndex equipmentIndex = extraEquipmentSlotBehaviour.equipments[i];
                equipmentPickerSlot.UpdatePickerSlot(equipmentIndex);
                equipmentPickerSlot.index = i;
                equipmentPickerSlot.hGButton.onClick.AddListener(SelectEquipment);
                void SelectEquipment()
                {
                    EquipmentIndex newEquipmentIndex = extraEquipmentSlotBehaviour.SelectEquipment(equipmentPickerSlot.index);
                    equipmentPickerSlot.UpdatePickerSlot(newEquipmentIndex);
                    blockEquipmentUse = true;
                }
            }
        }
    }
}
