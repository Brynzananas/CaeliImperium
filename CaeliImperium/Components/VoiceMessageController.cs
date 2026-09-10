using BepInEx;
using CaeliImperium.ScriptableObjects;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaeliImperium.Components;
public class VoiceMessageController : MonoBehaviour
{
    public Image portraitImage;
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI messageText;
    public TypewriteTextController typewriteTextController;
    public ObjectScaleCurve objectScaleCurve;
    private uint soundId;
    public void Init(VoiceMessageDef voiceMessageDef)
    {
        if (portraitImage) portraitImage.sprite = voiceMessageDef.portraitIcon;
        if (subtitleText) subtitleText.text = Language.GetString(voiceMessageDef.subtitleToken);
        if (messageText) messageText.text = Language.GetString(voiceMessageDef.messageToken);
        float duration = voiceMessageDef.duration == 0f && typewriteTextController ? messageText.text.Length * typewriteTextController.delayBetweenKeys + 4f : voiceMessageDef.duration;
        Destroy(gameObject, duration);
        if (!voiceMessageDef.playSoundString.IsNullOrWhiteSpace()) soundId = Util.PlaySound(voiceMessageDef.playSoundString, RoR2Application.instance.gameObject);
        if (typewriteTextController) typewriteTextController.soundString = voiceMessageDef.typewriteSoundString;
        if (objectScaleCurve) objectScaleCurve.timeMax = duration;

    }
    public void OnDisable()
    {
        if (soundId != 0U) AkSoundEngine.StopPlayingID(soundId);
    }
}
