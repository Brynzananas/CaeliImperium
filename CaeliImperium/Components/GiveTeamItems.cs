using JetBrains.Annotations;
using RoR2;
using RoR2.UI;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    public class GiveTeamItems : MonoBehaviour
    {
        public static int instancesCount;
        public static FixedConditionalWeakTable<ScoreboardController, GameObject> strips = [];
        public List<ItemIndex> itemIndices = [];
        public TeamIndex teamIndex;
        public int previousItemStacks;
        public CharacterMaster characterMaster;
        public static Inventory playerInventory;
        public void AddRandomItem()
        {
            HG.ReadOnlyArray<ItemIndex> itemIndices = ItemCatalog.GetItemsWithTag(ItemTag.Technology);
            List<ItemIndex> finalItemIndices = [];
            for (int i = 0; i < itemIndices.Length; i++)
            {
                ItemIndex itemIndex1 = itemIndices[i];
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex1);
                if (!itemDef || itemDef.ContainsTag(ItemTag.CannotCopy)) continue;
                finalItemIndices.Add(itemIndex1);
            }
            ItemIndex itemIndex = finalItemIndices[UnityEngine.Random.Range(0, finalItemIndices.Count)];
            AddItem(itemIndex);
        }
        public void AddItem(ItemIndex itemIndex)
        {
            itemIndices.Add(itemIndex);
            TeamSharedInventory.GiveTeamItemCountEffective(teamIndex, itemIndex);
            if (!playerInventory) return;
            if (teamIndex == TeamIndex.Player) playerInventory.GiveItemPermanent(itemIndex);
        }
        public void RemoveLastItem()
        {
            int itemCount = itemIndices.Count;
            if (itemCount <= 0) return;
            ItemIndex itemIndex = itemIndices[itemCount - 1];
            itemIndices.RemoveAt(itemCount - 1);
            TeamSharedInventory.RemoveTeamItemCountEffective(teamIndex, itemIndex);
            if (!playerInventory) return;
            if (teamIndex == TeamIndex.Player) playerInventory.RemoveItemPermanent(itemIndex);
        }
        public void FixedUpdate()
        {
            if (!characterMaster || teamIndex == characterMaster.teamIndex) return;
            List<ItemIndex> backupItemIndices = itemIndices.MemberwiseClone() as List<ItemIndex>;
            while (itemIndices.Count > 0) RemoveLastItem();
            teamIndex = characterMaster.teamIndex;
            foreach (ItemIndex itemIndex in backupItemIndices)
            {
                AddItem(itemIndex);
            }
        }
        public void Awake()
        {
            if (!playerInventory)
            {
                GameObject gameObject = new GameObject("PlayerTechnologyInventory");
                GameObject.DontDestroyOnLoad(gameObject);
                playerInventory = gameObject.AddComponent<Inventory>();
            }
            instancesCount++;
        }
        public void OnDestroy()
        {
            while (itemIndices.Count > 0) RemoveLastItem();
            instancesCount--;
        }
    }
}
