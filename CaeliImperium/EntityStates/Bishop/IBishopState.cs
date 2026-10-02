using CaeliImperium;
using CaeliImperium.Components;
using CaeliImperium.Components.Bishop;
using CaeliImperium.ScriptableObjects;
using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.Bishop;
public interface IBishopState
{
    public FirstPersonCameraController firstPersonCameraController => FirstPersonCameraController.instance;
    public Transform muzzle // TODO
    {
        get
        {
            if (this is not BaseState baseState) return null;
            CharacterBody characterBody = baseState.characterBody;
            if (characterBody)
            {
                if (characterBody.aimOriginTransform)
                {
                    return characterBody.aimOriginTransform;
                }
                else
                {
                    return characterBody.transform;
                }
            }
            else
            {
                return baseState.transform;
            }
        }
    }
    public BishopComponent bishopComponent
    {
        get
        {
            if (this is not BaseState baseState) return null;
            return baseState.GetComponent<BishopComponent>();
        }
    }
    public virtual Animator GetFirstPersonAnimator()
    {
        if (!firstPersonCameraController) return null;
        return firstPersonCameraController.animator;
    }
    public virtual void PlayFirstPersonCrossfade(string layerName, string animationStateName, string playbackRateParam, float duration, float crossfadeDuration, bool durationIsMultiplier)
    {
        if (!Condition()) return;
        firstPersonCameraController.animator.PlayCrossfade(layerName, animationStateName, playbackRateParam, duration, crossfadeDuration, durationIsMultiplier);
    }
    public virtual void PlayFirstPersonCrossfade(string layerName, int animationStateNameHash, int playbackRateParamHash, float duration, float crossfadeDuration, bool durationIsMultiplier)
    {
        if (!Condition()) return;
        firstPersonCameraController.animator.PlayCrossfade(layerName, animationStateNameHash, playbackRateParamHash, duration, crossfadeDuration, durationIsMultiplier);
    }
    public virtual void PlayFirstPersonCrossfade(string layerName, string animationStateName, float crossfadeDuration)
    {
        if (!Condition()) return;
        firstPersonCameraController.animator.PlayCrossfade(layerName, animationStateName, crossfadeDuration);
    }
    public virtual void PlayFirstPersonCrossfade(string layerName, int animationStateHash, float crossfadeDuration)
    {
        if (!Condition()) return;
        firstPersonCameraController.animator.PlayCrossfade(layerName, animationStateHash, crossfadeDuration);
    }
    private bool Condition()
    {
        if (this is not BaseState baseState) return false;
        if (!baseState.isAuthority || !baseState.characterBody.isPlayerControlled || !firstPersonCameraController || !firstPersonCameraController.animator) return false;
        return true;
    }
    public virtual HashSet<GenericSkill> GetGenericSkillsWithSpecialSpear() => this is BaseState baseState ? BishopSpecialSpearSkillDef.GetGenericSkills(baseState.characterBody) : null;
    public virtual void RechargeSpecialSpearStocks(int amount) { if (this is BaseState baseState) BishopSpecialSpearSkillDef.RechargeStocksForSpecialSpearSkills(baseState.characterBody, amount); }
    public virtual void RechargeSpecialSpearStocks() { if (this is BaseState baseState) BishopSpecialSpearSkillDef.RechargeStocksForSpecialSpearSkills(baseState.characterBody); }
    public virtual void Shake(float duration, float radius, float frequency, float amplitude) { if (bishopComponent) bishopComponent.Shake(duration, radius, frequency, amplitude);}
}
