using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace CaeliImperium.Components;

public class MonsterChestFilledMessageController : MonoBehaviour
{
    public float fadeTime;
    public float speed;
    public CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private float fadeTimer;
    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public void OnEnable()
    {
        fadeTimer = fadeTime;
    }
    public void Update()
    {
        if (fadeTimer > 0f) fadeTimer -= Time.deltaTime;
        if (canvasGroup)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeTimer / fadeTime);
        }
        if (!rectTransform) return;
        Vector2 vector2 = rectTransform.localPosition;
        vector2.y += speed * Time.deltaTime;
        rectTransform.localPosition = vector2;
    }
}
