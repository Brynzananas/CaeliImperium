using CaeliImperium.Configs;
using JetBrains.Annotations;
using R2API;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.UIElements.ListViewDragger;

namespace CaeliImperium.Components.Refinery;

public class PipelineRefineryController : NetworkBehaviour, IInteractable
{
    public static int runningCount;
    public Animator animator;
    public GameObject soundCenter;
    public CombatDirector combatDirector;
    public GameObject pipelineBuilderPrefab;
    public Transform[] transformsForPipelineBuilders;
    public float timeToComplete = 5f;
    public int neededCompletedBuilders = 3;
    public int sabotages = 3;
    public Transform dropPosition;
    public int placeholderItemsToGivePerCompletedBuilder = 1;
    public PickupDropTable placeholderDropTable;
    [SyncVar] public int currentlyCompletedBuilders;
    public bool buildersAreComplete => currentlyCompletedBuilders >= neededCompletedBuilders;
    public string completedContextString;
    public string incompletedContextString;
    public bool completed;
    public bool running;
    public float stopwatch;
    public bool shouldIgnoreSpherecastForInteractibility;
    public bool shouldProximityHighlight;
    public bool shouldShowOnScanner;
    public List<WellExtractorController> wellExtractorControllers = [];
    private PipelineBuilder[] pipelineBuilders;
    [SyncVar] public int sabotagesCompleted;
    [SyncVar] public float nextSabotage;
    [SyncVar] public bool underSabotage;
    [SyncVar] public int currentlySabotagedPipes;
    [SyncVar] public float runningPercentage;
    private bool oneTimeStarted;
    private bool objectiveAdded;
    private bool directorHookAdded;
    private bool addedRunningCount;
    public void Start()
    {
        AddObjective();
        if (!NetworkServer.active || !pipelineBuilderPrefab || transformsForPipelineBuilders == null) return;
        pipelineBuilders = new PipelineBuilder[transformsForPipelineBuilders.Length];
        for (int i = 0; i < transformsForPipelineBuilders.Length; i++)
        {
            Transform transform = transformsForPipelineBuilders[i];
            GameObject gameObject = Instantiate(pipelineBuilderPrefab, transform.position, transform.rotation);
            PipelineBuilder pipelineBuilder = gameObject.GetComponent<PipelineBuilder>();
            if (!pipelineBuilder) continue;
            pipelineBuilder.pipelineRefineryControllerNetworkInstanceId = netId;
            pipelineBuilders[i] = pipelineBuilder;
            NetworkServer.Spawn(gameObject);
        }
    }
    public void OnDestroy()
    {
        RemoveObjective();
        RemoveRunningCount();
        if (!NetworkServer.active) return;
        RemoveDirectorHook();
    }
    public void AddObjective()
    {
        if (objectiveAdded) return;
        objectiveAdded = true;
        ObjectivePanelController.collectObjectiveSources += ObjectivePanelController_collectObjectiveSources;
    }
    public void RemoveObjective()
    {
        if (!objectiveAdded) return;
        objectiveAdded = false;
        ObjectivePanelController.collectObjectiveSources -= ObjectivePanelController_collectObjectiveSources;
    }
    public void AddDirectorHook()
    {
        if (directorHookAdded) return;
        directorHookAdded = true;
        R2API.DirectorAPI.GetCombatDirectorActivityCount += DirectorAPI_GetCombatDirectorActivityCount;
    }
    public void RemoveDirectorHook()
    {
        if (!directorHookAdded) return;
        directorHookAdded = false;
        R2API.DirectorAPI.GetCombatDirectorActivityCount -= DirectorAPI_GetCombatDirectorActivityCount;
    }
    public void AddRunningCount()
    {
        if (addedRunningCount) return;
        addedRunningCount = true;
        runningCount++;
    }
    public void RemoveRunningCount()
    {
        if (!addedRunningCount) return;
        addedRunningCount = false;
        runningCount--;
    }
    private void ObjectivePanelController_collectObjectiveSources(CharacterMaster arg1, List<ObjectivePanelController.ObjectiveSourceDescriptor> arg2)
    {
        ObjectivePanelController.ObjectiveSourceDescriptor objectiveSourceDescriptor = new ObjectivePanelController.ObjectiveSourceDescriptor
        {
            master = arg1,
            objectiveType = typeof(PipelineRefineryObjective),
            source = this
        };
        arg2.Add(objectiveSourceDescriptor);
    }
    public string GetContextString([NotNull] Interactor activator) => buildersAreComplete ? Language.GetString(completedContextString) : Language.GetString(incompletedContextString);
    public void OnInteractionBegin([NotNull] Interactor activator)
    {
        if (completed || running || !buildersAreComplete) return;
        DisableBuilders();
        CallStartPumping();
    }
    public void CallStartPumping()
    {
        if (NetworkServer.active)
        {
            StartPumping();
            RpcStartPumping();
        }
        else
        {
            CmdStartPumping();
        }
    }
    [Command]
    public void CmdStartPumping() => CallStartPumping();
    [ClientRpc]
    public void RpcStartPumping()
    {
        if (NetworkServer.active) return;
        StartPumping();
    }
    public void StartPumping()
    {
        running = true;
        if (sabotages <= 0)
        {
            nextSabotage = float.MaxValue;
        }
        else
        {
            float timeForOneSabotage = timeToComplete / sabotages;
            nextSabotage = timeForOneSabotage * ((float)sabotagesCompleted + 0.9f);
        }
        if (animator) animator.Play("StartUp", 0);
        PlayAnimationForWellExtractors("StartUp");
        if (soundCenter) Util.PlaySound("Play_DRG_Refinery_Loop", soundCenter);
        OneTimeStartPumping();
    }
    public void OneTimeStartPumping()
    {
        if (oneTimeStarted) return;
        oneTimeStarted = true;
        AddRunningCount();
        if (!NetworkServer.active) return;
        AddDirectorHook();
        if (PipelineRefineryConfigs.PipelineRefineryEnableExtraCombatDirector != null && PipelineRefineryConfigs.PipelineRefineryEnableExtraCombatDirector.Value && combatDirector && !combatDirector.enabled) combatDirector.enabled = true;
        if (DirectorCore.instance)
        {
            CombatDirector[] components = DirectorCore.instance.GetComponents<CombatDirector>();
            if (components.Length != 0)
            {
                CombatDirector[] array = components;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].enabled = true;
                }
            }
        }
    }
    private void DirectorAPI_GetCombatDirectorActivityCount(CombatDirector combatDirector, ref int activityCount)
    {
        if (combatDirector.IsStageCombatDirector() && PipelineRefineryConfigs.PipelineRefineryStageCombatDirectorsBehaviour != null)
        {
            switch (PipelineRefineryConfigs.PipelineRefineryStageCombatDirectorsBehaviour.Value)
            {
                case StageCombatDirectorsBehaviour.AlwaysEnabled:
                    activityCount += 2;
                    break;
                case StageCombatDirectorsBehaviour.AlwaysDisabled:
                    activityCount -= 2;
                    break;
                default:
                    break;
            }
        }
    }
    public void PlayAnimationForWellExtractors(string animationName)
    {
        foreach (WellExtractorController wellExtractorController in wellExtractorControllers)
        {
            if (!wellExtractorController) continue;
            Animator animator = wellExtractorController.animator;
            Util.PlaySound("Play_DRG_WellExtractor_Loop", wellExtractorController.gameObject);
            if (!animator) continue;
            animator.Play(animationName, 0);
        }
    }
    public void StopAnimationForWellExtractors()
    {
        foreach (WellExtractorController wellExtractorController in wellExtractorControllers)
        {
            if (!wellExtractorController) continue;
            Animator animator = wellExtractorController.animator;
            Util.PlaySound("Stop_DRG_WellExtractor_Loop", wellExtractorController.gameObject);
            if (!animator) continue;
            animator.Play("BufferEmpty", 0);
        }
    }
    public void CallStopPumping()
    {
        if (NetworkServer.active)
        {
            StopPumping();
            RpcStopPumping();
        }
        else
        {
            CmdStopPumping();
        }
    }
    [Command]
    public void CmdStopPumping() => CallStopPumping();
    [ClientRpc]
    public void RpcStopPumping()
    {
        if (NetworkServer.active) return;
        StopPumping();
    }
    public void StopPumping()
    {
        running = false;
        if (animator) animator.Play("Stop", 0);
        StopAnimationForWellExtractors();
        if (soundCenter) Util.PlaySound("Stop_DRG_Refinery_Loop", soundCenter);
    }
    public void CallCompletePumping()
    {
        if (NetworkServer.active)
        {
            CompletePumping();
            RpcCompletePumping();
        }
        else
        {
            CmdCompletePumping();
        }
    }
    [Command]
    public void CmdCompletePumping() => CallCompletePumping();
    [ClientRpc]
    public void RpcCompletePumping()
    {
        if (NetworkServer.active) return;
        CompletePumping();
    }
    /*public void SaveCount()
    {
        PlayerCharacterMasterController playerCharacterMasterController = CaeliImperiumUtils.GetPlayerCharacterMasterController();
        if (!playerCharacterMasterController) return;
        NetworkUser networkUser = playerCharacterMasterController.networkUser;
        if (!networkUser) return;
        LocalUser localUser = networkUser.localUser;
        if (localUser == null) return;
        UserProfile userProfile = localUser.userProfile;
        if (userProfile == null) return;
        CaeliImperiumSave caeliImperiumSave = userProfile.GetCaeliImperiumSave();
        if (caeliImperiumSave == null) return;
        caeliImperiumSave.pipelineRefineriesCompletedCount++;
    }*/
    public void CompletePumping()
    {
        running = false;
        completed = true;
        if (animator) animator.Play("Launch", 0);
        if (soundCenter) Util.PlaySound("Stop_DRG_Refinery_Loop", soundCenter);
        StopAnimationForWellExtractors();
        RemoveRunningCount();
        RemoveObjective();
        //SaveCount();
        if (!NetworkServer.active) return;
        RemoveDirectorHook();
        if (combatDirector && combatDirector.enabled) combatDirector.enabled = false;
        PlaceholderGiveItems();
    }
    public void PlaceholderGiveItems()
    {
        Run run = Run.instance;
        if (!run) return;
        Xoroshiro128Plus rng = run.treasureRng;
        if (rng == null) return;
        int participatingPlayerCount = Run.instance.participatingPlayerCount;
        if (participatingPlayerCount == 0) return;
        if (!dropPosition) return;
        UniquePickup pickup = UniquePickup.none;
        RerollRewardItem(ref pickup, rng);
        int num = currentlyCompletedBuilders * placeholderItemsToGivePerCompletedBuilder;
        if (PipelineRefineryConfigs.PipelineRefineryMultiplyItemsToGiveByPlayerCount != null && PipelineRefineryConfigs.PipelineRefineryMultiplyItemsToGiveByPlayerCount.Value) num *= participatingPlayerCount;
        float angle = 360f / num;
        Vector3 vector = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
        Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
        int num2 = 0;
        while (num2 < num)
        {
            UniquePickup pickup2 = pickup;
            PickupDropletController.CreatePickupDroplet(pickup2, dropPosition.position, vector, false);
            if (PipelineRefineryConfigs.PipelineRefineryRerollEachItem != null && PipelineRefineryConfigs.PipelineRefineryRerollEachItem.Value) RerollRewardItem(ref pickup2, rng);
            num2++;
            vector = quaternion * vector;
        }
    }
    private void RerollRewardItem(ref UniquePickup pickup, Xoroshiro128Plus rng)
    {
        if (placeholderDropTable)
        {
            pickup = placeholderDropTable.GeneratePickup(rng);
        }
        else
        {
            List<PickupIndex> list = Run.instance.availableTier2DropList;
            pickup = new UniquePickup(rng.NextElementUniform(list));
        }
    }
    public void DisableBuilders()
    {
        if (!NetworkServer.active || pipelineBuilders == null) return;
        foreach (PipelineBuilder pipelineBuilder in pipelineBuilders)
        {
            if (!pipelineBuilder || !pipelineBuilder.canBeInteracted) continue;
            pipelineBuilder.canBeInteracted = false;
        }
    }
    public void Sabotage()
    {
        if (!NetworkServer.active) return;
        underSabotage = true;
        currentlySabotagedPipes = 0;
        int neededSabotagedPipes = currentlyCompletedBuilders;
        for (int i = 0, j = 0; i < neededSabotagedPipes; i++, j++)
        {
            if (j < 0) continue;
            if (j >= pipelineBuilders.Length) j = 0;
            PipelineBuilder pipelineBuilder = pipelineBuilders[j];
            if (!pipelineBuilder || !pipelineBuilder.completed) continue;
            PipelineInteractor pipelineInteractor = pipelineBuilder.pipelineInteractors[UnityEngine.Random.Range(0, pipelineBuilder.pipelineInteractors.Count)];
            if (!pipelineInteractor || pipelineInteractor.sabotaged) continue;
            pipelineInteractor.CallSabotage();
            currentlySabotagedPipes++;
        }
        CallStopPumping();
        if (currentlySabotagedPipes <= 0) UnSabotage();
    }
    public void UnSabotage()
    {
        if (!NetworkServer.active) return;
        currentlySabotagedPipes--;
        if (currentlySabotagedPipes <= 0)
        {
            sabotagesCompleted++;
            underSabotage = false;
            CallStartPumping();
        }
    }
    public void FixedUpdate()
    {
        if (!NetworkServer.active || completed || !running) return;
        stopwatch += Time.fixedDeltaTime;
        runningPercentage = stopwatch / timeToComplete * 100f;
        if (stopwatch >= nextSabotage)
        {
            Sabotage();
            return;
        }
        if (stopwatch >= timeToComplete)
        {
            CompletePumping();
            RpcCompletePumping();
        }
    }
    public Interactability GetInteractability([NotNull] Interactor activator)
    {
        if (running || completed || underSabotage) return Interactability.Disabled;
        return buildersAreComplete ? Interactability.Available : Interactability.ConditionsNotMet;
    }
    public bool ShouldIgnoreSpherecastForInteractibility([NotNull] Interactor activator) => shouldIgnoreSpherecastForInteractibility;
    public bool ShouldProximityHighlight() => shouldProximityHighlight;
    public bool ShouldShowOnScanner() => completed && shouldShowOnScanner;
    public enum StageCombatDirectorsBehaviour
    {
        DoNotModify,
        AlwaysEnabled,
        AlwaysDisabled
    }
}
