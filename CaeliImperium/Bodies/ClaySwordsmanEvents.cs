using RoR2.Skills;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using R2API.SpawnCardCloning;
using UnityEngine.AddressableAssets;

namespace CaeliImperium.Bodies;

public static class ClaySwordsmanEvents
{
    public static GameObject BodyPrefab;
    public static CharacterBody Body;
    public static GameObject MasterPrefab;
    public static SkillFamily Primary;
    public static SkillDef Swing;
    public static CharacterSpawnCardClone characterSpawnCardClone;
    private static bool inited;
    public static void Init(GameObject gameObject)
    {
        if (inited) return;
        GameObject ClayBruiserBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClayBruiser/ClayBruiserBody.prefab").WaitForCompletion();
        if (ClayBruiserBody) gameObject.CopyAKBank(ClayBruiserBody);
        Body = gameObject.HandleBody(BaseCrosshairType.SimpleDot, BaseCCPType.Standard, BaseFootstepDustType.GenericHuge);
        MasterPrefab = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/ClaySwordsman/CIClaySwordsmanMaster.prefab").RegisterMaster();
        Primary = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/ClaySwordsman/CIClaySwordsmanPrimary.asset").RegisterSkillFamily();
        Swing = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/ClaySwordsman/CIClaySwordsmanSwing.asset").RegisterSkillDef();
        characterSpawnCardClone = CaeliImperiumAssets.assetBundle.LoadAsset<CharacterSpawnCardClone>("Assets/CaeliImperium/Bodies/ClaySwordsman/csccClaySwordsman.asset");
        characterSpawnCardClone.Register();
        inited = true;
        typeof(CaeliImperiumEntityStates.ClaySwordsman.Spawn).RegisterEntityState();
        typeof(CaeliImperiumEntityStates.ClaySwordsman.BeginSwinging).RegisterEntityState();
        typeof(CaeliImperiumEntityStates.ClaySwordsman.Swing).RegisterEntityState();
    }
}
