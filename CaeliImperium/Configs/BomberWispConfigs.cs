using BepInEx.Configuration;
using CaeliImperium.Bodies;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using static CaeliImperium.CaeliImperiumUtils;

namespace CaeliImperium.Configs
{
    public static class BomberWispConfigs
    {
        public const string sectionName = "Monsters: Bomber Wisp";
        public static BomberWispStageRules bomberWispStageRules;
        public static void Init()
        {
            ProjectileStageRules = CreateConfig(sectionName, "Projectile Stage Rules", BomberWispStageRules.Default.ToXml().ConvertToString(), "");
            ProjectileStageRules.SettingChanged += ProjectileStageRules_SettingChanged;
            UpdateBomberWispStageRules();
            SpawnInArena = CreateConfig(sectionName, "Spawn in void fields", false, "Requires restart if void fields are visited once");
            SparksInProjectileGhost = CreateConfig(sectionName, "Projectile sparks", true, "Enable visual sparks?", false);
            LinesInProjectileGhost = CreateConfig(sectionName, "Projectile lines", true, "Enable visual lines?", false);
            SpawnStateDuration = CreateConfig(sectionName, "Spawn duration", 2f, "");
            SpawnPillarDamageCoefficient = CreateConfig(sectionName, "Spawn pillar damage coefficient", 5f, "");
            SpawnPillarProcCoefficient = CreateConfig(sectionName, "Spawn pillar proc coefficient", 1f, "");
            SpawnPillarForce = CreateConfig(sectionName, "Spawn pillar force", 3000f, "");
            SpawnPillarBaseDuration = CreateConfig(sectionName, "Spawn pillar duration", 2f, "");
            SpawnPillarCustomAIAiming = CreateConfig(sectionName, "Spawn pillar custom AI aim", true, "");
            SpawnPillarMinProjectilesForCustomAIAiming = CreateConfig(sectionName, "Spawn pillar minimum existing projectiles for custom AI aim", 1, "");
            DeathStateMinExplosionDamageCoefficient = CreateConfig(sectionName, "Death explosion minimal damage coefficient", 2f, "");
            DeathStateMaxExplosionDamageCoefficient = CreateConfig(sectionName, "Death explosion maximum damage coefficient", 5f, "");
            DeathStateProcCoefficient = CreateConfig(sectionName, "Death explosion proc coefficient", 1f, "");
            DeathStateMinExplosionForce = CreateConfig(sectionName, "Death explosion minimal force", 300f, "");
            DeathStateMaxExplosionForce = CreateConfig(sectionName, "Death explosion maximum force", 3000f, "");
            DeathStateMinRadius = CreateConfig(sectionName, "Death explosion minimum radius", 3f, "");
            DeathStateMaxRadius = CreateConfig(sectionName, "Death explosion maximum radius", 14f, "");
            DeathStateFalloffModel = CreateConfig(sectionName, "Death explosion falloff", BlastAttack.FalloffModel.Linear, "");
        }

        private static void ProjectileStageRules_SettingChanged(object sender, EventArgs e)
        {
            UpdateBomberWispStageRules();
        }

        public static void UpdateBomberWispStageRules()
        {
            XDocument xDocument = ProjectileStageRules.Value.ConvertToXDocument(true);
            if (xDocument == null)
            {
                bomberWispStageRules = BomberWispStageRules.Default;
            }
            else
            {
                bomberWispStageRules = BomberWispStageRules.FromXml(xDocument);
                if (bomberWispStageRules == null) bomberWispStageRules = BomberWispStageRules.Default;
            }
        }
        private static void SettingChanged(object sender, System.EventArgs e) => CaeliImperiumLanguage.InitBomberWisp();
        public static ConfigEntry<string> ProjectileStageRules;
        public static ConfigEntry<bool> SpawnInArena;
        public static ConfigEntry<bool> SparksInProjectileGhost;
        public static ConfigEntry<bool> LinesInProjectileGhost;
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
