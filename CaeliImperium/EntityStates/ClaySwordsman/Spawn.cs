using CaeliImperium;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.ClaySwordsman;

public class Spawn : BaseState
{
    public static float duration = 2f;
    public static string spawnSoundString = "Play_clayBruiser_spawn";
    public static GameObject spawnEffectPrefab;
    public static string spawnEffectChildString = "Root";
    public static float startingPrintBias = 1.3f;
    public static float maxPrintBias = 0f;
    public static float printDuration = 2f;
    public static float startingPrintHeight = 0.3f;
    public static float maxPrintHeight = 0.3f;
    private static int SpawnStateHash = Animator.StringToHash("Spawn");
    private static int SpawnParamHash = Animator.StringToHash("Spawn.playbackRate");
    public override void OnEnter()
    {
        base.OnEnter();
        EffectManager.SimpleMuzzleFlash(EntityStates.ClayBruiserMonster.SpawnState.spawnEffectPrefab, base.gameObject, spawnEffectChildString, false);
        Util.PlaySound(EntityStates.ClayBruiserMonster.SpawnState.spawnSoundString, base.gameObject);
        base.PlayAnimation("Body", SpawnStateHash, SpawnParamHash, duration);
        Transform modelTransform = GetModelTransform();
        if (!modelTransform) return;
        PrintController printController = modelTransform.GetOrAddComponent<PrintController>();
        printController.printTime = printDuration;
        printController.enabled = true;
        printController.startingPrintHeight = startingPrintHeight;
        printController.maxPrintHeight = maxPrintHeight;
        printController.startingPrintBias = startingPrintBias;
        printController.maxPrintBias = maxPrintBias;
        printController.disableWhenFinished = true;
        printController.printCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isAuthority || fixedAge < duration) return;
        outer.SetNextStateToMain();
    }
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Death;
}
