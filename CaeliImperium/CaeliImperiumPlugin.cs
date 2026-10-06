
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CaeliImperium.Components;
using CaeliImperium.Configs;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]
[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
[module: UnverifiableCode]
#pragma warning disable CS0618
#pragma warning restore CS0618
namespace CaeliImperium;

[BepInPlugin(ModGuid, ModName, ModVer)]
[BepInDependency(R2API.R2API.PluginGUID)]
[BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
[BepInDependency(R2API.SoundAPI.PluginGUID)]
[BepInDependency(R2API.ItemAPI.PluginGUID)]
[BepInDependency(R2API.DirectorAPI.PluginGUID)]
[BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
[BepInDependency(R2API.DamageAPI.PluginGUID)]
[BepInDependency(R2API.DotAPI.PluginGUID)]
[BepInDependency(R2API.ProcTypeAPI.PluginGUID)]
[BepInDependency(R2API.CharacterBodyAPI.PluginGUID)]
[BepInDependency(R2API.ColorsAPI.PluginGUID)]
[BepInDependency(BrynzaAPI.BrynzaAPI.ModGuid)]
[BepInDependency(ModCompatabilities.RiskOfOptionsCompatability.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[System.Serializable]
public class CaeliImperiumPlugin : BaseUnityPlugin
{
    public const string ModGuid = "com.brynzananas.caeliimperium";
    public const string ModName = "Caeli Imperium";
    public const string ModVer = "0.12.1";
    public const string ModPrefix = "CI";
    public static bool emotesEnabled;
    public static bool riskOfOptionsEnabled;
    public static ExpansionDef expansionDef;
    public static PluginInfo PluginInfo { get; private set; }
    public static ConfigFile configFile { get; private set; }
    public static ManualLogSource Log { get; private set; }
    public static BaseUnityPlugin instance { get; private set; }
    public static event Action onPluginDestroyed;
    public static NetworkUser currentNetworkUser;
    public void Awake()
    {
        Log = Logger;
        PluginInfo = Info;
        configFile = Config;
        instance = this;
        riskOfOptionsEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatabilities.RiskOfOptionsCompatability.GUID);
        CaeliImperiumSave.Init();
        CaeliImperiumConfigs.OverrideConfigValuesOnUpdate = Config.Bind(CaeliImperiumConfigs.sectionName, "Override config values on update", true, "Update config values with new default values if existing config value matches old default value on mod update?");
        CaeliImperiumAssets.Init();
        if (riskOfOptionsEnabled) ModCompatabilities.RiskOfOptionsCompatability.Init();
        CaeliImperiumConfigs.Init();
        RoR2Application.onLoad += CaeliImperiumLanguage.Init;
        //CaeliImperiumHooks.SetSaveHooks();
        CaeliImperiumHooks.SetHooks();
        
    }
    

    public void OnDestroy()
    {
        RoR2Application.onLoad -= CaeliImperiumLanguage.Init;
        //CaeliImperiumHooks.UnsetSaveHooks();
        CaeliImperiumHooks.UnsetHooks();
        onPluginDestroyed?.Invoke();
    }
    public void AddCustomMusic()
    {

    }
    public void RemoveCustomMusic()
    {

    }
}