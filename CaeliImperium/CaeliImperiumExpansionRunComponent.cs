using CaeliImperium.Components;
using CaeliImperium.Configs;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium
{
    public class CaeliImperiumExpansionRunComponent : MonoBehaviour
    {
        public static CaeliImperiumExpansionRunComponent instance;
        public static float minSuperSecretScreamTimerAdd = 600f;
        public static float maxSuperSecretScreamTimerAdd = 9000f;
        public static float minSuperSecretScreamTimerOnStageBegin = 60f;
        public float superSecretScreamTimer;
        public static float hudAlphaSmoothTime = 0.2f;
        public static List<CaeliImperiumRunAction> caeliImperiumRunActions = [];
        public float hudAlphaVelocity;
        public PlayerCharacterMasterController currentPlayerCharacterMasterController;
        public CharacterMaster currentCharacterMaster;
        public CharacterBody currentCharacterBody;
        public void OnEnable()
        {
            Stage.onServerStageBegin += Stage_onServerStageBegin;
        }
        public void OnDisable()
        {
            Stage.onServerStageBegin -= Stage_onServerStageBegin;
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
        public void Scream()
        {
            superSecretScreamTimer = UnityEngine.Random.Range(minSuperSecretScreamTimerAdd, maxSuperSecretScreamTimerAdd);
            EffectManager.SpawnEffect(CaeliImperiumAssets.SuperSecretScreamEffect.index, new EffectData(), true);
        }
        public void FixedUpdate()
        {
            if (NetworkServer.active && Stage.instance && !Stage.instance.completed)
            {
                if (superSecretScreamTimer <= 0f)
                {
                    Scream();
                }
                else
                {
                    superSecretScreamTimer -= Time.fixedDeltaTime;
                }
            }
            currentPlayerCharacterMasterController = null;
            currentCharacterMaster = null;
            currentCharacterBody = null;
            if (PlayerCharacterMasterController.instances != null && PlayerCharacterMasterController.instances.Count > 0)
            {
                currentPlayerCharacterMasterController = PlayerCharacterMasterController.instances[0];
                currentCharacterMaster = currentPlayerCharacterMasterController.master;
                if (currentCharacterMaster)
                {
                    currentCharacterBody = currentCharacterMaster.GetBody();
                }
            }
            foreach (CaeliImperiumRunAction caeliImperiumRunAction in caeliImperiumRunActions)
            {
                if (caeliImperiumRunAction == null) continue;
                caeliImperiumRunAction.caeliImperiumExpansionRunComponent = this;
                caeliImperiumRunAction.FixedUpdate();
            }
            
        }
        public void Update()
        {
            if (SuperSecretScreamComponent.count > 0 && CaeliImperiumConfigs.Screaming.Value)
            {
                foreach (HUD hUD in HUD.instancesList)
                {
                    if (!hUD) continue;
                    CanvasGroup canvasGroup = hUD.GetOrAddComponent<CanvasGroup>();
                    canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha, 0f, ref hudAlphaVelocity, hudAlphaSmoothTime);
                }
            }
            else
            {
                foreach (HUD hUD in HUD.instancesList)
                {
                    if (!hUD) continue;
                    CanvasGroup canvasGroup = hUD.GetOrAddComponent<CanvasGroup>();
                    canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha, 1f, ref hudAlphaVelocity, hudAlphaSmoothTime);
                }
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
