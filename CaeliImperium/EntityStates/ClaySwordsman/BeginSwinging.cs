using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaeliImperiumEntityStates.ClaySwordsman;

public class BeginSwinging : BaseClaySwordsmanState
{
    public static float baseDuration = 0.5f;
    public static float baseCrossfadeDuration = 0.02f;
    public float duration;
    public float crossfadeDuration;
    public float cachedAttackSpeed;
    public override void OnEnter()
    {
        base.OnEnter();
        cachedAttackSpeed = attackSpeedStat;
        duration = baseDuration / cachedAttackSpeed;
        crossfadeDuration = baseCrossfadeDuration / cachedAttackSpeed;
        PlayCrossfade("FullBody, Override", "SwingUp", "SwingUp.playbackRate", duration, crossfadeDuration);
        //PlayCrossfadeOnSword("Body", "SwingUp", "SwingUp.playbackRate", duration, crossfadeDuration);
        StartAimMode();
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (inputBank && characterDirection) characterDirection.moveVector = inputBank.aimDirection;
        if (!isAuthority || fixedAge < duration) return;
        outer.SetNextState(new Swing { cachedAttackSpeed = cachedAttackSpeed , activatorSkillSlot = activatorSkillSlot});
    }
}
