using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ScriptableObjects;
[CreateAssetMenu(menuName = "CaeliImperium/CIMusicTrackDef")]
public class CIMusicTrackDef : MusicTrackDef
{
    public string soundbankName;
    public static uint baseGroupID = 3793484759;
    public uint stateDirID;
    public uint groupID;
    public uint stateID;
    public override void Preload()
    {
        if (!string.IsNullOrWhiteSpace(soundbankName))
        {
            AkSoundEngine.LoadBank(soundbankName, out _);
        }
    }
    public override void Play()
    {
        AkSoundEngine.SetState(baseGroupID, stateDirID);
        AkSoundEngine.SetState(groupID, stateID);
    }
    public override void Stop()
    {
        AkSoundEngine.SetState(baseGroupID, 0u);
        AkSoundEngine.SetState(groupID, 0u);
    }
}
