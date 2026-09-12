using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace CaeliImperium.Components;
[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
public class GlitchHUDController : MonoBehaviour
{
    public static int instancesCount {  get; private set; }
    private static List<GlitchHUDController> instances = [];
    public static ReadOnlyCollection<GlitchHUDController> readOnlyInstances => instances.AsReadOnly();
    [Serializable]
    public struct GlitchValues
    {
        public int enableCount;
        public bool enableShake;
        public float shakeIntensity;
        public float shakeSpeed;
        public bool enableFlicker;
        public float totalAlpha;
        public float minAlpha;
        public float maxAlpha;
        public float dropProbability;
        public bool randomizeFillAmount;
        public float minFillAmount;
        public float maxFillAmount;
        public float minNormalDuration;
        public float maxNormalDuration;
        public float minGlitchDuration;
        public float maxGlitchDuration;
        public float changeImageChance;
        public float changeTextChance;
        public bool eitherImageOrText;
        public float postProcessWeight;
    }
    public delegate void UpdateGlitchValues(GlitchHUDController glitchHUDController, ref GlitchValues glitchValues);
    public static event UpdateGlitchValues globalUpdateGlitchValues;
    public event UpdateGlitchValues updateGlitchValues;
    public GlitchValues glitchValues = new GlitchValues
    {
        enableShake = true,
        shakeIntensity = 500f,
        shakeSpeed = 50f,
        enableFlicker = true,
        minAlpha = 0.01f,
        maxAlpha = 0.2f,
        minFillAmount = 0.1f,
        maxFillAmount = 1f,
        minNormalDuration = 0.25f,
        maxNormalDuration = 1.5f,
        minGlitchDuration = 0.25f,
        maxGlitchDuration = 0.5f,
        changeImageChance = 40f,
        changeTextChance = 60f
    };
    public int enableCount
    {
        get => currentGlitchValues.enableCount;
        set => currentGlitchValues.enableCount = value;
    }
    public bool enableShake
    {
        get => currentGlitchValues.enableShake;
        set => currentGlitchValues.enableShake = value;
    }
    public float shakeIntensity
    {
        get => currentGlitchValues.shakeIntensity;
        set => currentGlitchValues.shakeIntensity = value;
    }
    public float shakeSpeed
    {
        get => currentGlitchValues.shakeSpeed;
        set => currentGlitchValues.shakeSpeed = value;
    }
    public bool enableFlicker
    {
        get => currentGlitchValues.enableFlicker;
        set => currentGlitchValues.enableFlicker = value;
    }
    public float totalAlpha
    {
        get => currentGlitchValues.totalAlpha;
        set => currentGlitchValues.totalAlpha = value;
    }
    public float minAlpha
    {
        get => currentGlitchValues.minAlpha;
        set => currentGlitchValues.minAlpha = value;
    }
    public float maxAlpha
    {
        get => currentGlitchValues.maxAlpha;
        set => currentGlitchValues.maxAlpha = value;
    }
    public float dropProbability
    {
        get => currentGlitchValues.dropProbability;
        set => currentGlitchValues.dropProbability = value;
    }
    public bool randomizeFillAmount
    {
        get => currentGlitchValues.randomizeFillAmount;
        set => currentGlitchValues.randomizeFillAmount = value;
    }
    public float minFillAmount
    {
        get => currentGlitchValues.minFillAmount;
        set => currentGlitchValues.minFillAmount = value;
    }
    public float maxFillAmount
    {
        get => currentGlitchValues.maxFillAmount;
        set => currentGlitchValues.maxFillAmount = value;
    }
    public float minNormalDuration
    {
        get => currentGlitchValues.minNormalDuration;
        set => currentGlitchValues.minNormalDuration = value;
    }
    public float maxNormalDuration
    {
        get => currentGlitchValues.maxNormalDuration;
        set => currentGlitchValues.maxNormalDuration = value;
    }
    public float minGlitchDuration
    {
        get => currentGlitchValues.minGlitchDuration;
        set => currentGlitchValues.minGlitchDuration = value;
    }
    public float maxGlitchDuration
    {
        get => currentGlitchValues.maxGlitchDuration;
        set => currentGlitchValues.maxGlitchDuration = value;
    }
    public float changeImageChance
    {
        get => currentGlitchValues.changeImageChance;
        set => currentGlitchValues.changeImageChance = value;
    }
    public float changeTextChance
    {
        get => currentGlitchValues.changeTextChance;
        set => currentGlitchValues.changeTextChance = value;
    }
    public bool eitherImageOrText
    {
        get => currentGlitchValues.eitherImageOrText;
        set => currentGlitchValues.eitherImageOrText = value;
    }
    public float ppWeight
    {
        get => currentGlitchValues.postProcessWeight;
        set => currentGlitchValues.postProcessWeight = value;
    }
    public Image targetImage;
    public Sprite[] glitchSprites;
    public TextMeshProUGUI targetText;
    public PostProcessVolume postProcessVolume;
    [TextArea] public string[] glitchTextLanguageTokens;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private GlitchValues currentGlitchValues;

    public void Awake()
    {
        instancesCount++;
        instances.Add(this);
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPosition = rectTransform.anchoredPosition;
    }
    public void OnDestroy()
    {
        instancesCount--;
        instances.Remove(this);
    }
    public void OnEnable()
    {
        //StartCoroutine(GlitchRoutine());
    }
    public void OnDisable()
    {
        //StopAllCoroutines();
        ResetUI();
    }
    public void Update()
    {
        GlitchValues glitchValues = this.glitchValues;
        globalUpdateGlitchValues?.Invoke(this, ref glitchValues);
        updateGlitchValues?.Invoke(this, ref glitchValues);
        currentGlitchValues = glitchValues;
        //canvasGroup.alpha = totalAlpha;
        if (!postProcessVolume) return;
        postProcessVolume.weight = currentGlitchValues.postProcessWeight;
    }
    public IEnumerator GlitchRoutine()
    {
        while (true)
        {
            float normalDuration = UnityEngine.Random.Range(minNormalDuration, maxNormalDuration);
            while (enableCount > 0 && totalAlpha > 0)
            {
                float glitchDuration = UnityEngine.Random.Range(minGlitchDuration, maxGlitchDuration);
                float elapsed = 0f;
                if (eitherImageOrText)
                {
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        if (targetImage && !targetImage.gameObject.activeSelf) targetImage.gameObject.SetActive(true);
                        if (targetText && targetText.gameObject.activeSelf) targetText.gameObject.SetActive(false);
                    }
                    else
                    {
                        if (targetImage && targetImage.gameObject.activeSelf) targetImage.gameObject.SetActive(false);
                        if (targetText && !targetText.gameObject.activeSelf) targetText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (targetImage && !targetImage.gameObject.activeSelf) targetImage.gameObject.SetActive(true);
                    if (targetText && !targetText.gameObject.activeSelf) targetText.gameObject.SetActive(true);
                }
                if (targetImage && targetImage.gameObject.activeSelf)
                {
                    if (glitchSprites != null && glitchSprites.Length > 0 && UnityEngine.Random.value < changeImageChance / 100f) targetImage.sprite = glitchSprites[UnityEngine.Random.Range(0, glitchSprites.Length)];
                    if (randomizeFillAmount && targetImage.type == Image.Type.Filled) targetImage.fillAmount = UnityEngine.Random.Range(minFillAmount, maxFillAmount);
                }
                if (targetText && targetText.gameObject.activeSelf && glitchTextLanguageTokens != null && glitchTextLanguageTokens.Length > 0 && UnityEngine.Random.value < changeTextChance / 100f) targetText.text = Language.GetString(glitchTextLanguageTokens[UnityEngine.Random.Range(0, glitchTextLanguageTokens.Length)]);
                while (elapsed < glitchDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    if (enableShake)
                    {
                        Vector2 offset = new Vector2(
                            (Mathf.PerlinNoise(Time.unscaledTime * shakeSpeed, 0f) - 0.5f) * shakeIntensity * 2f,
                            (Mathf.PerlinNoise(0f, Time.unscaledTime * shakeSpeed) - 0.5f) * shakeIntensity * 2f
                        );
                        rectTransform.anchoredPosition = originalPosition + offset;
                    }
                    if (enableFlicker) canvasGroup.alpha *= UnityEngine.Random.value < dropProbability ? 0f : UnityEngine.Random.Range(minAlpha, maxAlpha);
                    yield return null;
                }
                ResetUI();
                yield return null;
            }
            yield return new WaitForSecondsRealtime(normalDuration);
        }
    }
    public void ResetUI()
    {
        rectTransform.anchoredPosition = originalPosition;
        canvasGroup.alpha = 0f;
        if (targetImage)
        {
            if (targetImage.type == Image.Type.Filled) targetImage.fillAmount = 1f;
            targetImage.gameObject.SetActive(false);
        }
        if (targetText)
        {
            targetText.gameObject.SetActive(false);
        }
    }
}
