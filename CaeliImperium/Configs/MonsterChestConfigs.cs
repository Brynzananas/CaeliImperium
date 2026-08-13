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
        public const string sectionName = "Interactables: Monster Chest";
        public static InteractableSpawnRules monsterChestSpawnRules;
        public static void Init()
        {
            MonsterChestSpawnRules = CaeliImperiumUtils.CreateConfig(sectionName, "Spawn Rules", Interactables.InteractableSpawnRules.Default.ToXml().ConvertToString(), "");
            UpdateMonsterChestSpawnRules();
            MonsterChestSpawnRules.SettingChanged += UpdateMonsterChestSpawnRules;
            MonsterChestCanSpewMessage = CaeliImperiumUtils.CreateConfig(sectionName, "Can spew message", true, "", false);
            MonsterChestHighlight = CaeliImperiumUtils.CreateConfig(sectionName, "Highlight", false, "", false);
            MonsterChestNeededSacrifices = CaeliImperiumUtils.CreateConfig(sectionName, "Needed Amount of Sacrifices", 8, "");
            MonsterChestSacrificeValueForTier1 = CaeliImperiumUtils.CreateConfig(sectionName, "Sacrifices Value from Common Item", 1, "");
            MonsterChestSacrificeValueForTier2 = CaeliImperiumUtils.CreateConfig(sectionName, "Sacrifices Value from Rare Item", 2, "");
            MonsterChestSacrificeValueForTier3 = CaeliImperiumUtils.CreateConfig(sectionName, "Sacrifices Value from Legendary Item", 4, "");
            MonsterChestSacrificeValueForTierBoss = CaeliImperiumUtils.CreateConfig(sectionName, "Sacrifices Value from Boss Item", 8, "");
        }

        private static void UpdateMonsterChestSpawnRules(object sender, EventArgs e) => UpdateMonsterChestSpawnRules();
        public static void UpdateMonsterChestSpawnRules()
        {
            XDocument xDocument = MonsterChestSpawnRules.Value.ConvertToXDocument(true);
            if (xDocument == null)
            {
                monsterChestSpawnRules = Interactables.InteractableSpawnRules.Default;
            }
            else
            {
                monsterChestSpawnRules = Interactables.InteractableSpawnRules.FromXml(xDocument);
                if (monsterChestSpawnRules == null) monsterChestSpawnRules = Interactables.InteractableSpawnRules.Default;
            }
        }
        private static void SettingChanged(object sender, System.EventArgs e) => CaeliImperiumLanguage.InitMonsterChest();
        public static ConfigEntry<string> MonsterChestSpawnRules;
        public static ConfigEntry<bool> MonsterChestCanSpewMessage;
        public static ConfigEntry<bool> MonsterChestHighlight;
        public static ConfigEntry<int> MonsterChestNeededSacrifices;
        public static ConfigEntry<int> MonsterChestSacrificeValueForTier1;
        public static ConfigEntry<int> MonsterChestSacrificeValueForTier2;
        public static ConfigEntry<int> MonsterChestSacrificeValueForTier3;
        public static ConfigEntry<int> MonsterChestSacrificeValueForTierBoss;
    }
}
