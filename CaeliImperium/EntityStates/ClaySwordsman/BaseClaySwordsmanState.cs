using CaeliImperium;
using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperiumEntityStates.ClaySwordsman;

public class BaseClaySwordsmanState : BaseSkillState
{
    public virtual Animator GetSwordAnimator()
    {
        ChildLocator childLocator = GetModelChildLocator();
        if (!childLocator) return null;
        Transform sword = childLocator.FindChild("Sword");
        if (!sword) return null;
        return sword.GetComponent<Animator>();
    }
    public virtual void PlayCrossfadeOnSword(string layerName, string animationStateName, float crossfadeDuration)
    {
        Animator swordAnimator = GetSwordAnimator();
        if (!swordAnimator) return;
        swordAnimator.PlayCrossfade(layerName, animationStateName, crossfadeDuration);
    }
    public virtual void PlayCrossfadeOnSword(string layerName, int animationStateHash, float crossfadeDuration)
    {
        Animator swordAnimator = GetSwordAnimator();
        if (!swordAnimator) return;
        swordAnimator.PlayCrossfade(layerName, animationStateHash, crossfadeDuration);
    }
    public virtual void PlayCrossfadeOnSword(string layerName, int animationStateNameHash, int playbackRateParamHash, float duration, float crossfadeDuration)
    {
        Animator swordAnimator = GetSwordAnimator();
        if (!swordAnimator) return;
        swordAnimator.PlayCrossfade(layerName, animationStateNameHash, playbackRateParamHash, duration, crossfadeDuration);
    }
    public virtual void PlayCrossfadeOnSword(string layerName, string animationStateName, string playbackRateParam, float duration, float crossfadeDuration)
    {
        Animator swordAnimator = GetSwordAnimator();
        if (!swordAnimator) return;
        swordAnimator.PlayCrossfade(layerName, animationStateName, playbackRateParam, duration, crossfadeDuration);
    }
}
