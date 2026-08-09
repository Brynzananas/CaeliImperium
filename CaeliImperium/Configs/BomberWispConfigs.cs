using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static CaeliImperium.CaeliImperiumUtils;

namespace CaeliImperium.Configs
{
    public class BomberWispConfigs
    {
        public static void Init()
        {
            SpawnStateDuration = CreateConfig("Monsters: Bomber Wisp", "Spawn duration", 2f, "");
            SpawnPillarDamageCoefficient = CreateConfig("Monsters: Bomber Wisp", "Spawn pillar damage coefficient", 5f, "");
            SpawnPillarProcCoefficient = CreateConfig("Monsters: Bomber Wisp", "Spawn pillar proc coefficient", 1f, "");
            SpawnPillarForce = CreateConfig("Monsters: Bomber Wisp", "Spawn pillar force", 3000f, "");
            SpawnPillarBaseDuration = CreateConfig("Monsters: Bomber Wisp", "Spawn pillar duration", 2f, "");
            SpawnPillarCustomAIAiming = CreateConfig("Monsters: Bomber Wisp", "Spawn pillar custom AI aim", true, "");
            SpawnPillarMinProjectilesForCustomAIAiming = CreateConfig("Monsters: Bomber Wisp", "Spawn pillar minimum existing projectiles for custom AI aim", 1, "");
            DeathStateMinExplosionDamageCoefficient = CreateConfig("Monsters: Bomber Wisp", "Death explosion minimal damage coefficient", 2f, "");
            DeathStateMaxExplosionDamageCoefficient = CreateConfig("Monsters: Bomber Wisp", "Death explosion maximum damage coefficient", 5f, "");
            DeathStateProcCoefficient = CreateConfig("Monsters: Bomber Wisp", "Death explosion proc coefficient", 1f, "");
            DeathStateMinExplosionForce = CreateConfig("Monsters: Bomber Wisp", "Death explosion minimal force", 300f, "");
            DeathStateMaxExplosionForce = CreateConfig("Monsters: Bomber Wisp", "Death explosion maximum force", 3000f, "");
            DeathStateMinRadius = CreateConfig("Monsters: Bomber Wisp", "Death explosion minimum radius", 3f, "");
            DeathStateMaxRadius = CreateConfig("Monsters: Bomber Wisp", "Death explosion maximum radius", 14f, "");
            DeathStateFalloffModel = CreateConfig("Monsters: Bomber Wisp", "Death explosion falloff", BlastAttack.FalloffModel.Linear, "");
        }
        private static void SettingChanged(object sender, System.EventArgs e) => CaeliImperiumLanguage.InitBomberWisp();
        public static ConfigEntry<float> ChargeBombDuration;
        public static ConfigEntry<float> DeathStateMinExplosionDamageCoefficient;
        public static ConfigEntry<float> DeathStateMaxExplosionDamageCoefficient;
        public static ConfigEntry<float> DeathStateProcCoefficient;
        public static ConfigEntry<float> DeathStateMinExplosionForce;
        public static ConfigEntry<float> DeathStateMaxExplosionForce;
        public static ConfigEntry<float> DeathStateMinRadius;
        public static ConfigEntry<float> DeathStateMaxRadius;
        public static ConfigEntry<BlastAttack.FalloffModel> DeathStateFalloffModel;
        public static ConfigEntry<float> FireBombDamageCoefficient;
        public static ConfigEntry<float> FireBombProcCoefficient;
        public static ConfigEntry<float> FireBombForce;
        public static ConfigEntry<float> FireBombBaseDuration;
        public static ConfigEntry<float> FireBombProjectileSpeed;
        public static ConfigEntry<float> FireBombTimeToTarget;
        public static ConfigEntry<bool> FireBombCustomAIAiming;
        public static ConfigEntry<int> FireBombMinProjectilesForCustomAIAiming;
        public static ConfigEntry<float> SpawnPillarDamageCoefficient;
        public static ConfigEntry<float> SpawnPillarProcCoefficient;
        public static ConfigEntry<float> SpawnPillarForce;
        public static ConfigEntry<float> SpawnPillarBaseDuration;
        public static ConfigEntry<bool> SpawnPillarCustomAIAiming;
        public static ConfigEntry<int> SpawnPillarMinProjectilesForCustomAIAiming;
        public static ConfigEntry<float> SpawnStateDuration;
    }
}
