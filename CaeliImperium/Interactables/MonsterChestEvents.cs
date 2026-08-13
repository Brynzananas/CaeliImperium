using BepInEx;
using CaeliImperium.Components;
using CaeliImperium.Configs;
using CaeliImperium.Items;
using CaeliImperiumEntityStates.MonsterChest;
using Newtonsoft.Json.Utilities;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.UI;
using RoR2.UI.SkinControllers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace CaeliImperium.Interactables
{
    public static class MonsterChestEvents
    {
        public static int baseSacrificesForDefault = 2;
        public static List<string> allBossItemNames = [];
        public static List<string> baseBossItemNames = [];
        public static Dictionary<ExpansionDef, List<string>> bossItemNamesFromExpansions = [];
        public static Dictionary<string, string> nameToSHA = [];
        public static Dictionary<string, string> SHAToName = [];
        public static GameObject MonsterChestPickerPanel;
        public static GameObject MonsterChest;
        public static InteractableSpawnCard MonsterChestSpawnCard;
        private static bool inited;
        public static void Init(GameObject gameObject)
        {
            CaeliImperiumPlugin.onPluginDestroyed += CaeliImperiumPlugin_onPluginDestroyed;
            RoR2Application.onLoadFinished += OnLoadFinished;
            SceneDirector.onPostPopulateSceneServer += SceneDirector_onPostPopulateSceneServer;
            CaeliImperiumHooks.OnPickupPickerControllerOnDisplayBegin += Hooks_OnPickupPickerControllerOnDisplayBegin;
            if (inited) return;
            inited = true;
            MonsterChest = gameObject;
            MonsterChestPickerPanel = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/MonsterChest/MonsterChestPickerPanel.prefab");
            LeTai.Asset.TranslucentImage.TranslucentImage translucentImage = MonsterChestPickerPanel.GetComponent<LeTai.Asset.TranslucentImage.TranslucentImage>();
            if (translucentImage)
            {
                translucentImage.material = Addressables.LoadAssetAsync<Material>("TranslucentImage/Default-Translucent.mat").WaitForCompletion();
            }
            InspectDef MissingInspectDefFallback = Addressables.LoadAssetAsync<InspectDef>("RoR2/Base/UI/UnknownItemInspectInfo.asset").WaitForCompletion();
            InspectDef UnknownItemDefOverride = Addressables.LoadAssetAsync<InspectDef>("RoR2/Base/UI/MissingInspectInfoFallbackDef.asset").WaitForCompletion();
            UISkinData uISkinData = Addressables.LoadAssetAsync<UISkinData>("RoR2/Base/UI/skinNakedButton.asset").WaitForCompletion();
            Transform MonsterChestPickerPanelInspectPanel = MonsterChestPickerPanel.transform.Find("MainPanel/Juice/ScrapperDetailsVertical/InspectPanel");
            if (MonsterChestPickerPanelInspectPanel)
            {
                InspectPanelController inspectPanelController = MonsterChestPickerPanelInspectPanel.GetComponent<InspectPanelController>();
                if (inspectPanelController)
                { 
                    inspectPanelController.MissingInspectDefFallback = MissingInspectDefFallback;
                    inspectPanelController.UnknownItemDefOverride = UnknownItemDefOverride;
                }
            }
            Transform MonsterChestPickerPanelInspectPanel1 = MonsterChestPickerPanel.transform.Find("MainPanel/Juice/Label/InspectPanel");
            if (MonsterChestPickerPanelInspectPanel1)
            {
                InspectPanelController inspectPanelController = MonsterChestPickerPanelInspectPanel1.GetComponent<InspectPanelController>();
                if (inspectPanelController)
                {
                    inspectPanelController.MissingInspectDefFallback = MissingInspectDefFallback;
                    inspectPanelController.UnknownItemDefOverride = UnknownItemDefOverride;
                }
            }
            Transform spewButton = MonsterChestPickerPanel.transform.Find("MainPanel/Juice/SpewButton");
            if (spewButton)
            {
                ButtonSkinController buttonSkinController = spewButton.GetComponent<ButtonSkinController>();
                if (buttonSkinController)
                {
                    buttonSkinController.skinData = uISkinData;
                }
            }
            Transform cancelButtonButton = MonsterChestPickerPanel.transform.Find("MainPanel/Juice/CancelButton");
            if (cancelButtonButton)
            {
                ButtonSkinController buttonSkinController = cancelButtonButton.GetComponent<ButtonSkinController>();
                if (buttonSkinController)
                {
                    buttonSkinController.skinData = uISkinData;
                }
            }
            PickupPickerController pickupPickerController = MonsterChest.GetComponent<PickupPickerController>();
            pickupPickerController.panelPrefab = MonsterChestPickerPanel;
            MonsterChestSpawnCard = CaeliImperiumAssets.assetBundle.LoadAsset<InteractableSpawnCard>("Assets/CaeliImperium/Interactables/MonsterChest/iscMonsterChest.asset");
            typeof(Eat).RegisterEntityState();
            typeof(Spew).RegisterEntityState();
            typeof(Idle).RegisterEntityState();
        }

        private static void Hooks_OnPickupPickerControllerOnDisplayBegin(PickupPickerController arg1, NetworkUIPromptController arg2, LocalUser arg3, CameraRigController arg4)
        {
            if (!arg1.panelInstance) return;
            MonsterChestPanelHelper monsterChestPanelHelper = arg1.panelInstance.GetComponent<MonsterChestPanelHelper>();
            if (!monsterChestPanelHelper) return;
            MonsterChestController monsterChestController = arg1.GetComponent<MonsterChestController>();
            if (!monsterChestController) return;
            monsterChestPanelHelper.Init(monsterChestController);
        }
        public static void GiveItems(int itemCountForTier1, int itemCountForTier2, int itemCountForTier3)
        {
            if (PlayerCharacterMasterController.instances.Count <= 0) return;
            PlayerCharacterMasterController playerCharacterMasterController = PlayerCharacterMasterController.instances[0];
            if (!playerCharacterMasterController) return;
            CharacterMaster characterMaster = playerCharacterMasterController.master;
            if (!characterMaster) return;
            GiveItems(characterMaster.inventory, itemCountForTier1, itemCountForTier2, itemCountForTier3);
        }
        public static void GiveItems(Inventory inventory, int itemCountForTier1, int itemCountForTier2, int itemCountForTier3)
        {
            if (!inventory) return;
            for (int i = 0; i < ItemCatalog.itemCount; i++)
            {
                ItemIndex itemIndex = (ItemIndex)i;
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                if (!itemDef || itemDef == CaeliImperiumContent.Items.DrawSpeedPath || itemDef.hidden || itemDef.ContainsTag(ItemTag.WorldUnique) || itemDef.ContainsTag(ItemTag.CommandArtifactBlacklist)) continue;
                int count = 0;
                switch (itemDef.tier)
                {
                    case ItemTier.Tier1:
                        count = itemCountForTier1;
                        break;
                    case ItemTier.Tier2:
                        count = itemCountForTier2;
                        break;
                    case ItemTier.Tier3:
                        count = itemCountForTier3;
                        break;
                        default:
                        break;
                }
                if (count == 0) continue;
                inventory.GiveItemPermanent(itemDef, count);
            }
        }
        public static void SpawnMonsterChest()
        {
            DirectorCore directorCore = DirectorCore.instance;
            if (!directorCore) return;
            SceneDirector sceneDirector = directorCore.GetComponent<SceneDirector>();
            if (!sceneDirector) return;
            SpawnMonsterChest(sceneDirector);
        }
        public static void SpawnMonsterChest(SceneDirector sceneDirector)
        {
            Xoroshiro128Plus xoroshiro128Plus = new Xoroshiro128Plus(sceneDirector.rng.nextUlong);
            DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(MonsterChestSpawnCard, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Random
            }, xoroshiro128Plus));
        }
        public static void SpawnMonsterChest(int amount)
        {
            DirectorCore directorCore = DirectorCore.instance;
            if (!directorCore) return;
            SceneDirector sceneDirector = directorCore.GetComponent<SceneDirector>();
            if (!sceneDirector) return;
            SpawnMonsterChest(sceneDirector, amount);
        }
        public static void SpawnMonsterChest(SceneDirector sceneDirector, int amount)
        {
            for (int i = 0; i < amount; i++) SpawnMonsterChest(sceneDirector);
        }
        private static void SceneDirector_onPostPopulateSceneServer(SceneDirector obj)
        {
            if (!Run.instance || !SceneInfo.instance || !SceneInfo.instance.sceneDef) return;
            SceneDef sceneDef = SceneInfo.instance.sceneDef;
            InteractableSpawnRules monsterChestSpawnRules = MonsterChestConfigs.monsterChestSpawnRules;
            if (monsterChestSpawnRules == null) monsterChestSpawnRules = InteractableSpawnRules.Default;
            if (monsterChestSpawnRules.spawnRules == null) return;
            int spawnCount = 0;
            float spawmChance = 0f;
            foreach (InteractableSpawnRules.SpawnRule spawnRule in monsterChestSpawnRules.spawnRules)
            {
                SceneType[] sceneTypes = spawnRule.allowedSceneTypes;
                if (sceneTypes != null)
                {
                    bool allow = false;
                    foreach (SceneType sceneType in sceneTypes)
                    {
                        if (sceneType == sceneDef.sceneType) allow = true; break;
                    }
                    if (!allow) continue;
                }
                if (spawnRule.useStageName && !spawnRule.stageName.IsNullOrWhiteSpace())
                {
                    Scene scene = SceneManager.GetActiveScene();
                    if (scene != null && !scene.name.IsNullOrWhiteSpace() &&  scene.name == spawnRule.stageName)
                    {
                        spawnCount = spawnRule.spawnCount;
                        spawmChance = spawnRule.spawnChance;
                        break;
                    }
                }
                if (spawnRule.useStageCount)
                {
                    if (Run.instance.stageClearCountInCurrentLoop + 1 == spawnRule.stageCount)
                    {
                        spawnCount = spawnRule.spawnCount;
                        spawmChance = spawnRule.spawnChance;
                        break;
                    }
                }
            }
            if (spawnCount <= 0 || spawmChance <= 0f) return;
            for (int i = 0; i < spawnCount; i++)
            {
                if (spawmChance < 100f && !Util.CheckRoll(spawmChance)) continue;
                SpawnMonsterChest(obj);
            }
        }

        private static void CaeliImperiumPlugin_onPluginDestroyed()
        {
            CaeliImperiumPlugin.onPluginDestroyed -= CaeliImperiumPlugin_onPluginDestroyed;
            RoR2Application.onLoadFinished -= OnLoadFinished;
            SceneDirector.onPostPopulateSceneServer -= SceneDirector_onPostPopulateSceneServer;
        }
        public static string[] GetBossItemNames()
        {
            Run run = Run.instance;
            if (!run) return null;
            List<string> names = [];
            names.AddRange(baseBossItemNames);
            foreach (var pain in bossItemNamesFromExpansions)
            {
                ExpansionDef expansionDef = pain.Key;
                if (!expansionDef || !run.IsExpansionEnabled(expansionDef)) continue;
                names.AddRange(pain.Value);
            }
            return names.ToArray();
        }
        public static string[] GetBossItemNamesToSHA()
        {
            Run run = Run.instance;
            if (!run) return null;
            List<string> names = [];
            foreach (string str in baseBossItemNames)
            {
                if (!nameToSHA.TryGetValue(str, out string sha)) continue;
                names.Add(sha);
            }
            foreach (var pain in bossItemNamesFromExpansions)
            {
                ExpansionDef expansionDef = pain.Key;
                if (!expansionDef || !run.IsExpansionEnabled(expansionDef)) continue;
                foreach (string str in pain.Value)
                {
                    if (!nameToSHA.TryGetValue(str, out string sha)) continue;
                    names.Add(sha);
                }
            }
            return names.ToArray();
        }
        private static void OnLoadFinished()
        {
            foreach (ItemDef itemDef in ItemCatalog.allItemDefs)
            {
                if (!nameToSHA.ContainsKey(itemDef.name))
                {
                    string sha = itemDef.name.SHA256Encode();
                    nameToSHA.Add(itemDef.name, sha);
                    SHAToName.Add(sha, itemDef.name);
                }
                if (itemDef.tier != ItemTier.Boss || itemDef.hidden || itemDef.ContainsTag(ItemTag.WorldUnique) || itemDef.ContainsTag(ItemTag.CommandArtifactBlacklist) || itemDef.ContainsTag(CaeliImperiumAssets.CannotbeCraftedFromMonsterChest)) continue;
                allBossItemNames.Add(itemDef.name);
                ExpansionDef expansionDef = itemDef.requiredExpansion;
                if (expansionDef)
                {
                    if (bossItemNamesFromExpansions.ContainsKey(expansionDef))
                    {
                        bossItemNamesFromExpansions[expansionDef].Add(itemDef.name);
                    }
                    else
                    {
                        bossItemNamesFromExpansions.Add(expansionDef, [itemDef.name]);
                    }
                }
                else
                {
                    baseBossItemNames.Add(itemDef.name);
                }
            }
        }
    }
}
