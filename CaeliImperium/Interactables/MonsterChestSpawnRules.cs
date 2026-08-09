using BepInEx;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CaeliImperium.Interactables
{
    public class MonsterChestSpawnRules
    {
        private static MonsterChestSpawnRules _default;
        public static MonsterChestSpawnRules Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new MonsterChestSpawnRules();
                    _default.spawnRules = new SpawnRule[] {
                    new SpawnRule
                    {
                        useStageCount = true,
                        stageCount = 3,
                        allowedSceneTypes = new SceneType[] {
                            SceneType.Stage
                        },
                        spawnChance = 100f,
                        spawnCount = 1,
                    }
                };
                }
                return Clone(_default);
            }
        }
        public static MonsterChestSpawnRules Clone(MonsterChestSpawnRules monsterChestSpawnRules)
        {
            return new MonsterChestSpawnRules
            {
                spawnRules = monsterChestSpawnRules.spawnRules
            };
        }
        public SpawnRule[] spawnRules;
        public struct SpawnRule
        {
            public bool useStageName;
            public string stageName;
            public bool useStageCount;
            public int stageCount;
            public int spawnCount;
            public float spawnChance;
            public SceneType[] allowedSceneTypes;
        }
        public XDocument ToXml() => ToXml(this);
        public static XDocument ToXml(MonsterChestSpawnRules monsterChestSpawnRules)
        {
            List<object> list = [];
            XElement xelement = new XElement("SpawnRules");
            list.Add(xelement);
            if (monsterChestSpawnRules.spawnRules != null)
                foreach (MonsterChestSpawnRules.SpawnRule spawnRule in monsterChestSpawnRules.spawnRules)
                {
                    XElement xelement1 = new XElement("SpawnRule");
                    xelement.Add(xelement1);
                    XElement xelement2 = new XElement("UseStageName", spawnRule.useStageName);
                    xelement1.Add(xelement2);
                    XElement xelement3 = new XElement("StageName", spawnRule.stageName.IsNullOrWhiteSpace() ? "" : spawnRule.stageName);
                    xelement1.Add(xelement3);
                    XElement xelement4 = new XElement("UseStageCount", spawnRule.useStageCount);
                    xelement1.Add(xelement4);
                    XElement xelement5 = new XElement("StageCount", spawnRule.stageCount);
                    xelement1.Add(xelement5);
                    XElement xelement8 = new XElement("SpawnCount", spawnRule.spawnCount);
                    xelement1.Add(xelement8);
                    XElement xelement9 = new XElement("SpawnChance", spawnRule.spawnChance);
                    xelement1.Add(xelement9);
                    XElement xelement6 = new XElement("AllowedSceneTypes");
                    foreach (SceneType sceneType in spawnRule.allowedSceneTypes)
                    {
                        XElement xelement7 = new XElement("SceneType", Enum.GetName(typeof(SceneType), sceneType));
                        xelement6.Add(xelement7);
                    }
                    xelement1.Add(xelement6);
                }
            return new XDocument(list.ToArray());
        }
        public static MonsterChestSpawnRules FromXml(XDocument xDocument)
        {
            try
            {
                MonsterChestSpawnRules monsterChestSpawnRules = new MonsterChestSpawnRules();
                XElement root = xDocument.Root;
                List<MonsterChestSpawnRules.SpawnRule> spawnRules = [];
                foreach (XElement xElement in root.Elements("SpawnRule"))
                {
                    MonsterChestSpawnRules.SpawnRule spawnRule = new MonsterChestSpawnRules.SpawnRule();
                    XElement xElement1 = xElement.Element("UseStageName");
                    if (xElement1 != null && bool.TryParse(xElement1.Value, out bool useStageName)) spawnRule.useStageName = useStageName;
                    XElement xElement2 = xElement.Element("StageName");
                    if (xElement2 != null) spawnRule.stageName = xElement2.Value;
                    XElement xElement3 = xElement.Element("UseStageCount");
                    if (xElement3 != null && bool.TryParse(xElement3.Value, out bool useStageCount)) spawnRule.useStageCount = useStageCount;
                    XElement xElement4 = xElement.Element("StageCount");
                    if (xElement4 != null && int.TryParse(xElement4.Value, out int stageCount)) spawnRule.stageCount = stageCount;
                    XElement xElement7 = xElement.Element("SpawnCount");
                    if (xElement7 != null && int.TryParse(xElement7.Value, out int spawnCount)) spawnRule.spawnCount = spawnCount;
                    XElement xElement8 = xElement.Element("SpawnChance");
                    if (xElement8 != null && float.TryParse(xElement8.Value, out float spawnChance)) spawnRule.spawnChance = spawnChance;
                    XElement xElement5 = xElement.Element("AllowedSceneTypes");
                    if (xElement5 != null)
                    {
                        List<SceneType> sceneTypes = [];
                        foreach (XElement xElement6 in xElement5.Elements("SceneType"))
                        {
                            if (Enum.TryParse(typeof(SceneType), xElement6.Value, false, out object sceneType)) sceneTypes.Add((SceneType)sceneType);
                        }
                        spawnRule.allowedSceneTypes = sceneTypes.ToArray();
                    }
                    spawnRules.Add(spawnRule);
                }
                monsterChestSpawnRules.spawnRules = spawnRules.ToArray();
                return monsterChestSpawnRules;
            }
            catch (Exception e)
            {
                CaeliImperiumPlugin.Log.LogError(e);
                return MonsterChestSpawnRules.Default;
            }
        }
    }
}
