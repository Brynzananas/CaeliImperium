using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components;

public class PipelineInteractor : MonoBehaviour, IInteractable
{
    public static float baseDuration = 1f;
    public float duration;
    public PipelineBuilder pipelineBuilder;
    public Transform start;
    public string conextToken;
    public bool shouldIgnoreSpherecastForInteractibility;
    public bool shouldProximityHighlight;
    public bool shouldShowOnScanner;
    public string GetContextString([NotNull] Interactor activator) => Language.GetString(conextToken);

    public Interactability GetInteractability([NotNull] Interactor activator)
    {
        if (duration > 0f) return Interactability.Disabled;
        if (pipelineBuilder)
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
        duration = baseDuration;
    }
    public void FixedUpdate()
    {
        if (duration > 0f) duration -= Time.fixedDeltaTime;
    }
    public void OnInteractionBegin([NotNull] Interactor activator)
    {
        if (pipelineBuilder) pipelineBuilder.OnInteract(activator);
    }
    public bool ShouldIgnoreSpherecastForInteractibility([NotNull] Interactor activator) => shouldIgnoreSpherecastForInteractibility;
    public bool ShouldProximityHighlight() => shouldProximityHighlight;
    public bool ShouldShowOnScanner() => shouldShowOnScanner;
}
