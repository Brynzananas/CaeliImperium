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
        public static GameObject stunEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade");
        public static GameObject childProjectileGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Child/FrolicProjectileGhost.prefab").WaitForCompletion();
        public static GameObject chestModel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Chest1/mdlChest1.fbx").WaitForCompletion();
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
        public static Action<Material> onMaterialFound;
        public static void Init()
        {
            assetBundle = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(CaeliImperiumPlugin.PluginInfo.Location), "assetbundles", "caeliimperiumassets")).assetBundle;
            SoundAPI.SoundBanks.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(CaeliImperiumPlugin.PluginInfo.Location), "soundbanks", "CaeliImperiumBank.bnk"));
            EquipmentPicker = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Prefabs/EquipmentPicker.prefab");
            EquipmentPickerSlot = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Prefabs/EquipmentPickerSlot.prefab");
            SpeedPathPrefab = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Prefabs/SpeedPath.prefab");
            GlobalSpeedPathPrefab = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Prefabs/GlobalSpeedPath.prefab");
            SpeedPathEndPrefab = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Effects/ChalkEnd.prefab");
            LassoJoint = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Prefabs/LassoJoint.prefab");
            LassoEffect = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Prefabs/LassoEffect.prefab");
            LassoTestSkill = assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Skills/LassoTest.asset").RegisterSkillDef();
            SuperSecretScreamEffect = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Effects/SuperSecretScreamEffect.prefab").RegisterEffect();
            typeof(LassoTest).RegisterEntityState();
            /*ChargeAtomicBeamOnSpecialSkill = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/ChargeAtomicBeamOnSpecialSkill.asset").RegisterItemDef(ChargeAtomicBeamOnSpecialSkillEvents);
            CopyNearbyCharactersSkillsOnDeath = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/CopyNearbyCharactersSkillsOnDeath.asset").RegisterItemDef(CopyNearbyCharactersSkillsOnDeathEvents);
            CritUpgradeOnKill = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/CritUpgradeOnKill.asset").RegisterItemDef(CritUpgradeOnKillEvents);
            ShareDamageToAll = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/DamageAllEnemies.asset").RegisterItemDef(ShareDamageToAllEvents);
            DropHealOrbsOnContiniousHits = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/DropHealOrbsOnContiniousHits.asset").RegisterItemDef(TaoEvents);
            DuplicateMainSkills = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/DuplicateMainSkills.asset").RegisterItemDef(DuplicateMainSkillsEvents);
            ExtraEquipmentSlot = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/ExtraEquipmentSlot.asset").RegisterItemDef(ExtraEquipmentSlotEvents);
            FireBulletOnPrimarySkill = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/FireBulletOnPrimarySkill.asset").RegisterItemDef(FireBulletOnPrimarySkillEvents);
            ImproveHealingAndRegen = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/ImproveHealingAndRegen.asset").RegisterItemDef(ImproveHealingAndRegenEvents);
            PeriodicDamageIncrease = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/PeriodicDamageIncrease.asset").RegisterItemDef(PeriodicDamageIncreaseEvents);
            StunEnemyOnItsAttack = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/StunEnemyOnItsAttack.asset").RegisterItemDef(StunEnemyOnItsAttackEvents);
            SummonMercenary = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/SummonMercenary.asset").RegisterItemDef(SummonMercenaryEvents);
            TeleportAroundOpenedChests = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/TeleportAroundOpenedChests.asset").RegisterItemDef(TeleportAroundOpenedChestsEvents);
            TransferDamageOwnership = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/TransferDamageOwnership.asset").RegisterItemDef(TransferDamageOwnershipEvents);
            WoundEnemyOnContiniousHits = assetBundle.LoadAsset<ItemDef>("Assets/CaeliImperium/Items/WoundEnemyOnContiniousHits.asset").RegisterItemDef(TaoEvents);
            AffixSpeedster = assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/AffixSpeedster.asset").RegisterBuffDef();
            IncreaseCritChanceAndDamage = assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/IncreaseCritChanceAndCritDamage.asset").RegisterBuffDef();
            IncreaseDamagePereodically = assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/IncreaseDamagePeriodically.asset").RegisterBuffDef();
            IrradiatedBuff = assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/Irradiated.asset").RegisterBuffDef();
            IrradiatedDot = Utils.CreateDOT(IrradiatedBuff, out IrradiatedDotIndex, false, 0.5f, 0.5f, DamageColorIndex.Item, IrradiatedDotBehaviour, null, IrradiatedDotEvaluation);
            TaoPunchCooldown = assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/TaoChargeCooldown.asset").RegisterBuffDef();
            TaoPunchReady = assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/TaoChargeReady.asset").RegisterBuffDef();
            Necronomicon = assetBundle.LoadAsset<EquipmentDef>("Assets/CaeliImperium/Equipments/Necronomicon.asset").RegisterEquipmentDef(NecronomiconEvents);
            SpeedsterEquipment = assetBundle.LoadAsset<EquipmentDef>("Assets/CaeliImperium/Equipments/SpeedsterEquipment.asset").RegisterEquipmentDef();
            HastingElite = assetBundle.LoadAsset<EliteDef>("Assets/CaeliImperium/Elites/Speedster.asset").RegisterEliteDef(HastingEvents);*/
            DrawSpeedPath = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/DrawSpeedPath.asset").RegisterItemDef(DrawSpeedPathEvents.Init);
            HealReceivedDamage = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/HealReceivedDamage.asset").RegisterItemDef(HealReceivedDamageEvents.Init);
            InfiniteSecondarySkillCharges = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/InfiniteSecondarySkillCharges.asset").RegisterItemDef(InfiniteSecondarySkillChargesEvents.Init);
            //BribeEnemiesAndBuffMinions = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/BribeEnemiesAndBuffMinions.asset").RegisterItemDef(BribeEnemiesAndBuffMinionsEvents.Init);
            //InflictIrradiatedOnHit = assetBundle.LoadAsset<CIItemDef>("Assets/CaeliImperium/Items/InflictIrradiatedOnHit.asset").RegisterItemDef(InflictIrradiatedOnHitEvents.Init);
            BomberWisp2Events.BodyPrefab = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/BomberWisp2/CIBomberWisp2Body.prefab").RegisterBody(BomberWisp2Events.Init);
            //VictorEvents.BodyPrefab = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/Victor/Character/CIVictorBody.prefab").RegisterBody(VictorEvents.Init);
            CannotbeCraftedFromMonsterChest = ItemAPI.AddItemTag("CannotbeCraftedFromMonsterChest");
            MonsterChestEvents.MonsterChest = assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Interactables/MonsterChest/MonsterChest.prefab").RegisterNetworkPrefab(MonsterChestEvents.Init);
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
            //mercenaryGhostDeployable = DeployableAPI.RegisterDeployableSlot(GetMercenaryDeployableSlot);
            //int GetMercenaryDeployableSlot(CharacterMaster self, int deployableSlotMultiplier)
            //{
            //    return deployableSlotMultiplier;
            //}
            ContentManager.collectContentPackProviders += (addContentPackProvider) =>
            {
                addContentPackProvider(new CaeliImperiumContent());
            };
        }
    }
}
