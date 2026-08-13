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

namespace CaeliImperium
{
    public static class CaeliImperiumAssets
    {
        public static AssetBundle assetBundle;
        public static EntityStateConfiguration fireSnipeHeavyConfig = Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/DLC1/Railgunner/EntityStates.Railgunner.Weapon.FireSnipeHeavy.asset").WaitForCompletion();
        public static GameObject fireSnipeSuperTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunSuper.prefab").WaitForCompletion();
        public static GameObject igniteOnkillExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/IgniteOnKill/IgniteExplosionVFX.prefab").WaitForCompletion();
        public static GameObject mercMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercMonsterMaster.prefab").WaitForCompletion();
        public static GameObject defaultCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion();
        public static CharacterCameraParams defaultCharacterCameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandard.asset").WaitForCompletion();
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
        public static Action<Material> onMaterialFound;
        public static void Init()
        {
            assetBundle = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(CaeliImperiumPlugin.PluginInfo.Location), "assetbundles", "caeliimperiumassets")).assetBundle;
            SoundAPI.SoundBanks.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(CaeliImperiumPlugin.PluginInfo.Location), "soundbanks", "CaeliImperiumBank.bnk"));
            ArenaMonstersDccsPool = Addressables.LoadAssetAsync<DccsPool>("RoR2/Base/arena/dpArenaMonsters.asset").WaitForCompletion();
            SuperSecretScreamEffect = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Effects/SuperSecretScreamEffect.prefab").RegisterEffect();
            DrawSpeedPath = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/DrawSpeedPath.asset").RegisterItemDef(DrawSpeedPathEvents.Init);
            HealReceivedDamage = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/HealReceivedDamage.asset").RegisterItemDef(HealReceivedDamageEvents.Init);
            InfiniteSecondarySkillCharges = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/InfiniteSecondarySkillCharges.asset").RegisterItemDef(InfiniteSecondarySkillChargesEvents.Init);
            //BribeEnemiesAndBuffMinions = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/BribeEnemiesAndBuffMinions.asset").RegisterItemDef(BribeEnemiesAndBuffMinionsEvents.Init);
            //InflictIrradiatedOnHit = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/InflictIrradiatedOnHit.asset").RegisterItemDef(InflictIrradiatedOnHitEvents.Init);
            BomberWisp2Events.BodyPrefab = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/BomberWisp2/CIBomberWisp2Body.prefab").RegisterBody(BomberWisp2Events.Init, Configs.BomberWispConfigs.sectionName);
            //VictorEvents.BodyPrefab = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Victor/Character/CIVictorBody.prefab").RegisterBody(VictorEvents.Init);
            CannotbeCraftedFromMonsterChest = ItemAPI.AddItemTag("CannotbeCraftedFromMonsterChest");
            MonsterChestEvents.MonsterChest = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/MonsterChest/MonsterChest.prefab").RegisterNetworkPrefab(MonsterChestEvents.Init, Configs.MonsterChestConfigs.sectionName);
            //PipelineRefineryEvents.PipelineRefinery = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/PipelineRefinery/PipelineRefinery.prefab").RegisterNetworkPrefab(PipelineRefineryEvents.Init);
            CaeliImperiumPlugin.expansionDef = assetBundle.LoadAsset<ExpansionDef>("Assets/CaeliImperium/CaeliImperiumExpansion.asset").RegisterExpansionDef();
            CaeliImperiumPlugin.expansionDef.disabledIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png").WaitForCompletion();
            CaeliImperiumPlugin.expansionDef.runBehaviorPrefab.RegisterNetworkPrefab();
            R2API.Networking.NetworkingAPI.RegisterMessageType<HealReceivedDamageHealRateReportMessage>();
            //R2API.Networking.NetworkingAPI.RegisterMessageType<VictorChargeMessage>();
            //R2API.Networking.NetworkingAPI.RegisterMessageType<VictorDamageDealtMessage>();
            foreach (Material material in assetBundle.LoadAllAssets<Material>())
            {
                onMaterialFound?.Invoke(material);
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
}
