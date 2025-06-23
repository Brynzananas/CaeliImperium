using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CaeliImperium
{
    public class Assets
    {
        public static GameObject stunEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade");
        public static GameObject childProjectileGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Child/FrolicProjectileGhost.prefab").WaitForCompletion();
        public static GameObject chestModel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Chest1/mdlChest1.fbx").WaitForCompletion();
        public static EntityStateConfiguration fireSnipeHeavyConfig = Addressables.LoadAssetAsync<EntityStateConfiguration>("RoR2/DLC1/Railgunner/EntityStates.Railgunner.Weapon.FireSnipeHeavy.asset").WaitForCompletion();
        public static GameObject fireSnipeSuperTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunSuper.prefab").WaitForCompletion();
        public static GameObject igniteOnkillExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/IgniteOnKill/IgniteExplosionVFX.prefab").WaitForCompletion();
        public static GameObject mercMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercMonsterMaster.prefab").WaitForCompletion();
    }
}
