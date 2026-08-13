
using System.Security.Permissions;
using System.Security;
using BepInEx;
using BepInEx.Configuration;
using RoR2.ExpansionManagement;
using System;
using RoR2;
using BepInEx.Logging;
using CaeliImperium.Configs;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]
[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
[module: UnverifiableCode]
#pragma warning disable CS0618
#pragma warning restore CS0618
namespace CaeliImperium
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2API.SoundAPI.PluginGUID)]
    [BepInDependency(R2API.ItemAPI.PluginGUID)]
    [BepInDependency(R2API.DirectorAPI.PluginGUID)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
    [BepInDependency(BrynzaAPI.BrynzaAPI.ModGuid)]
    [BepInDependency(ModCompatabilities.RiskOfOptionsCompatability.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [System.Serializable]
    public class CaeliImperiumPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.brynzananas.caeliimperium";
        public const string ModName = "Caeli Imperium";
        public const string ModVer = "0.11.0";
        public const string ModPrefix = "CI";
        public static bool emotesEnabled;
        public static bool riskOfOptionsEnabled;
        public static ExpansionDef expansionDef;
        public static PluginInfo PluginInfo { get; private set; }
        public static ConfigFile configFile { get; private set; }
        public static ManualLogSource Log { get; private set; }
        public static BaseUnityPlugin instance { get; private set; }
        public static event Action onPluginDestroyed;
        public void Awake()
        {
            Log = Logger;
            PluginInfo = Info;
            configFile = Config;
            instance = this;
            riskOfOptionsEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatabilities.RiskOfOptionsCompatability.GUID);
            CaeliImperiumConfigs.OverrideConfigValuesOnUpdate = Config.Bind(CaeliImperiumConfigs.sectionName, "Override config values on update", true, "Update config values with new default values if existing config value matches old default value on mod update?");
            CaeliImperiumAssets.Init();
            if (riskOfOptionsEnabled) ModCompatabilities.RiskOfOptionsCompatability.Init();
            CaeliImperiumConfigs.Init();
            RoR2Application.onLoad += CaeliImperiumLanguage.Init;
        }
        public void OnDestroy()
        {
            RoR2Application.onLoad -= CaeliImperiumLanguage.Init;
            onPluginDestroyed?.Invoke();
        }
        public void AddCustomMusic()
        {

        }
        public void RemoveCustomMusic()
        {

        }
    }
}