using BepInEx;
using BrynzaAPI;
using CaeliImperium.Configs;
using CaeliImperiumEntityStates.BomberWisp;
using R2API;
using R2API.SpawnCardCloning;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace CaeliImperium.Bodies
{
    public static class BomberWisp2Events
    {
        public static GameObject BodyPrefab;
        public static CharacterBody Body;
        public static GameObject MasterPrefab;
        public static SkillFamily Primary;
        public static SkillDef SpawnPillar;
        public static GameObject BombProjectile;
        public static ProjectileImpactCapsuleExplosion projectileImpactCapsuleExplosion;
        public static int BombProjectileIndex;
        public static EffectDef Explosion;
        public static EffectDef Charging;
        public static CharacterSpawnCardClone characterSpawnCardClone;
        internal static bool inited;

        public static void Init(GameObject gameObject)
        {
            CaeliImperiumPlugin.onPluginDestroyed += OnPluginDestroyed;
            CaeliImperiumHooks.OnSetProjectilePrefabsIndividualPrefab += Hooks_OnSetProjectilePrefabsIndividualPrefab;
            Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            if (inited) return;
            BodyPrefab = gameObject;
            inited = true;
            Body = CaeliImperiumUtils.HandleBody(gameObject);
            MasterPrefab = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/BomberWisp2/CIBomberWisp2Master.prefab").RegisterMaster();
            Primary = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/BomberWisp2/CIBomberWisp2Primary.asset").RegisterSkillFamily();
            SpawnPillar = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/BomberWisp2/CIBomberWisp2SpawnPillar.asset").RegisterSkillDef();
            BombProjectile = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/BomberWisp2/CIBomberWisp2Bomb.prefab").RegisterProjectile();
            projectileImpactCapsuleExplosion = BombProjectile.GetComponent<ProjectileImpactCapsuleExplosion>();
            ProjectileController projectileController = BombProjectile.GetComponent<ProjectileController>();
            PostProcessVolume postProcessVolume = projectileController.ghostPrefab.transform.Find("indicator/PP").GetComponent<PostProcessVolume>();
            BombProjectileIndex = projectileController.catalogIndex;
            postProcessVolume.sharedProfile = Addressables.LoadAssetAsync<PostProcessProfile>("RoR2/Base/title/PostProcessing/ppLocalGrandparent.asset").WaitForCompletion();
            Explosion = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/BomberWisp/BomberWispBombExplosion.prefab").RegisterEffect();
            Charging = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/BomberWisp/BomberWispCharge.prefab").RegisterEffect();
            characterSpawnCardClone = CaeliImperiumAssets.assetBundle.LoadAsset<CharacterSpawnCardClone>("Assets/CaeliImperium/Bodies/BomberWisp2/csccBomberWisp2.asset");
            characterSpawnCardClone.customCondition = CustomCondition;
            characterSpawnCardClone.Register();
            typeof(ChargeBomb).RegisterEntityState();
            typeof(SpawnState).RegisterEntityState();
            typeof(SpawnPillar).RegisterEntityState();
            typeof(FireBomb).RegisterEntityState();
            typeof(DeathState).RegisterEntityState();
            typeof(SpecialBomberWispMainState).RegisterEntityState();
        }

        private static void Stage_onStageStartGlobal(Stage obj)
        {
            if (!Run.instance || BomberWispConfigs.bomberWispStageRules == null || BomberWispConfigs.bomberWispStageRules.stageRules == null || BomberWispConfigs.bomberWispStageRules.stageRules.Length == 0 || !projectileImpactCapsuleExplosion) return;
            Scene scene = SceneManager.GetActiveScene();
            BomberWispStageRules.StageRule stageRule = BomberWispConfigs.bomberWispStageRules.GetStageRule(Run.instance.stageClearCount, scene == null ? null : scene.name);
            projectileImpactCapsuleExplosion.blastRadius = stageRule.blastRadius;
            projectileImpactCapsuleExplosion.blastDamageCoefficient = stageRule.blastDamageCoefficient;
            projectileImpactCapsuleExplosion.lifetime = stageRule.detonationTime;
        }

        private static void OnPluginDestroyed()
        {
            CaeliImperiumPlugin.onPluginDestroyed -= OnPluginDestroyed;
            CaeliImperiumHooks.OnSetProjectilePrefabsIndividualPrefab -= Hooks_OnSetProjectilePrefabsIndividualPrefab;
            Stage.onStageStartGlobal -= Stage_onStageStartGlobal;
        }
        private static void Hooks_OnSetProjectilePrefabsIndividualPrefab(ProjectileController obj)
        {
            if (obj.gameObject == BombProjectile.gameObject) BombProjectileIndex = obj.catalogIndex;
        }
        public static bool CustomCondition(RebuildCardsInfo rebuildCardsInfo)
        {
            if (!BomberWispConfigs.SpawnInArena.Value && rebuildCardsInfo.dccsPool == CaeliImperiumAssets.ArenaMonstersDccsPool) return false;
            return true;
        }
    }
}
