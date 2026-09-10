using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static CaeliImperium.CaeliImperiumContent.Items;
using RoR2.ExpansionManagement;
using RoR2.ContentManagement;
using CaeliImperium.Items;
using CaeliImperium.NetworkMessages;
using R2API;
using System;
using CaeliImperium.Bodies;
using RoR2.Skills;
using CaeliImperiumEntityStates.Test;
using CaeliImperium.Interactables;
using CaeliImperium.Stages;
using CaeliImperium.Components;

namespace CaeliImperium
{
    public static class CaeliImperiumAssets
    {
        public static AssetBundle assetBundle;
        public static GameObject PlayerCharacterMasterControllerPrefab;
        public static GameObject ClassicRunPrefab;
        public static GameObject IgniteOnKillExplosion;
        public static GameObject MercMaster;
        public static GameObject SimpleDotCrosshair;
        public static CharacterCameraParams StandardCharacterCameraParams;
        public static CharacterCameraParams StandardHugeCharacterCameraParams;
        public static GameObject GenericFootstepDust;
        public static GameObject GenericHugeFootstepDust;
        public static GameObject GenericLargeFootstepDust;
        public static DeployableSlot mercenaryGhostDeployable;
        public static GameObject EquipmentPicker;
        public static GameObject EquipmentPickerSlot;
        public static GameObject SpeedPathPrefab;
        public static GameObject GlobalSpeedPathPrefab;
        public static GameObject SpeedPathEndPrefab;
        public static GameObject LassoJoint;
        public static GameObject LassoEffect;
        public static EffectDef SuperSecretScreamEffect;
        public static SkillDef LassoTestSkill;
        public static ItemTag CannotbeCraftedFromMonsterChest;
        public static DccsPool ArenaMonstersDccsPool;
        public static Material Predict;
        public static Material DoesntPredict;
        public static SceneCollection Stage1;
        public static SceneCollection Stage2;
        public static SceneCollection Stage3;
        public static SceneCollection Stage4;
        public static SceneCollection Stage5;
        public static SceneCollection LoopStage1;
        public static SceneCollection LoopStage2;
        public static SceneCollection LoopStage3;
        public static SceneCollection LoopStage4;
        public static SceneCollection LoopStage5;
        public static SceneDef TitanicPlains;
        public static SurfaceDef MetalSurface;
        public static GameObject GlitchHUD;
        public static GameObject VoiceMessage;
        public static Action<Material> onMaterialFound;
        public static void Init()
        {
            assetBundle = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(CaeliImperiumPlugin.PluginInfo.Location), "assetbundles", "caeliimperiumassets")).assetBundle;
            SoundAPI.SoundBanks.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(CaeliImperiumPlugin.PluginInfo.Location), "soundbanks", "CaeliImperiumBank.bnk"));
            CaeliImperiumUtils.AddMusic("Play_CaeliImperiumMusicSystem", "CaeliImperiumMusic");
            PlayerCharacterMasterControllerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Core/PlayerMaster.prefab").WaitForCompletion();
            PlayerCharacterMasterControllerPrefab.AddComponent<ClientToServerMessenger>();
            ClassicRunPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClassicRun/ClassicRun.prefab").WaitForCompletion();
            ClassicRunPrefab.AddComponent<CaeliImperiumExpansionRunComponent>();
            ArenaMonstersDccsPool = Addressables.LoadAssetAsync<DccsPool>("RoR2/Base/arena/dpArenaMonsters.asset").WaitForCompletion();
            SimpleDotCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion();
            IgniteOnKillExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/IgniteOnKill/IgniteExplosionVFX.prefab").WaitForCompletion();
            StandardCharacterCameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandard.asset").WaitForCompletion();
            StandardHugeCharacterCameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandardHuge.asset").WaitForCompletion();
            MercMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercMonsterMaster.prefab").WaitForCompletion();
            GenericFootstepDust = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
            GenericHugeFootstepDust = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericHugeFootstepDust.prefab").WaitForCompletion();
            GenericLargeFootstepDust = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericLargeFootstepDust.prefab").WaitForCompletion();
            Stage1 = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/sgStage1.asset").WaitForCompletion();
            Stage2 = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/sgStage2.asset").WaitForCompletion();
            Stage3 = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/sgStage3.asset").WaitForCompletion();
            Stage4 = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/sgStage4.asset").WaitForCompletion();
            Stage5 = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/sgStage5.asset").WaitForCompletion();
            LoopStage1 = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/loopSgStage1.asset").WaitForCompletion();
            LoopStage2 = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/loopSgStage2.asset").WaitForCompletion();
            LoopStage3 = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/loopSgStage3.asset").WaitForCompletion();
            LoopStage4 = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/loopSgStage4.asset").WaitForCompletion();
            LoopStage5 = Addressables.LoadAssetAsync<SceneCollection>("RoR2/Base/SceneGroups/loopSgStage5.asset").WaitForCompletion();
            TitanicPlains = Addressables.LoadAssetAsync<SceneDef>("RoR2/Base/golemplains/golemplains.asset").WaitForCompletion();
            MetalSurface = Addressables.LoadAssetAsync<SurfaceDef>("RoR2/Base/Common/sdMetal.asset").WaitForCompletion();
            GlitchHUD = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Prefabs/GlitchHUD.prefab");
            VoiceMessage = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Prefabs/VoiceMessage.prefab");
            SuperSecretScreamEffect = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Effects/SuperSecretScreamEffect.prefab").RegisterEffect();
            DrawSpeedPath = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/DrawSpeedPath.asset").RegisterItemDef(DrawSpeedPathEvents.Init);
            HealReceivedDamage = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/HealReceivedDamage.asset").RegisterItemDef(HealReceivedDamageEvents.Init);
            InfiniteSecondarySkillCharges = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/InfiniteSecondarySkillCharges.asset").RegisterItemDef(InfiniteSecondarySkillChargesEvents.Init);
            //BribeEnemiesAndBuffMinions = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/BribeEnemiesAndBuffMinions.asset").RegisterItemDef(BribeEnemiesAndBuffMinionsEvents.Init);
            //InflictIrradiatedOnHit = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/InflictIrradiatedOnHit.asset").RegisterItemDef(InflictIrradiatedOnHitEvents.Init);
            BomberWisp2Events.BodyPrefab = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/BomberWisp2/CIBomberWisp2Body.prefab").RegisterBody(BomberWisp2Events.Init, Configs.BomberWispConfigs.sectionName);
            //ClaySwordsmanEvents.BodyPrefab = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/ClaySwordsman/CIClaySwordsmanBody.prefab").RegisterBody(ClaySwordsmanEvents.Init);
            //VictorEvents.BodyPrefab = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Victor/Character/CIVictorBody.prefab").RegisterBody(VictorEvents.Init);
            CannotbeCraftedFromMonsterChest = ItemAPI.AddItemTag("CannotbeCraftedFromMonsterChest");
            MonsterChestEvents.MonsterChest = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/MonsterChest/MonsterChest.prefab").RegisterNetworkPrefab(MonsterChestEvents.Init, Configs.MonsterChestConfigs.sectionName);
            PipelineRefineryEvents.PipelineRefinery = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/PipelineRefinery/Refinery/PipelineRefinery.prefab").RegisterNetworkPrefab(PipelineRefineryEvents.Init);
            //GooseRunnerEvents.sceneDef = assetBundle.LoadAsset<SceneDef>("Assets/CaeliImperium/Stages/GooseRunner/ci_gooserunner.asset").RegisterSceneDef(GooseRunnerEvents.Init);
            CaeliImperiumPlugin.expansionDef = assetBundle.LoadAsset<ExpansionDef>("Assets/CaeliImperium/CaeliImperiumExpansion.asset").RegisterExpansionDef();
            CaeliImperiumPlugin.expansionDef.disabledIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png").WaitForCompletion();
            CaeliImperiumPlugin.expansionDef.runBehaviorPrefab = null;
            R2API.Networking.NetworkingAPI.RegisterMessageType<HealReceivedDamageHealRateReportMessage>();
            //R2API.Networking.NetworkingAPI.RegisterMessageType<VictorChargeMessage>();
            //R2API.Networking.NetworkingAPI.RegisterMessageType<VictorDamageDealtMessage>();
            foreach (Material material in assetBundle.LoadAllAssets<Material>())
            {
                onMaterialFound?.Invoke(material);
                if (material.name == "matDoesntPredict") DoesntPredict = material;
                if (material.name == "matPredict") Predict = material;
                if (!material.shader.name.StartsWith("StubbedRoR2"))
                {
                    continue;
                }
                string shaderName = material.shader.name.Replace("StubbedRoR2", "RoR2") + ".shader";
                Shader replacementShader = Addressables.LoadAssetAsync<Shader>(shaderName).WaitForCompletion();
                if (replacementShader)
                {
                    int renderPath = material.renderQueue;
                    material.shader = replacementShader;
                    material.renderQueue = renderPath;
                }
            }
            ContentManager.collectContentPackProviders += (addContentPackProvider) =>
            {
                addContentPackProvider(new CaeliImperiumContent());
            };
        }
        
    }
    public enum BaseFootstepDustType
    {
        Generic,
        GenericHuge,
        GenericLarge
    }
    public enum BaseCCPType
    {
        Standard,
        StandardHuge
    }
    public enum BaseCrosshairType
    {
        SimpleDot
    }
}
