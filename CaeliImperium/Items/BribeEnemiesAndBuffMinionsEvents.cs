using BrynzaAPI;
using CaeliImperium.Components;
using R2API;
using RoR2;
using RoR2.UI;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Items
{
    public static class BribeEnemiesAndBuffMinionsEvents
    {
        public static GameObject BribeCollider;
        internal static HashSet<Type> onDestroyCallbackTypeFilter = [];
        internal static HashSet<string> onDestroyCallbackMethodFilter = [];
        private static bool inited;
        public static void Init(ItemDef itemDef)
        {
            CaeliImperiumPlugin.onPluginDestroyed += OnPluginDestroyed;
            CaeliImperiumHooks.OnSetBodyPrefabsIndividualPrefab += HandleBribeEnemy;
            On.RoR2.Inventory.GetItemCountEffective_ItemIndex += Inventory_GetItemCountEffective_ItemIndex;
            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
            On.RoR2.UI.ScoreboardController.Rebuild += ScoreboardController_Rebuild;
            if (inited) return;
            inited = true;
            BribeCollider = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Prefabs/BribeCollider.prefab");
            WhitelistTypeForOnDestroyCallback(typeof(CombatSquad));
            WhitelistMethodForOnDestroyCallback(nameof(CombatSquad.OnMemberDestroyedServer));
        }

        private static void ScoreboardController_Rebuild(On.RoR2.UI.ScoreboardController.orig_Rebuild orig, ScoreboardController self)
        {
            orig(self);
            if (GiveTeamItems.instancesCount <= 0)
            {
                if (GiveTeamItems.strips.TryGetValue(self, out GameObject strip2))
                {
                    GiveTeamItems.strips.Remove(self);
                    GameObject.Destroy(strip2);
                }
                return;
            }
            if (!GiveTeamItems.strips.TryGetValue(self, out GameObject strip))
            {
                strip = GameObject.Instantiate(self.stripPrefab, self.container);
                ScoreboardStrip scoreboardStrip = strip.GetComponent<ScoreboardStrip>();
                scoreboardStrip.inventory = GiveTeamItems.playerInventory;
                scoreboardStrip.itemInventoryDisplay.SetSubscribedInventory(scoreboardStrip.inventory);
                scoreboardStrip.classIcon.texture = CaeliImperiumContent.Items.BribeEnemiesAndBuffMinions.pickupIconSprite.texture;
                scoreboardStrip.nameLabel.text = RoR2.Language.GetString(CaeliImperiumContent.Items.BribeEnemiesAndBuffMinions.nameToken);
                GiveTeamItems.strips.Add(self, strip);
            }
            strip.transform.SetAsLastSibling();
        }

        private static FixedConditionalWeakTable<CharacterMaster, GiveTeamItems> keyValuePairs = [];
        private static void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            orig(self);
            int itemCount = self.inventory.GetItemCountEffective(CaeliImperiumContent.Items.BribeEnemiesAndBuffMinions);
            if (keyValuePairs.ContainsKey(self))
            {
                GiveTeamItems bribeEnemiesGiveTeamItems = keyValuePairs[self];
                if (itemCount <= 0)
                {
                    keyValuePairs.Remove(self);
                    GameObject.Destroy(bribeEnemiesGiveTeamItems);
                }
                else if (itemCount != bribeEnemiesGiveTeamItems.previousItemStacks)
                {
                    int itemDelta = itemCount - bribeEnemiesGiveTeamItems.previousItemStacks;
                    bool reverse = itemDelta < 0;
                    if (reverse) itemDelta *= -1;
                    for (int i = 0; i < itemDelta; i++)
                    {
                        if (reverse)
                        {
                            bribeEnemiesGiveTeamItems.RemoveLastItem();
                        }
                        else
                        {
                            bribeEnemiesGiveTeamItems.AddRandomItem();
                        }
                    }
                    bribeEnemiesGiveTeamItems.previousItemStacks = itemCount;
                }
            }
            else
            {
                if (itemCount > 0)
                {
                    GiveTeamItems bribeEnemiesGiveTeamItems = self.gameObject.AddComponent<GiveTeamItems>();
                    bribeEnemiesGiveTeamItems.characterMaster = self;
                    bribeEnemiesGiveTeamItems.teamIndex = self.teamIndex;
                    for (int i = 0; i < itemCount; i++) bribeEnemiesGiveTeamItems.AddRandomItem();
                    bribeEnemiesGiveTeamItems.previousItemStacks = itemCount;
                    keyValuePairs.Add(self, bribeEnemiesGiveTeamItems);
                }
            }

        }
        private static int Inventory_GetItemCountEffective_ItemIndex(On.RoR2.Inventory.orig_GetItemCountEffective_ItemIndex orig, Inventory self, ItemIndex itemIndex)
        {
            int itemCount = orig(self, itemIndex);
            CharacterMaster characterMaster = self.GetCharacterMaster();
            if (characterMaster) itemCount += TeamSharedInventory.GetTeamItemCountEffective(characterMaster.teamIndex, itemIndex);
            return itemCount;
        }
        private static void Hooks_OnSetBodyPrefabs()
        {
            foreach (GameObject gameObject in BodyCatalog.bodyPrefabs)
            {
                CharacterBody characterBody = gameObject.GetComponent<CharacterBody>();
                if (!characterBody) continue;
                HandleBribeEnemy(characterBody);
            }
        }
        public static void HandleBribeEnemy(CharacterBody characterBody)
        {
            IInteractable interactable = characterBody.GetComponent<IInteractable>();
            if (interactable != null) return;
            TeamComponent teamComponent = characterBody.GetComponent<TeamComponent>();
            if (!teamComponent) return;
            DeathRewards deathRewards = characterBody.GetComponent<DeathRewards>();
            if (!deathRewards) return;
            BribeEnemyInteraction bribeEnemyInteraction = characterBody.gameObject.AddComponent<BribeEnemyInteraction>();
            bribeEnemyInteraction.characterBody = characterBody;
            bribeEnemyInteraction.deathRewards = deathRewards;
            bribeEnemyInteraction.teamComponent = teamComponent;
            ModelLocator modelLocator = characterBody.gameObject.GetComponent<ModelLocator>();
            if (!modelLocator) return;
            Transform transform = modelLocator._modelTransform;
            if (!transform) return;
            List<Renderer> rendererInfos = [];
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform Child = transform.GetChild(i);
                Renderer renderer = Child.GetComponent<Renderer>();
                if (!renderer) continue;
                rendererInfos.Add(renderer);
            }
            if (rendererInfos.Count == 0) return;
            Highlight highlight1 = transform.gameObject.AddComponent<Highlight>();
            highlight1.SetTargetRendererList(rendererInfos);
            bribeEnemyInteraction.highlight = highlight1;
        }
        public static void WhitelistTypeForOnDestroyCallback(Type classType) => onDestroyCallbackTypeFilter.Add(classType);
        public static void WhitelistMethodForOnDestroyCallback(string methodName) => onDestroyCallbackMethodFilter.Add(methodName);
        public static Action<BribeReport> onBribe;
        public static void OnPluginDestroyed()
        {
            CaeliImperiumPlugin.onPluginDestroyed -= OnPluginDestroyed;
            CaeliImperiumHooks.OnSetBodyPrefabsIndividualPrefab -= HandleBribeEnemy;
        }
    }
}
