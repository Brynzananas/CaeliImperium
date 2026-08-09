using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaeliImperium.Components
{
    public class MonsterChestPanelHelper : MonoBehaviour
    {
        public InspectPanelController inspectPanelController;
        public InspectPanelController spewedItemInspectPanelController;
        public TextMeshProUGUI sacrificeCountText;
        public MPEventSystem eventSystem;
        public Image currentFillImage;
        public Image expectedFillImage;
        public Transform stripContainer;
        public GameObject stripPrefab;

        public float maxValue = 100f;
        public float currentValue = 100f;
        public float expectedValue = 100f;

        public int totalSegments = 10;
        private int previousTotalSegments;

        private List<GameObject> spawnedStrips = new List<GameObject>();
        private MonsterChestController monsterChestController;
        private Inventory cachedBodyInventory;
        public void Awake()
        {
            MPEventSystemLocator component = base.GetComponent<MPEventSystemLocator>();
            this.eventSystem = component.eventSystem;
            if (this.eventSystem != null && this.eventSystem.localUser != null && this.eventSystem.localUser.cachedBody != null)
            {
                this.cachedBodyInventory = this.eventSystem.localUser.cachedBody.inventory;
            }
        }
        private UserProfile GetUserProfile()
        {
            if (this.eventSystem != null && this.eventSystem.localUser != null)
            {
                return this.eventSystem.localUser.userProfile;
            }
            return null;
        }
        public void Init(MonsterChestController monsterChestController)
        {
            this.monsterChestController = monsterChestController;
            if (!spewedItemInspectPanelController || !monsterChestController) return;
            if (monsterChestController.sacrificesCount >= monsterChestController.neededSacrifices && monsterChestController.expectItem)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef((ItemIndex)monsterChestController.expectedItemIndex);
                if (itemDef)
                {
                    spewedItemInspectPanelController.gameObject.SetActive(true);
                    spewedItemInspectPanelController.Show(itemDef, false, this.GetUserProfile());
                }
            }
            else
            {
                spewedItemInspectPanelController.gameObject.SetActive(false);
            }
        }
        public void UpdateValues()
        {
            if (!monsterChestController) return;
            totalSegments = monsterChestController.neededSacrifices;
            maxValue = monsterChestController.neededSacrifices * 100f;
            currentValue = monsterChestController.sacrificesCount * 100f;
        }
        public void Spew()
        {
            if (!monsterChestController) return;
            monsterChestController.CallSpew();
        }

        public void Update()
        {
            UpdateValues();
            UpdateFills();
            if (previousTotalSegments != totalSegments)
            {
                SetSegmentCount(totalSegments);
            }
            if (this.eventSystem.player.GetButtonDown(15))
            {
                Destroy(base.gameObject);
            }
        }
        public void SetSegmentCount(int count)
        {
            previousTotalSegments = totalSegments;
            GenerateStrips();
        }

        public void UpdateFills()
        {
            if (maxValue <= 0) return;
            float currentFillPercent = currentValue / maxValue;
            float expectedFillPercent = expectedValue / maxValue;
            if (currentFillImage) currentFillImage.fillAmount = currentFillPercent;
            if (expectedFillImage) expectedFillImage.fillAmount = expectedFillPercent;
        }

        public void GenerateStrips()
        {
            if (!stripContainer || !stripPrefab) return;
            foreach (GameObject strip in spawnedStrips)
            {
                if (strip) Destroy(strip);
            }
            spawnedStrips.Clear();
            int dividersToCreate = totalSegments - 1;
            for (int i = 0; i < dividersToCreate; i++)
            {
                GameObject newStrip = Instantiate(stripPrefab, stripContainer);
                newStrip.SetActive(true);
                spawnedStrips.Add(newStrip);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(stripContainer.GetComponent<RectTransform>());
        }
        public void ShowInfo(MPButton button, PickupDef pickupDef)
        {
            this.inspectPanelController.Show(pickupDef, false, this.GetUserProfile());
            int? sacrificeCount = null;
            ItemIndex itemIndex = pickupDef.itemIndex;
            if (monsterChestController && itemIndex != ItemIndex.None)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
                if (itemDef)
                {
                    sacrificeCount = monsterChestController.GetSacrificeCount(ItemCatalog.GetItemDef(pickupDef.itemIndex));
                }
            }
            if (sacrificeCount.HasValue)
            {
                expectedValue = (sacrificeCount.Value * 100f) + currentValue;
                if (sacrificeCountText) sacrificeCountText.text = sacrificeCount.Value.ToString();
            }
            else
            {
                expectedValue = currentValue;
                if (sacrificeCountText) sacrificeCountText.text = "";
            }
        }
        public void AddQuantityToPickerButton(MPButton button, PickupDef pickupDef)
        {
            ItemIndex itemIndex = pickupDef.itemIndex;
            if (itemIndex != ItemIndex.None)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
                if (itemDef)
                {
                    if (!(itemDef.tier == ItemTier.Tier1 || itemDef.tier == ItemTier.Tier2 || itemDef.tier == ItemTier.Tier3))
                    {
                        button.gameObject.SetActive(false); // I have no idea how to exclude boss items from picker panel so I made this shitass solution. TODO
                    }
                }
            }
            if (this.cachedBodyInventory)
            {
                int itemCountPermanent = this.cachedBodyInventory.GetItemCountPermanent(itemIndex);
                TextMeshProUGUI textMeshProUGUI = button.GetComponent<ChildLocator>().FindChildComponent<TextMeshProUGUI>("Quantity");
                if (textMeshProUGUI)
                {
                    if (itemCountPermanent > 1)
                    {
                        textMeshProUGUI.SetText(string.Format("{0}", itemCountPermanent), true);
                        return;
                    }
                    textMeshProUGUI.gameObject.SetActive(false);
                }
            }
        }
    }
}
