using BepInEx;
using CaeliImperium.Configs;
using CaeliImperium.Interactables;
using RoR2;
using RoR2.Hologram;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static CaeliImperium.Interactables.MonsterChestEvents;

namespace CaeliImperium.Components
{
    public class MonsterChestController : NetworkBehaviour
    {
        [SyncVar] public int neededSacrifices;
        [SyncVar] public int sacrificesForTier1;
        [SyncVar] public int sacrificesForTier2;
        [SyncVar] public int sacrificesForTier3;
        [SyncVar] public int sacrificesForBoss;
        [SyncVar] public int sacrificesCount;
        [SyncVar] public int expectedItemIndex;
        [SyncVar] public bool expectItem;
        public List<string> consumedInternalNames = [];
        public bool spewed;
        public Interactor interactor;
        public EntityStateMachine entityStateMachine;
        public HologramProjector hologramProjector;
        public ChildLocator childLocator;
        private static string lastSHA;
        public void Awake()
        {
            if (!entityStateMachine) entityStateMachine = GetComponent<EntityStateMachine>();
            if (!hologramProjector) hologramProjector = GetComponent<HologramProjector>();
            if (!childLocator)
            {
                ModelLocator modelLocator = GetComponent<ModelLocator>();
                if (modelLocator && modelLocator.modelChildLocator)
                {
                    childLocator = modelLocator.modelChildLocator;
                }
            }
            neededSacrifices = MonsterChestConfigs.MonsterChestNeededSacrifices.Value;
            sacrificesForTier1 = MonsterChestConfigs.MonsterChestSacrificeValueForTier1.Value;
            sacrificesForTier2 = MonsterChestConfigs.MonsterChestSacrificeValueForTier2.Value;
            sacrificesForTier3 = MonsterChestConfigs.MonsterChestSacrificeValueForTier3.Value;
            sacrificesForBoss = MonsterChestConfigs.MonsterChestSacrificeValueForTierBoss.Value;
        }
        public void AssignPotentialInteractor(Interactor potentialInteractor)
        {
            interactor = potentialInteractor;
        }
        public void Eat(int intPickupIndex) => Eat(new UniquePickup(new PickupIndex(intPickupIndex)));
        public void Eat(UniquePickup pickupToTake)
        {
            if (!NetworkServer.active) return;
            PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupToTake.pickupIndex);
            if (pickupDef == null) return;
            ItemDef itemDef = ItemCatalog.GetItemDef((pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None);
            if (!itemDef) return;
            CharacterBody characterBody;
            if (!this.interactor.TryGetComponent<CharacterBody>(out characterBody)) return;
            Inventory inventory = characterBody.inventory;
            if (!inventory)  return;
            Inventory.ItemTransformation.TryTransformResult tryTransformResult;
            if (new Inventory.ItemTransformation
            {
                allowWhenDisabled = false,
                forbidPermanentItems = pickupToTake.isTempItem,
                forbidTempItems = !pickupToTake.isTempItem,
                minToTransform = 1,
                maxToTransform = 1,
                originalItemIndex = itemDef.itemIndex,
                newItemIndex = ItemIndex.None,
                transformationType = ItemTransformationTypeIndex.None
            }.TryTransform(inventory, out tryTransformResult))
            {
                //Inventory.ItemAndStackValues takenItem = tryTransformResult.takenItem;
                //takenItem.itemIndex = itemIndex;
                //takenItem.AddAsPickupsToList(this.pickupPrintQueue);
                if (characterBody)
                {
                    for (int i = 0; i < tryTransformResult.totalTransformed; i++)
                    {
                        ScrapperController.CreateItemTakenOrb(characterBody.corePosition, base.gameObject, tryTransformResult.takenItem.itemIndex);
                    }
                }
            }
            int eatAmount = GetSacrificeCount(itemDef);
            for (int i = 0; i < eatAmount; i++)  consumedInternalNames.Add(itemDef.name);
            sacrificesCount += eatAmount;
            if (sacrificesCount >= neededSacrifices)
            {
                if (consumedInternalNames.Count > neededSacrifices)
                {
                    int removeAmount = consumedInternalNames.Count - neededSacrifices;
                    consumedInternalNames.RemoveRange(0, removeAmount);
                }
                ItemIndex itemIndex = ItemIndex.None;
                expectItem = TryGetItemIndex(out itemIndex);
                expectedItemIndex = (int)itemIndex;
            }
            if (entityStateMachine) entityStateMachine.SetNextState(new CaeliImperiumEntityStates.MonsterChest.Eat());
        }
        public int GetSacrificeCount(ItemDef itemDef)
        {
            int sacrificeCount = 0;
            if (!itemDef) return sacrificeCount;
            switch (itemDef.tier)
            {
                case ItemTier.Tier1:
                    sacrificeCount = sacrificesForTier1;
                    break;
                case ItemTier.Tier2:
                    sacrificeCount = sacrificesForTier2;
                    break;
                case ItemTier.Tier3:
                    sacrificeCount = sacrificesForTier3;
                    break;
                case ItemTier.Boss:
                    sacrificeCount = sacrificesForBoss;
                    break;
                default:
                    sacrificeCount = baseSacrificesForDefault;
                    break;

            }
            return sacrificeCount;
        }
        public void CallSpew()
        {
            if (NetworkServer.active)
            {
                Spew();
            }
            else
            {
                CmdSpew();
            }
        }
        [Command]
        public void CmdSpew() => Spew();
        public void Spew()
        {
            if (spewed || sacrificesCount < neededSacrifices) return;
            spewed = true;
            if (entityStateMachine) entityStateMachine.SetNextState(new CaeliImperiumEntityStates.MonsterChest.Spew());
        }
        public bool TryGetItemIndex(out ItemIndex itemIndex)
        {
            itemIndex = ItemIndex.None;
            string id = null;
            string sha = "";
            foreach (string str in consumedInternalNames)
            {
                sha += str;
            }
            sha = sha.SHA256Encode();
            lastSHA = sha;
            id = CaeliImperiumUtils.FindMostSimilar(sha, MonsterChestEvents.GetBossItemNamesToSHA());
            if (id == null || id.IsNullOrWhiteSpace() || !MonsterChestEvents.SHAToName.TryGetValue(id, out string itemName)) return false;
            itemIndex = ItemCatalog.FindItemIndex(itemName);
            if (itemIndex == ItemIndex.None) return false;
            return true;
        }
    }
}
