
using System.Security.Permissions;
using System.Security;
using BepInEx;
using RoR2.ContentManagement;

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
    //[R2APISubmoduleDependency(nameof(CommandHelper))]
    [System.Serializable]
    public class Main : BaseUnityPlugin
    {
        public const string ModGuid = "com.brynzananas.caeliimperium";
        public const string ModName = "Caeli Imperium";
        public const string ModVer = "1.0.0";
        public const string ModPrefix = "CI";
        
        public void Awake()
        {
            Hooks.Init();
            new Assets();
            new Items();
            new Equipments();
            new Buffs();
            new Elites();
            ContentManager.collectContentPackProviders += (addContentPackProvider) =>
            {
                addContentPackProvider(new ContentPacks());
            };
            
        }
    }
}