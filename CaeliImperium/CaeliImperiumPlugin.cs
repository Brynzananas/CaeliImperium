
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CaeliImperium.Bodies;
using CaeliImperium.Components;
using CaeliImperium.Configs;
using CaeliImperium.ModCompatibilities;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.UI;
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
[BepInDependency(R2API.DifficultyAPI.PluginGUID)]
[BepInDependency(BrynzaAPI.BrynzaAPI.ModGuid)]
[BepInDependency(RiskOfOptionsCompatability.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(EnemiesReturnsCompatability.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(EnemyAbilitiesCompatibility.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[System.Serializable]
public class CaeliImperiumPlugin : BaseUnityPlugin
{
    public const string ModGuid = "com.brynzananas.caeliimperium";
    public const string ModName = "Caeli Imperium";
    public const string ModVer = "0.13.0";
    public const string ModPrefix = "CI";
    public static bool emotesEnabled;
    public static bool riskOfOptionsEnabled;
    public static bool enemiesReturnsEnabled;
    public static bool enemyAbilitiesEnabled;
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
        riskOfOptionsEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(RiskOfOptionsCompatability.GUID);
        enemiesReturnsEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(EnemiesReturnsCompatability.GUID);
        enemyAbilitiesEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(EnemyAbilitiesCompatibility.GUID);
        CaeliImperiumSave.Init();
        CaeliImperiumConfigs.OverrideConfigValuesOnUpdate = Config.Bind(CaeliImperiumConfigs.sectionName, "Override config values on update", true, "Update config values with new default values if existing config value matches old default value on mod update?");
        CaeliImperiumConfigs.AlphaContent = Config.Bind(CaeliImperiumConfigs.sectionName, "Alpha content", true, "Enable alpha content? Current alpha content: Doomguy survivor, Hurt Me Plenty difficulty");
        CaeliImperiumAssets.Init();
        if (riskOfOptionsEnabled) RiskOfOptionsCompatability.Init();
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
    public void Update()
    {
        if (!DoomguyEvents.init) return;
        bool anyoneISDoombuy = CaeliImperiumUtils.AnyoneSelectedDoomguy();
        RuleChoiceController normalRuleChoiceController = null;
        RuleChoiceController hurtMePlentyRuleChoiceController = null;
        NetworkUser networkUser1 = null;
        foreach (RuleChoiceController ruleChoiceController in RuleChoiceController.instancesList)
        {
            if (!ruleChoiceController) continue;
            RuleChoiceDef ruleChoiceDef = ruleChoiceController.choiceDef;
            if (ruleChoiceDef == null) continue;
            if (ruleChoiceDef.difficultyIndex == DifficultyIndex.Normal) normalRuleChoiceController = ruleChoiceController;
            if (ruleChoiceDef.difficultyIndex == DoomguyEvents.HurtMePlentyDifficultyIndex) hurtMePlentyRuleChoiceController = ruleChoiceController;
            NetworkUser networkUser = ruleChoiceController.FindNetworkUser();
            if (!networkUser || !networkUser.hasAuthority) continue;
            networkUser1 = networkUser;
            if (normalRuleChoiceController && hurtMePlentyRuleChoiceController && networkUser1) break;
        }
        if (!normalRuleChoiceController || !hurtMePlentyRuleChoiceController || !networkUser1) return;
        PreGameRuleVoteController preGameRuleVoteController = PreGameRuleVoteController.FindForUser(networkUser1);
        if (!preGameRuleVoteController) return;
        if (anyoneISDoombuy)
        {
            if (!preGameRuleVoteController.IsChoiceVoted(hurtMePlentyRuleChoiceController.choiceDef)) hurtMePlentyRuleChoiceController.OnClick();
        }
        else
        {
            if (preGameRuleVoteController.IsChoiceVoted(hurtMePlentyRuleChoiceController.choiceDef)) normalRuleChoiceController.OnClick();
        }
    }
    public void AddCustomMusic()
    {

    }
    public void RemoveCustomMusic()
    {

    }
}