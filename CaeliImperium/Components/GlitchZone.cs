using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
    }
    private void GlitchHUDController_globalUpdateGlitchValues(GlitchHUDController glitchHUDController, ref GlitchHUDController.GlitchValues glitchValues)
    {
        if (glitchZoneRunAction == null || !glitchZoneRunAction.enable) return;
        float sqrDistance = distance * distance;
        if (glitchZoneRunAction.sqrDistance > sqrDistance) return;
        glitchValues.enableCount++;
        float value = glitchCurve == null ? 0f : glitchCurve.Evaluate(glitchZoneRunAction.sqrDistance / sqrDistance);
        glitchValues.totalAlpha += value;
        glitchValues.postProcessWeight += value;
    }
    public void OnDisable()
    {
        CaeliImperiumExpansionRunComponent.caeliImperiumRunActions.Remove(glitchZoneRunAction);
        GlitchHUDController.globalUpdateGlitchValues -= GlitchHUDController_globalUpdateGlitchValues;
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

