using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Interactables;

public static class PipelineRefineryEvents
{
    public static GameObject PipelineRefinery;
    public static GameObject PipelinePoint;
    public static GameObject Pipe;
    public static InteractableSpawnCard PipelineRefinerySpawnCard;
    private static bool inited;
    public static void Init(GameObject gameObject)
    {
        if (inited) return;
        inited = true;
        PipelineRefinery = gameObject;
        PipelinePoint = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/PipelineRefinery/PipelinePoint.prefab");
        Pipe = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/PipelineRefinery/Pipe.prefab");
        PipelineRefinerySpawnCard = CaeliImperiumAssets.assetBundle.LoadAsset<InteractableSpawnCard>("Assets/CaeliImperium/Interactables/PipelineRefinery/iscPipelineRefinery.asset");
    }
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
}
