using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components.Refinery;

public class PipelineInteractor : NetworkBehaviour, IInteractable
{
    public static float baseDuration = 0.2f;
    public float duration;
    public float monsterCredits;
    [SyncVar] public NetworkInstanceId pipelineBuilderNetworkInstanceId;
    private PipelineBuilder _pipelineBuilder;
    public PipelineBuilder pipelineBuilder
    {
        get
        {
            if (!_pipelineBuilder)
            {
                GameObject gameObject = Util.FindNetworkObject(pipelineBuilderNetworkInstanceId);
                if (gameObject)
                {
                    _pipelineBuilder = gameObject.GetComponent<PipelineBuilder>();
                }
            }
            return _pipelineBuilder;
        }
    }
    public Transform start;
    public GameObject sabotageIndicator;
    public Vector3 startOffset;
    public string contextToken;
    public string underSabotageContextToken;
    public bool shouldIgnoreSpherecastForInteractibility;
    public bool shouldProximityHighlight;
    public bool shouldShowOnScanner;
    public bool sabotaged;
    [SyncVar] public BezierSegmentData bezierSegmentData;
    private bool started;
    public string GetContextString([NotNull] Interactor activator) => sabotaged ? Language.GetString(underSabotageContextToken) : Language.GetString(contextToken);

    public Interactability GetInteractability([NotNull] Interactor activator)
    {
        if (duration > 0f) return Interactability.Disabled;
        if (sabotaged) return Interactability.Available;
        if (pipelineBuilder && pipelineBuilder.canBeInteracted)
        {
            if (pipelineBuilder.currentSourceNode != transform) return Interactability.Disabled;
            if (pipelineBuilder.currentInteractor)
            {
                return Interactability.ConditionsNotMet;
            }
            else
            {
                return Interactability.Available;
            }
        }
        else
        {
            return Interactability.Disabled;
        }
    }
    public void Start()
    {
        if (started) return;
        Util.PlaySound("Play_DRG_Pipe_Build", gameObject);
        duration = baseDuration;
        started = true;
        if (pipelineBuilder)
        {
            pipelineBuilder.currentSourceNode = transform;
            pipelineBuilder.pipelineInteractors.Add(this);
            pipelineBuilder.RebuildZiprail();
        }
    }
    [Command]
    public void CmdSabotage() => CallSabotage();
    public void CallSabotage()
    {
        if (NetworkServer.active)
        {
            Sabotage();
            RpcStabotage();
        }
        else
        {
            CmdSabotage();
        }
    }
    [ClientRpc]
    public void RpcStabotage()
    {
        if (NetworkServer.active) return;
        Sabotage();
    }
    public void Sabotage()
    {
        sabotaged = true;
        if (sabotageIndicator) sabotageIndicator.SetActive(true);
        if (monsterCredits <= 0) return;
        if (!NetworkServer.active) return;
        DirectorCore directorCore = DirectorCore.instance;
        if (!directorCore) return;
        CombatDirector combatDirector = directorCore.GetComponent<CombatDirector>();
        if (!combatDirector) return;
        float previousDirectorCredits = combatDirector.monsterCredit;
        combatDirector.monsterCredit = monsterCredits;
        DirectorCard directorCard = combatDirector.currentMonsterCard;
        combatDirector.currentMonsterCard = null;
        if (combatDirector.AttemptSpawnOnTarget(transform, DirectorPlacementRule.PlacementMode.Approximate))
        {
            if (combatDirector.shouldSpawnOneWave) combatDirector.hasStartedWave = true;
        }
        combatDirector.currentMonsterCard = directorCard;
        combatDirector.monsterCredit = previousDirectorCredits;
    }
    [Command]
    public void CmdUnSabotage() => CallUnSabotage();
    public void CallUnSabotage()
    {
        if (NetworkServer.active)
        {
            UnSabotage();
            RpcUnStabotage();
        }
        else
        {
            CmdUnSabotage();
        }
    }
    [ClientRpc]
    public void RpcUnStabotage()
    {
        if (NetworkServer.active) return;
        UnSabotage();
    }
    public void UnSabotage()
    {
        sabotaged = false;
        if (sabotageIndicator) sabotageIndicator.SetActive(false);
        if (pipelineBuilder && pipelineBuilder.pipelineRefineryController) pipelineBuilder.pipelineRefineryController.UnSabotage();
    }
    public void FixedUpdate()
    {
        if (duration > 0f) duration -= Time.fixedDeltaTime;
    }
    public void OnInteractionBegin([NotNull] Interactor activator)
    {
        if (sabotaged)
        {
            CallUnSabotage();
            return;
        }
        if (pipelineBuilder) pipelineBuilder.OnInteract(activator);
    }
    public bool ShouldIgnoreSpherecastForInteractibility([NotNull] Interactor activator) => shouldIgnoreSpherecastForInteractibility;
    public bool ShouldProximityHighlight() => shouldProximityHighlight;
    public bool ShouldShowOnScanner() => shouldShowOnScanner;
}
