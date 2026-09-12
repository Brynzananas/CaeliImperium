using CaeliImperium.Components.Refinery;
using CaeliImperium.Configs;
using CaeliImperium.NetworkMessages;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static BrynzaAPI.BrynzaAPI;

namespace CaeliImperium.Interactables;

public static class PipelineRefineryEvents
{
    public static int wellCount = 3;
    public static GameObject PipelineRefinery;
    public static GameObject PipelinePoint;
    public static GameObject Pipe;
    public static GameObject ResourceWell;
    public static GameObject WellExtractor;
    public static GameObject PipelineBuilder;
    public static GameObject PipelineZiprailVehicle;
    public static InteractableSpawnCard PipelineRefinerySpawnCard;
    public static InteractableSpawnCard ResourceWellSpawnCard;
    public static InteractableSpawnRules DefaultSpawnRules = new InteractableSpawnRules
    {
        spawnRules = new InteractableSpawnRules.SpawnRule[] {new InteractableSpawnRules.SpawnRule
        {
            useStageCount = true,
                        stageCount = 1,
                        allowedSceneTypes = new SceneType[] {
                            SceneType.Stage
                        },
                        spawnChance = 100f,
                        spawnCount = 1,
        }
        }
    };
    private static bool inited;
    public static void Init(GameObject gameObject)
    {
        SceneDirector.onPostPopulateSceneServer += SceneDirector_onPostPopulateSceneServer;
        CaeliImperiumPlugin.onPluginDestroyed += CaeliImperiumPlugin_onPluginDestroyed;
        IL.RoR2.MusicController.PickCurrentTrack += MusicController_PickCurrentTrack;
        IL.RoR2.UI.ContextManager.Update += ContextManager_Update;
        if (inited) return;
        inited = true;
        List<Transform> children = CaeliImperiumUtils.GetFilteredChildren(gameObject.transform, FilterColliders);
        foreach (Transform child in children)
        {
            SurfaceDefProvider surfaceDefProvider = child.GetComponent<SurfaceDefProvider>();
            if (!surfaceDefProvider) continue;
            surfaceDefProvider.surfaceDef = CaeliImperiumAssets.MetalSurface;
        }
        PipelinePoint = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/PipelineRefinery/PipeConnector/PipelinePoint.prefab").RegisterNetworkPrefab();
        Pipe = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/PipelineRefinery/Pipe.prefab");
        ResourceWell = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/PipelineRefinery/ResourceWell/ResourceWell.prefab").RegisterNetworkPrefab();
        WellExtractor = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/PipelineRefinery/WellExtractor/WellExtractor.prefab").RegisterNetworkPrefab();
        PipelineBuilder = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/PipelineRefinery/PipelineBuilder.prefab").RegisterNetworkPrefab();
        ZiprailController ziprailController = PipelineBuilder.GetComponent<ZiprailController>();
        if (ziprailController)
        {
            PipelineZiprailVehicle = ziprailController._ziprailVehiclePrefab.gameObject.RegisterNetworkPrefab();
            //ziprailController._ziprailVehiclePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/ZiprailVehicle.prefab").WaitForCompletion().GetComponent<ZiprailVehicle>();
            ziprailController._vehicleVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/ZipRail_VehicleVFX_VFX.prefab").WaitForCompletion();
        }
        PipelineRefinerySpawnCard = CaeliImperiumAssets.assetBundle.LoadAsset<InteractableSpawnCard>("Assets/CaeliImperium/Interactables/PipelineRefinery/Refinery/iscPipelineRefinery.asset");
        ResourceWellSpawnCard = CaeliImperiumAssets.assetBundle.LoadAsset<InteractableSpawnCard>("Assets/CaeliImperium/Interactables/PipelineRefinery/ResourceWell/iscResourceWell.asset");
        R2API.Networking.NetworkingAPI.RegisterMessageType<PipelineBuilderDisablePreviewNetMessage>();
        R2API.Networking.NetworkingAPI.RegisterMessageType<PipelineBuilderPlaceNodeNetMessage>();
        R2API.Networking.NetworkingAPI.RegisterMessageType<PipelineBuilderPlacePreviewNetMessage>();
        R2API.Networking.NetworkingAPI.RegisterMessageType<PipelineBuilderRemoveCurrentInteractorNetMessage>();
        R2API.Networking.NetworkingAPI.RegisterMessageType<WellExtractorControllerOnPipeConnectedNetMessage>();
    }

    private static void ContextManager_Update(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        if (!c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<ContextManager>(nameof(ContextManager.glyphTMP))
            ))
        {
            CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 1 failed!");
            return;
        }
        Instruction instruction = c.Next;
        c = new ILCursor(il);
        if (!c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<ContextManager>(nameof(ContextManager.hud)),
                x => x.MatchCallvirt(typeof(HUD).GetPropertyGetter(nameof(HUD.targetBodyObject))),
                x => x.MatchCallvirt<GameObject>(nameof(GameObject.GetComponent))
            ))
        {
            CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook 2 failed!");
            return;
        }
        int valuesId = il.Body.Variables.Count;
        c.Body.Variables.Add(new Mono.Cecil.Cil.VariableDefinition(il.Import(typeof(ContextManager_Update_Values))));
        Instruction instruction2 = c.Next;
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate(OverrideString);
        c.Emit(OpCodes.Stloc, valuesId);
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ContextManager_Update_Values), nameof(ContextManager_Update_Values.doOverride)));
        c.Emit(OpCodes.Brfalse_S, instruction2);
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ContextManager_Update_Values), nameof(ContextManager_Update_Values.glyphText)));
        c.Emit(OpCodes.Stloc, 0); // bad!!!
        c.Emit(OpCodes.Ldloc, valuesId);
        c.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(ContextManager_Update_Values), nameof(ContextManager_Update_Values.descriptionText)));
        c.Emit(OpCodes.Stloc, 2); // bad!!!
        c.Emit(OpCodes.Ldc_I4, 1);
        c.Emit(OpCodes.Stloc, 4); // bad!!!
        c.Emit(OpCodes.Br_S, instruction);
    }
    public struct ContextManager_Update_Values
    {
        public bool doOverride;
        public string glyphText;
        public string descriptionText;
    }
    private static ContextManager_Update_Values OverrideString(ContextManager contextManager)
    {
        ContextManager_Update_Values contextManager_Update_Values = new ContextManager_Update_Values();
        HUD hUD = contextManager.hud;
        if (!hUD) return contextManager_Update_Values;
        GameObject gameObject = hUD.targetBodyObject;
        if (!gameObject) return contextManager_Update_Values;
        foreach (CaeliImperium.Components.Refinery.PipelineBuilder pipelineBuilder in CaeliImperium.Components.Refinery.PipelineBuilder.instances)
        {
            if (!pipelineBuilder || !pipelineBuilder.canBeInteracted || !pipelineBuilder.currentInteractor) continue;
            if (pipelineBuilder.currentInteractor.gameObject == gameObject)
            {
                contextManager_Update_Values.doOverride = true;
                contextManager_Update_Values.glyphText = string.Format(CultureInfo.InvariantCulture, "<style=cKeyBinding>{0}</style>", Glyphs.GetGlyphString(contextManager.eventSystemLocator, "Interact"));
                contextManager_Update_Values.descriptionText = pipelineBuilder.cancelBuilding ? Language.GetString("CI_PIPELINE_CANCEL_CONTEXT") : Language.GetString("CI_PIPELINE_BUILD_CONTEXT");
                return contextManager_Update_Values;
            }
        }
        return contextManager_Update_Values;
    }
    private static bool FilterColliders(Transform child)
    {
        if (child.name.StartsWith("Collider")) return true;
        return false;
    }
    private static void MusicController_PickCurrentTrack(MonoMod.Cil.ILContext il)
    {
        ILCursor c = new ILCursor(il);
        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchStloc(1) // TODO: Make this good
            ))
        {
            CaeliImperiumPlugin.Log.LogError(il.Method.Name + " IL Hook failed!");
            return;
        }
        Instruction instruction = c.Next;
        c.Emit(OpCodes.Ldsfld, AccessTools.Field(typeof(PipelineRefineryController), nameof(PipelineRefineryController.runningCount)));
        c.Emit(OpCodes.Ldc_I4_0);
        c.Emit(OpCodes.Ble_S, instruction);
        c.Emit(OpCodes.Ldc_I4_1);
        c.Emit(OpCodes.Stloc_1);
    }
    private static void CaeliImperiumPlugin_onPluginDestroyed()
    {
        SceneDirector.onPostPopulateSceneServer -= SceneDirector_onPostPopulateSceneServer;
        CaeliImperiumPlugin.onPluginDestroyed -= CaeliImperiumPlugin_onPluginDestroyed;
        IL.RoR2.MusicController.PickCurrentTrack -= MusicController_PickCurrentTrack;
        IL.RoR2.UI.ContextManager.Update -= ContextManager_Update;
    }
    private static void SceneDirector_onPostPopulateSceneServer(SceneDirector obj) => CaeliImperiumUtils.SimulateInteractableSpawnUsingSpawnRules(obj, PipelineRefineryConfigs.pipelineRefinerySpawnRules, PipelineRefineryEvents.DefaultSpawnRules, SpawnPipelineRefinery, SpawnResourceWells);
    public static void SpawnPipelineRefinery()
    {
        if (PlayerCharacterMasterController.instances.Count <= 0) return;
        PlayerCharacterMasterController playerCharacterMasterController = PlayerCharacterMasterController.instances[0];
        if (!playerCharacterMasterController) return;
        CharacterMaster characterMaster = playerCharacterMasterController.master;
        if (!characterMaster) return;
        CharacterBody characterBody = characterMaster.GetBody();
        if (!characterBody) return;
        DirectorCore directorCore = DirectorCore.instance;
        if (!directorCore) return;
        SceneDirector sceneDirector = directorCore.GetComponent<SceneDirector>();
        if (!sceneDirector) return;
        Xoroshiro128Plus xoroshiro128Plus = new Xoroshiro128Plus(sceneDirector.rng.nextUlong);
        DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(PipelineRefinerySpawnCard, new DirectorPlacementRule
        {
            placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
            position = characterBody.transform.position,
        }, xoroshiro128Plus));
    }
    public static void SpawnPipelineRefinery(SceneDirector sceneDirector)
    {
        Xoroshiro128Plus xoroshiro128Plus = new Xoroshiro128Plus(sceneDirector.rng.nextUlong);
        DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(PipelineRefinerySpawnCard, new DirectorPlacementRule
        {
            placementMode = DirectorPlacementRule.PlacementMode.Random,
        }, xoroshiro128Plus));
    }
    public static void SpawnResourceWells(SceneDirector sceneDirector, int count)
    {
        Xoroshiro128Plus xoroshiro128Plus = new Xoroshiro128Plus(sceneDirector.rng.nextUlong);
        int spawnCount = count;
        if (PipelineRefineryConfigs.ResourceWellSpawnCount != null) spawnCount *= PipelineRefineryConfigs.ResourceWellSpawnCount.Value;
        for (int i = 0; i < spawnCount; i++)
        {
            DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(ResourceWellSpawnCard, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Random,
            }, xoroshiro128Plus));
        }
    }
}
