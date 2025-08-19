
using System.Security.Permissions;
using System.Security;
using BepInEx;
using RoR2.ContentManagement;
using BepInEx.Configuration;
using RoR2.ExpansionManagement;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]
[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
[module: UnverifiableCode]
#pragma warning disable CS0618
#pragma warning restore CS0618
namespace CaeliImperium
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(BrynzaAPI.BrynzaAPI.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
    [System.Serializable]
    public class Main : BaseUnityPlugin
    {
        public const string ModGuid = "com.brynzananas.caeliimperium";
        public const string ModName = "Caeli Imperium";
        public const string ModVer = "1.0.0";
        public const string ModPrefix = "CI";
        public static bool emotesEnabled;
        public static bool riskOfOptionsEnabled;
        public static ExpansionDef expansionDef;
        public static PluginInfo PluginInfo { get; private set; }
        public static ConfigFile configFile { get; private set; }

        public void Awake()
        {
            PluginInfo = Info;
            configFile = Config;
            Hooks.Init();
            Assets.Init();
            ContentManager.collectContentPackProviders += (addContentPackProvider) =>
            {
                addContentPackProvider(new ContentPacks());
            };
            
        }
    }
}