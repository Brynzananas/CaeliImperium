using BepInEx.Configuration;
using CaeliImperium.Interactables;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CaeliImperium.Configs
{
    public static class MonsterChestConfigs
    {
        public static MonsterChestSpawnRules monsterChestSpawnRules;
        public static void Init()
        {
            MonsterChestSpawnRules = CaeliImperiumUtils.CreateConfig("Interactables: Monster Chest", "Spawn Rules", Interactables.MonsterChestSpawnRules.Default.ToXml().ConvertToString(), "");
            UpdateMonsterChestSpawnRules();
            MonsterChestSpawnRules.SettingChanged += UpdateMonsterChestSpawnRules;
            MonsterChestNeededSacrifices = CaeliImperiumUtils.CreateConfig("Interactables: Monster Chest", "Needed Amount of Sacrifices", 8, "");
            MonsterChestSacrificeValueForTier1 = CaeliImperiumUtils.CreateConfig("Interactables: Monster Chest", "Sacrifices Value from Common Item", 1, "");
            MonsterChestSacrificeValueForTier2 = CaeliImperiumUtils.CreateConfig("Interactables: Monster Chest", "Sacrifices Value from Rare Item", 2, "");
            MonsterChestSacrificeValueForTier3 = CaeliImperiumUtils.CreateConfig("Interactables: Monster Chest", "Sacrifices Value from Legendary Item", 4, "");
            MonsterChestSacrificeValueForTierBoss = CaeliImperiumUtils.CreateConfig("Interactables: Monster Chest", "Sacrifices Value from Boss Item", 8, "");
        }

        private static void UpdateMonsterChestSpawnRules(object sender, EventArgs e) => UpdateMonsterChestSpawnRules();
        public static void UpdateMonsterChestSpawnRules()
        {
            XDocument xDocument = MonsterChestSpawnRules.Value.ConvertToXDocument(true);
            if (xDocument == null)
            {
                monsterChestSpawnRules = Interactables.MonsterChestSpawnRules.Default;
            }
            else
            {
                monsterChestSpawnRules = Interactables.MonsterChestSpawnRules.FromXml(xDocument);
                if (monsterChestSpawnRules == null) monsterChestSpawnRules = Interactables.MonsterChestSpawnRules.Default;
            }
        }
        private static void SettingChanged(object sender, System.EventArgs e) => CaeliImperiumLanguage.InitMonsterChest();
        public static ConfigEntry<string> MonsterChestSpawnRules;
        public static ConfigEntry<int> MonsterChestNeededSacrifices;
        public static ConfigEntry<int> MonsterChestSacrificeValueForTier1;
        public static ConfigEntry<int> MonsterChestSacrificeValueForTier2;
        public static ConfigEntry<int> MonsterChestSacrificeValueForTier3;
        public static ConfigEntry<int> MonsterChestSacrificeValueForTierBoss;
    }
}
