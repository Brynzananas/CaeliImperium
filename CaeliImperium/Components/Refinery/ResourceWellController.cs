using CaeliImperium.ScriptableObjects;
using JetBrains.Annotations;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components.Refinery;
public class ResourceWellController : NetworkBehaviour, IInteractable
{
    public static List<ResourceWellController> instances = [];
    public VoiceMessageDef[] voiceMessages = [];
    public float delayBeforePlayingVoiceMessage = 0.6f;
    public GameObject extractorPrefab;
    public GameObject objectToDisable;
    [HideInInspector] public GameObject extractorInstance;
    public bool available = true;
    public string contextToken;
    public bool shouldIgnoreSpherecastForInteractibility;
    public bool shouldProximityHighlight;
    public bool shouldShowOnScanner;
    public void OnEnable()
    {
        instances.Add(this);
    }
    public void OnDisable()
    {
        instances.Remove(this);
    }
    public string GetContextString([NotNull] Interactor activator) => Language.GetString(contextToken);
    public Interactability GetInteractability([NotNull] Interactor activator) => available ? Interactability.Available : Interactability.Disabled;
    public void OnInteractionBegin([NotNull] Interactor activator)
    {
        if (!available) return;
        /*bool dontPlay = false;
        foreach (ResourceWellController resourceWellController in instances)
        {
            if (!resourceWellController || resourceWellController.available) continue;
            dontPlay = true;
        }
        if (!dontPlay) Invoke("CallPlayVoiceMessage", delayBeforePlayingVoiceMessage);*/
        CallSpawnExtractor();
    }
    public void CallPlayVoiceMessage()
    {
        if (!NetworkServer.active)
        {
            CmdPlayVoiceMessage();
            return;
        }
        if (voiceMessages == null || voiceMessages.Length == 0) return;
        int index = UnityEngine.Random.Range(0, voiceMessages.Length);
        RpcPlayVoiceMessage(index);
    }
    [Command]
    public void CmdPlayVoiceMessage() => CallPlayVoiceMessage();
    [ClientRpc]
    public void RpcPlayVoiceMessage(int index)
    {
        //if (CaeliImperiumUtils.HasCompletedPipelineRefinery()) return;
        if (voiceMessages == null || voiceMessages.Length == 0) return;
        if (index < 0 || index >= voiceMessages.Length) return;
        VoiceMessageDef voiceMessageDef = voiceMessages[index];
        if (!voiceMessageDef) return;
        voiceMessageDef.Play();
    }
    public void CallSpawnExtractor()
    {
        if (NetworkServer.active)
        {
            SpawnExtractor();
            RpcSpawnExtractor();
        }
        else
        {
            CmdSpawnExtractor();
        }
    }
    [Command]
    public void CmdSpawnExtractor() => CallSpawnExtractor();
    [ClientRpc]
    public void RpcSpawnExtractor()
    {
        if (NetworkServer.active) return;
        SpawnExtractor();
    }
    public void SpawnExtractor()
    {
        if (!available) return;
        available = false;
        Util.PlaySound("Play_DRG_WellExtractor_Impact", gameObject);
        if (objectToDisable) objectToDisable.SetActive(false);
        if (!NetworkServer.active) return;
        extractorInstance = Instantiate(extractorPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(extractorInstance);
    }
    public bool ShouldIgnoreSpherecastForInteractibility([NotNull] Interactor activator) => shouldIgnoreSpherecastForInteractibility;
    public bool ShouldProximityHighlight() => shouldProximityHighlight;
    public bool ShouldShowOnScanner() => available && shouldShowOnScanner;
}
