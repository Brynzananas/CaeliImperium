
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
    [BepInDependency(BrynzaAPI.BrynzaAPI.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(ModCompatabilities.RiskOfOptionsCompatability.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [System.Serializable]
    public class CaeliImperiumPlugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.brynzananas.caeliimperium";
        public const string ModName = "Caeli Imperium";
        public const string ModVer = "0.9.0";
        public const string ModPrefix = "CI";
        public static bool emotesEnabled;
        public static bool riskOfOptionsEnabled;
        public static ExpansionDef expansionDef;
        public static PluginInfo PluginInfo { get; private set; }
        public static ConfigFile configFile { get; private set; }
        public static ManualLogSource Log { get; private set; }
        public static Action OnPluginDestroyed;
        public void Awake()
        {
            Log = Logger;
            PluginInfo = Info;
            configFile = Config;
            riskOfOptionsEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatabilities.RiskOfOptionsCompatability.GUID);
            CaeliImperiumAssets.Init();
            if (riskOfOptionsEnabled) ModCompatabilities.RiskOfOptionsCompatability.Init();
            DrawSpeedPathConfigs.Init();
            HealReceivedDamageConfigs.Init();
            InfiniteSecondarySkillChargesConfigs.Init();
            RoR2Application.onLoad += Language.Init;
        }
        public void OnDestroy()
        {
            RoR2Application.onLoad -= Language.Init;
            OnPluginDestroyed?.Invoke();
        }
    }
}