using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    public class EquipmentPickerSlot : MonoBehaviour
    {
        [HideInInspector] public int index = -1;
        public HGButton hGButton;
        [HideInInspector] public EquipmentIndex equipmentIndex;
        public void UpdatePickerSlot(EquipmentIndex equipmentIndex)
        {
            this.equipmentIndex = equipmentIndex;
            Sprite sprite = equipmentIndex != EquipmentIndex.None ? EquipmentCatalog.GetEquipmentDef(equipmentIndex).pickupIconSprite : null;
            hGButton.image.sprite = sprite;
        }
    }
}
