using CaeliImperium.Items;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components
{
    public class HealReceivedDamageVisual : AkTriggerBase
    {
        public static float smoundVolumeSmoothTime = 0.2f;
        public static float overlaySmoothTime = 0.2f;
        public static float overlayAlphaMultiplier = 1f;
        public static float effectScale;
        public static float maxVolume = 1f;
        public static float volumeMultiplier = 5f;
        private float volume;
        private float healthCoefficient;
        private float overlayAlpha;
        private float overlayAlphaVelocity;
        private uint soundEvent1;
        private float sound1Volume;
        private float sound1VolumeVelocity;
        private uint soundEvent2;
        private float sound2Volume;
        private float sound2VolumeVelocity;
        private float healRate;
        private bool typeBeat;
        private bool alive;
        private bool inited;
        private TemporaryOverlay temporaryOverlay;
        private TemporaryOverlayInstance temporaryOverlayInstance;
        private CharacterBody characterBody;
        private ModelLocator modelLocator;
        private Transform modelTransform;
        private CharacterModel characterModel;
        public void Update()
        {
            overlayAlpha = Mathf.SmoothDamp(overlayAlpha, alive ? healthCoefficient * overlayAlphaMultiplier : 0f, ref overlayAlphaVelocity, overlaySmoothTime, float.MaxValue, Time.unscaledDeltaTime);
            if (temporaryOverlayInstance != null)
            {
                //temporaryOverlayInstance.alphaCurve = AnimationCurve.Constant(0f, 0f, overlayAlpha);
                AnimationCurve animationCurve = temporaryOverlayInstance.alphaCurve;
                Keyframe[] keyframes = animationCurve.keys;
                for (int i = 0; i < keyframes.Length; i++)
                {
                    ref Keyframe keyfame = ref keyframes[i];
                    keyfame.value = overlayAlpha;
                }
                animationCurve.keys = keyframes;
            }
            if (!inited) return;
            sound1Volume = Mathf.SmoothDamp(sound1Volume, alive ? (typeBeat ? 0f : 100f * volume) : 0f, ref sound1VolumeVelocity, smoundVolumeSmoothTime, float.MaxValue, Time.unscaledDeltaTime);
            sound2Volume = Mathf.SmoothDamp(sound2Volume, alive ? (typeBeat ? 100f * volume : 0f) : 0f, ref sound2VolumeVelocity, smoundVolumeSmoothTime, float.MaxValue, Time.unscaledDeltaTime);
            AkSoundEngine.SetRTPCValueByPlayingID("Volume_HalfLife_Charger", sound1Volume, soundEvent1);
            AkSoundEngine.SetRTPCValueByPlayingID("Volume_HalfLife_Charger_Type_Beat", sound2Volume, soundEvent2);
        }
        public void UpdateVisuals(float healRate, float healthCoefficient)
        {
            this.healthCoefficient = healthCoefficient;
            this.healRate = healRate;
            this.typeBeat = healthCoefficient > HealReceivedDamageEvents.neededHealRateToTypeBeat;
            this.alive = healRate > 0f;
            volume = Mathf.Lerp(0f, maxVolume, healthCoefficient * volumeMultiplier);
            if (!inited)
            {
                soundEvent1 = CaeliImperiumUtils.PlaySound("Play_HalfLife_Health_Charger", gameObject, MarkerCallback, this);
                soundEvent2 = CaeliImperiumUtils.PlaySound("Play_HalfLife_Health_Charger_Beat", gameObject, MarkerCallback, this);
                inited = true;
            }
            if (!characterModel) GetCharacterModel();
            if (!characterModel) return;
            if (temporaryOverlayInstance == null)
            {
                temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(characterModel.gameObject);
                temporaryOverlayInstance.alphaCurve = AnimationCurve.Constant(0f, 0f, healthCoefficient * overlayAlphaMultiplier);
                temporaryOverlayInstance.originalMaterial = HealReceivedDamageEvents.HealMaterial;
                temporaryOverlayInstance.inspectorCharacterModel = characterModel;
                temporaryOverlayInstance.assignedCharacterModel = characterModel;
                temporaryOverlayInstance.animateShaderAlpha = true;
                temporaryOverlayInstance.AddToCharacterModel(characterModel);
            }
        }
        public void MarkerCallback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
        {
            if (sound2Volume == 0f || !characterBody) return;
            AkMarkerCallbackInfo markerInfo = in_info as AkMarkerCallbackInfo;
            if (markerInfo.strLabel == "HalfLife_Charger_Type_Beat_Boop")
            {
                EffectData effectData = new EffectData
                {
                    scale = (characterBody.bestFitActualRadius + effectScale),
                    rootObject = characterBody.gameObject,
                    origin = characterBody.footPosition,
                };
                EffectManager.SpawnEffect(HealReceivedDamageEvents.TypeBeatEffect.index, effectData, false);
            }
        }
        public CharacterModel GetCharacterModel()
        {
            if (characterModel) return characterModel;
            if (!characterBody) characterBody = GetComponent<CharacterBody>();
            if (!characterBody) return null;
            if (!modelLocator) modelLocator = characterBody.modelLocator;
            if (!modelLocator) return null;
            if (!modelTransform) modelTransform = modelLocator.modelTransform;
            if (!modelTransform) return null;
            if (!characterModel) characterModel = modelTransform.GetComponent<CharacterModel>();
            return characterModel;
        }
    }
}
