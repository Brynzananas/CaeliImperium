using CaeliImperium.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaeliImperiumEntityStates.BomberWisp
{
    public class SpecialBomberWispMainState : EntityStates.FlyState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            //musicSubscriber = GetComponent<MusicSubscriber>();
            //if (musicSubscriber) musicSubscriber.OnMusicAction += MusicSubscriber_OnMusicAction;
            BrynzaAPI.BrynzaMusicTrackController.OnMusicAction += MusicSubscriber_OnMusicAction;
        }
        public override void OnExit()
        {
            base.OnExit();
            BrynzaAPI.BrynzaMusicTrackController.OnMusicAction -= MusicSubscriber_OnMusicAction;
            //if (musicSubscriber) musicSubscriber.OnMusicAction -= MusicSubscriber_OnMusicAction;
        }

        private void MusicSubscriber_OnMusicAction(object cookie, AkCallbackType type, object info)
        {
            if (type == AkCallbackType.AK_MusicSyncUserCue && skillLocator && skillLocator.primary && skillLocator.primary.stateMachine)
            {
                skillLocator.primary.stateMachine.SetNextState(new FireBomb { activatorSkillSlot = skillLocator.primary });
            }
        }
    }
}
