using CaeliImperium.Components;
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
        public static float minSuperSecretScreamTimerAdd = 300f;
        public static float maxSuperSecretScreamTimerAdd = 600f;
        public static float minSuperSecretScreamTimerOnStageBegin = 60f;
        public float superSecretScreamTimer;
        public static event Action<CaeliImperiumExpansionRunComponent> onFixedUpdate;
        public static event Action<CaeliImperiumExpansionRunComponent> onUpdate;
        public static float hudAlphaSmoothTime = 0.2f;
        public float hudAlphaVelocity;
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
            EffectManager.SpawnEffect(CaeliImperiumAssets.SuperSecretScreamEffect.index, new EffectData(), true);
            superSecretScreamTimer = UnityEngine.Random.Range(minSuperSecretScreamTimerAdd, maxSuperSecretScreamTimerAdd);
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
            onFixedUpdate?.Invoke(this);
        }
        public void Update()
        {
            if (SuperSecretScreamComponent.count > 0)
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
            onUpdate?.Invoke(this);
        }
    }
}
