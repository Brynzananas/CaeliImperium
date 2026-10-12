using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace CaeliImperium.Components;
public class CopyPostProcessVolume : MonoBehaviour
{
    public PostProcessVolume sourcePostProcessVolume;
    public PostProcessVolume destinationPostProcessVolume;
    public void Update()
    {
        Copy();
    }
    public void Copy()
    {
        if (!sourcePostProcessVolume || destinationPostProcessVolume) return;
        destinationPostProcessVolume.blendDistance = sourcePostProcessVolume.blendDistance;
        destinationPostProcessVolume.previousBlendDistCalc = sourcePostProcessVolume.previousBlendDistCalc;
        destinationPostProcessVolume.isGlobal = sourcePostProcessVolume.isGlobal;
        destinationPostProcessVolume.m_InternalProfile = sourcePostProcessVolume.m_InternalProfile;
        destinationPostProcessVolume.m_PreviousLayer = sourcePostProcessVolume.m_PreviousLayer;
        destinationPostProcessVolume.m_PreviousPriority = sourcePostProcessVolume.m_PreviousPriority;
        destinationPostProcessVolume.m_TempColliders = sourcePostProcessVolume.m_TempColliders;
        destinationPostProcessVolume.oldWeight = sourcePostProcessVolume.oldWeight;
        destinationPostProcessVolume.priority = sourcePostProcessVolume.priority;
        destinationPostProcessVolume.sharedProfile = sourcePostProcessVolume.sharedProfile;
        destinationPostProcessVolume.targetLayer = sourcePostProcessVolume.targetLayer;
        destinationPostProcessVolume.weight = sourcePostProcessVolume.weight;
    }
}
