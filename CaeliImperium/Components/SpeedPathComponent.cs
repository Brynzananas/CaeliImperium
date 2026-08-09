using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    public class SpeedPathComponent : MonoBehaviour
    {
        public GlobalSpeedPath globalSpeedPath;
        public ItemBehaviours.DrawSpeedPathBehaviour speedPathDrawerComponent;
        public SpeedPathComponent nextSpeedPatchComponent;
        public int index;
        public static event Action<SpeedPathComponent> OnSpeedPathCharged;
        public void Charge(SpeedPathComponent speedPatchComponent)
        {
            nextSpeedPatchComponent = speedPatchComponent;
            if (speedPatchComponent)
            {
                speedPatchComponent.transform.position = (speedPatchComponent.transform.position + transform.position) / 2f;
                speedPatchComponent.name += "Charged";
            }
            if (!speedPathDrawerComponent) return;
            //speedPathDrawerComponent.globalSpeedPath.AddPath(this);
            OnSpeedPathCharged?.Invoke(this);
        }
    }
}
