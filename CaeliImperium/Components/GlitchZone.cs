using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperium.Components.GlitchHUDController;

namespace CaeliImperium.Components;
public class GlitchZone : MonoBehaviour
{
    public AnimationCurve glitchCurve;
    public float distance;
    [SerializeField] private Transform _center;
    public Transform center => _center ?? transform;
    private GlitchZoneRunAction glitchZoneRunAction;
    public void Awake()
    {
        glitchZoneRunAction = new GlitchZoneRunAction(this);
    }
    public void OnEnable()
    {
        CaeliImperiumExpansionRunComponent.caeliImperiumRunActions.Add(glitchZoneRunAction);
        GlitchHUDController.globalUpdateGlitchValues += GlitchHUDController_globalUpdateGlitchValues;
        CaeliImperiumExpansionRunComponent.getDeathValue += CaeliImperiumExpansionRunComponent_getDeathValue;
    }

    private void CaeliImperiumExpansionRunComponent_getDeathValue(ref float deathScream)
    {
        float value = GetValue();
        if (value <= 0) return;
        deathScream += value;
    }

    private void GlitchHUDController_globalUpdateGlitchValues(GlitchHUDController glitchHUDController, ref GlitchHUDController.GlitchValues glitchValues)
    {
        float value = GetValue();
        if (value <= 0) return;
        glitchValues.enableCount++;
        glitchValues.totalAlpha += value;
        glitchValues.postProcessWeight += value;
    }
    public float GetValue()
    {
        if (glitchZoneRunAction == null) return 0f;
        return glitchZoneRunAction.GetValue();
    }
    public void OnDisable()
    {
        CaeliImperiumExpansionRunComponent.caeliImperiumRunActions.Remove(glitchZoneRunAction);
        GlitchHUDController.globalUpdateGlitchValues -= GlitchHUDController_globalUpdateGlitchValues;
        CaeliImperiumExpansionRunComponent.getDeathValue -= CaeliImperiumExpansionRunComponent_getDeathValue;
    }
    public class GlitchZoneRunAction : CaeliImperiumRunAction
    {
        public GlitchZoneRunAction(GlitchZone glitchZone)
        {
            this.glitchZone = glitchZone;
        }
        public GlitchZone glitchZone;
        public float sqrDistance = float.MaxValue;
        public bool enable;
        public float GetValue()
        {
            if (!glitchZone || !enable) return 0f;
            float sqrDistance = glitchZone.distance * glitchZone.distance;
            if (this.sqrDistance > sqrDistance) return 0f;
            return glitchZone.glitchCurve == null ? 0f : glitchZone.glitchCurve.Evaluate(this.sqrDistance / sqrDistance);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (glitchZone && currentCharacterBody)
            {
                enable = true;
                Vector3 vector3 = glitchZone.center.position - currentCharacterBody.transform.position;
                sqrDistance = vector3.sqrMagnitude;
            }
            else
            {
                enable = false;
                sqrDistance = float.MaxValue;
            }
        }
    }
}

