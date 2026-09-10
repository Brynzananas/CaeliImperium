using CaeliImperium.Components;
using CaeliImperium.Configs;
using RoR2;
using RoR2.UI;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium
{
    public class CaeliImperiumExpansionRunComponent : NetworkBehaviour
    {
        public delegate void GetDeathValue(ref float deathScream);
        public static event GetDeathValue getDeathValue;
        public static CaeliImperiumExpansionRunComponent instance;
        public static float minSuperSecretScreamTimerAdd = 600f;
        public static float maxSuperSecretScreamTimerAdd = 9000f;
        public static float minSuperSecretScreamTimerOnStageBegin = 60f;
        public static float superSecretScreamChance = 25f;
        public float superSecretScreamTimer;
        public static float hudAlphaSmoothTime = 0.2f;
        public static List<CaeliImperiumRunAction> caeliImperiumRunActions = [];
        public float death;
        public float hudAlpha = 1f;
        public float hudAlphaVelocity;
        public PlayerCharacterMasterController currentPlayerCharacterMasterController;
        public CharacterMaster currentCharacterMaster;
        public CharacterBody currentCharacterBody;
        private FixedConditionalWeakTable<HUD, CanvasGroup> keyValuePairs = [];
        private GameObject glitchHUDController;
        public void OnEnable()
        {
            Stage.onServerStageBegin += Stage_onServerStageBegin;
            GlitchHUDController.globalUpdateGlitchValues += GlitchHUDController_globaUpdateGlitchValues;
        }
        private void GlitchHUDController_globaUpdateGlitchValues(GlitchHUDController glitchHUDController, ref GlitchHUDController.GlitchValues glitchValues)
        {
            float newValue = 1f - hudAlpha;
            if (newValue <= 0f) return;
            glitchValues.enableCount++;
            glitchValues.totalAlpha += newValue;
        }

        public void OnDisable()
        {
            Stage.onServerStageBegin -= Stage_onServerStageBegin;
            GlitchHUDController.globalUpdateGlitchValues -= GlitchHUDController_globaUpdateGlitchValues;
        }
        private void Stage_onServerStageBegin(Stage obj)
        {
            superSecretScreamTimer = Mathf.Max(superSecretScreamTimer, minSuperSecretScreamTimerOnStageBegin);
        }
        public void Awake()
        {
            instance = this;
            superSecretScreamTimer = UnityEngine.Random.Range(minSuperSecretScreamTimerAdd, maxSuperSecretScreamTimerAdd);
        }
        public void CallScream()
        {
            if (NetworkServer.active)
            {
                Scream();
                RpcScream();
            }
            else
            {
                CmdScream();
            }
        }
        [Command]
        public void CmdScream() => CallScream();
        [ClientRpc]
        public void RpcScream()
        {
            if (!NetworkServer.active) Scream();
        }
        public void Scream()
        {
            if (CaeliImperiumConfigs.Screaming.Value) EffectManager.SpawnEffect(CaeliImperiumAssets.SuperSecretScreamEffect.index, new EffectData(), false);
        }
        public void FixedUpdate()
        {
            if (NetworkServer.active && Stage.instance && !Stage.instance.completed)
            {
                if (superSecretScreamTimer <= 0f)
                {
                    superSecretScreamTimer = UnityEngine.Random.Range(minSuperSecretScreamTimerAdd, maxSuperSecretScreamTimerAdd);
                    if (UnityEngine.Random.value < superSecretScreamChance / 100f) CallScream();
                }
                else
                {
                    superSecretScreamTimer -= Time.fixedDeltaTime;
                }
            }
            currentPlayerCharacterMasterController = CaeliImperiumUtils.GetPlayerCharacterMasterController();
            currentCharacterMaster = currentPlayerCharacterMasterController ? currentPlayerCharacterMasterController.master : null;
            currentCharacterBody = currentPlayerCharacterMasterController ? currentPlayerCharacterMasterController.body : null;
            foreach (CaeliImperiumRunAction caeliImperiumRunAction in caeliImperiumRunActions)
            {
                if (caeliImperiumRunAction == null) continue;
                caeliImperiumRunAction.caeliImperiumExpansionRunComponent = this;
                caeliImperiumRunAction.FixedUpdate();
            }

        }
        public void Update()
        {
            death = 0f;
            getDeathValue?.Invoke(ref death);
            if (!glitchHUDController) glitchHUDController = Instantiate(CaeliImperiumAssets.GlitchHUD);
            hudAlpha = Mathf.Clamp01(Mathf.SmoothDamp(hudAlpha, 1f - death, ref hudAlphaVelocity, hudAlphaSmoothTime));
            foreach (HUD hUD in HUD.instancesList)
            {
                if (!hUD) continue;
                if (!keyValuePairs.TryGetValue(hUD, out CanvasGroup canvasGroup))
                {
                    canvasGroup = hUD.GetOrAddComponent<CanvasGroup>();
                    keyValuePairs.Add(hUD, canvasGroup);
                }
                canvasGroup.alpha = hudAlpha;
            }
            foreach (CaeliImperiumRunAction caeliImperiumRunAction in caeliImperiumRunActions)
            {
                if (caeliImperiumRunAction == null) continue;
                caeliImperiumRunAction.caeliImperiumExpansionRunComponent = this;
                caeliImperiumRunAction.Update();
            }
        }
    }
}
