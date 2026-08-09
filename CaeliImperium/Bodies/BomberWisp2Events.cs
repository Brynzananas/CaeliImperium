using BepInEx;
using CaeliImperiumEntityStates.BomberWisp;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.PostProcessing;

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
        public static int BombProjectileIndex;
        public static EffectDef Explosion;
        public static EffectDef Charging;
        public static BrynzaAPI.CharacterSpawnCardMirror spawnCardMirror;
        public static R2API.SpawnCardCloning.CharacterSpawnCardClone characterSpawnCardClone;
        internal static bool inited;

        public static void Init(GameObject gameObject)
        {
            CaeliImperiumPlugin.onPluginDestroyed += OnPluginDestroyed;
            CaeliImperiumHooks.OnSetProjectilePrefabsIndividualPrefab += Hooks_OnSetProjectilePrefabsIndividualPrefab;
            if (inited) return;
            BodyPrefab = gameObject;
            /*var d = new SoundAPI.Music.CustomMusicData();
            d.BanksFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(CaeliImperiumPlugin.PluginInfo.Location), "soundbanks");
            d.BepInPlugin = CaeliImperiumPlugin.PluginInfo.Metadata;
            d.InitBankName = "CaeliImperiumMusicInit.bnk";
            d.PlayMusicSystemEventName = "Play_MIOProjectPart1";
            d.SoundBankName = "CaeliImperiumMusicBank.bnk";
            SoundAPI.Music.Add(d);*/
            inited = true;
            Body = CaeliImperiumUtils.HandleBody(gameObject);
            MasterPrefab = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/BomberWisp2/CIBomberWisp2Master.prefab").RegisterMaster();
            Primary = CaeliImperiumAssets.assetBundle.LoadAsset<SkillFamily>("Assets/CaeliImperium/Bodies/BomberWisp2/CIBomberWisp2Primary.asset").RegisterSkillFamily();
            SpawnPillar = CaeliImperiumAssets.assetBundle.LoadAsset<SkillDef>("Assets/CaeliImperium/Bodies/BomberWisp2/CIBomberWisp2SpawnPillar.asset").RegisterSkillDef();
            BombProjectile = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/BomberWisp2/CIBomberWisp2Bomb.prefab").RegisterProjectile();
            ProjectileController projectileController = BombProjectile.GetComponent<ProjectileController>();
            PostProcessVolume postProcessVolume = projectileController.ghostPrefab.transform.Find("indicator/PP").GetComponent<PostProcessVolume>();
            BombProjectileIndex = projectileController.catalogIndex;
            postProcessVolume.sharedProfile = Addressables.LoadAssetAsync<PostProcessProfile>("RoR2/Base/title/PostProcessing/ppLocalGrandparent.asset").WaitForCompletion();
            Explosion = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/BomberWisp/BomberWispBombExplosion.prefab").RegisterEffect();
            Charging = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Bodies/BomberWisp/BomberWispCharge.prefab").RegisterEffect();
            characterSpawnCardClone = CaeliImperiumAssets.assetBundle.LoadAsset<R2API.SpawnCardCloning.CharacterSpawnCardClone>("Assets/CaeliImperium/Bodies/BomberWisp2/csccBomberWisp2.asset");
            characterSpawnCardClone.Register();
            typeof(ChargeBomb).RegisterEntityState();
            typeof(SpawnState).RegisterEntityState();
            typeof(SpawnPillar).RegisterEntityState();
            typeof(FireBomb).RegisterEntityState();
            typeof(DeathState).RegisterEntityState();
            typeof(SpecialBomberWispMainState).RegisterEntityState();
        }
        private static void OnPluginDestroyed()
        {
            CaeliImperiumPlugin.onPluginDestroyed -= OnPluginDestroyed;
            CaeliImperiumHooks.OnSetProjectilePrefabsIndividualPrefab -= Hooks_OnSetProjectilePrefabsIndividualPrefab;
        }
        private static void Hooks_OnSetProjectilePrefabsIndividualPrefab(ProjectileController obj)
        {
            if (obj.gameObject == BombProjectile.gameObject)
            {
                BombProjectileIndex = obj.catalogIndex;
            }
        }
    }
}
