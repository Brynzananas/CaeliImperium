using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace CaeliImperium.Stages;
public static class GooseRunnerEvents
{
    public static SceneDef sceneDef;
    public static AssetBundle assetBundle;
    private static bool inited;
    public static void Init(SceneDef sceneDef)
    {
        if (inited) return;
        inited = true;
        assetBundle = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(CaeliImperiumPlugin.PluginInfo.Location), "assetbundles", "caeliimperiumgooserunnerscene")).assetBundle;
        sceneDef.loopedDestinationsGroup = CaeliImperiumAssets.LoopStage5;
        sceneDef.destinationsGroup = CaeliImperiumAssets.Stage5;
        CaeliImperiumAssets.Stage4.AddScene(sceneDef, CaeliImperiumAssets.TitanicPlains, 1f);
        CaeliImperiumAssets.LoopStage4.AddScene(sceneDef, CaeliImperiumAssets.TitanicPlains, 1f);
    }
}
