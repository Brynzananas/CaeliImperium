using CaeliImperium.Components;
using RoR2.UI;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ScriptableObjects;
public class VoiceMessageDef : ScriptableObject
{
    private static FixedConditionalWeakTable<HUD, GameObject> voiceMessageFromHUD = [];
    public Sprite portraitIcon;
    public string subtitleToken;
    public string messageToken;
    public string playSoundString;
    public string typewriteSoundString;
    public float duration;
    public bool overrideExistingVoiceMessage;
    public void Play()
    {
        foreach (HUD hUD in HUD.instancesList)
        {
            if (!hUD || !hUD.mainContainer) continue;
            if (voiceMessageFromHUD.TryGetValue(hUD, out GameObject gameObject))
            {
                if (overrideExistingVoiceMessage)
                {
                    voiceMessageFromHUD.Remove(hUD);
                    Destroy(gameObject);
                }
                else
                {
                    continue;
                }
            }
            GameObject voiceMessage = Instantiate(CaeliImperiumAssets.VoiceMessage, hUD.mainContainer.transform);
            voiceMessageFromHUD.Add(hUD, voiceMessage);
            VoiceMessageController voiceMessageController = voiceMessage.GetComponent<VoiceMessageController>();
            voiceMessageController.Init(this);
        }
    }
}
