using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Mechanim
{
    public class ToggleComponentBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (activityOnEnter != ActivityType.Ignore) EnableDisableComponent(animator.gameObject, activityOnEnter == ActivityType.Enable);
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (activityOnExit != ActivityType.Ignore) EnableDisableComponent(animator.gameObject, activityOnExit == ActivityType.Enable);
        }
        public void EnableDisableComponent(GameObject gameObject, bool enabled)
        {
            if (componentTypeName.IsNullOrWhiteSpace()) return;
            Type type = Type.GetType(componentTypeName);
            if (type == null) return;
            Behaviour behaviour = gameObject.GetComponent(type) as Behaviour;
            if (!behaviour) return;
            behaviour.enabled = enabled;
        }
        public string componentTypeName;
        public ActivityType activityOnEnter;
        public ActivityType activityOnExit;
        public enum ActivityType
        {
            Ignore,
            Enable,
            Disable
        }
    }
}
