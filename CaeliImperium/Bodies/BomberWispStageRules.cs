using BepInEx;
using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace CaeliImperium.Bodies;

public class BomberWispStageRules 
{
    private static BomberWispStageRules _default;
    public static BomberWispStageRules Default
    {
        get
        {
            if (_default == null)
            {
                _default = new BomberWispStageRules();
                _default.stageRules = new StageRule[] {
                new StageRule
                {
                    useStageCount = true,
                    stageCount = 1,
                    blastRadius = 10f,
                    detonationTime = 2.25f,
                    blastDamageCoefficient = 1f
                },
                 new StageRule
                {
                    useStageCount = true,
                    stageCount = 2,
                    blastRadius = 12f,
                    detonationTime = 2f,
                    blastDamageCoefficient = 1.5f
                },
                  new StageRule
                {
                    useStageCount = true,
                    stageCount = 3,
                    blastRadius = 14f,
                    detonationTime = 2f,
                    blastDamageCoefficient = 2f
                },
                   new StageRule
                {
                    useStageCount = true,
                    stageCount = 4,
                    blastRadius = 14f,
                    detonationTime = 1.75f,
                    blastDamageCoefficient = 2.5f
                },
                    new StageRule
                {
                    useStageCount = true,
                    stageCount = 5,
                    blastRadius = 16f,
                    detonationTime = 1.5f,
                    blastDamageCoefficient = 3f
                },
                    new StageRule
                {
                    useStageCount = true,
                    stageCount = 6,
                    blastRadius = 18f,
                    detonationTime = 1f,
                    blastDamageCoefficient = 5f
                }
            };
            }
            return Clone(_default);
        }
    }
    public static BomberWispStageRules Clone(BomberWispStageRules bomberWispStageRules)
    {
        return new BomberWispStageRules
        {
            stageRules = bomberWispStageRules.stageRules
        };
    }
    public StageRule GetStageRule(int stageCount, string stageName)
    {
        int maxSetStageCount = 1;
        foreach (StageRule stageRule in stageRules)
        {
            if (stageRule.stageCount > maxSetStageCount) maxSetStageCount = stageRule.stageCount;
        }
        maxSetStageCount--;
        StageRule stageRule1 = DefaultStageRule;
        int currentStageCount = -1;
        foreach (StageRule stageRule in stageRules)
        {
            if (!stageName.IsNullOrWhiteSpace() && stageRule.useStageName && !stageRule.stageName.IsNullOrWhiteSpace() && stageName == stageRule.stageName)
            {
                stageRule1 = stageRule;
                break;
            }
            int stageRuleStageCount = stageRule.stageCount - 1;
            if (stageRule.useStageCount && stageCount >= stageRuleStageCount && currentStageCount < stageRuleStageCount)
            {
                stageRule1 = stageRule;
                currentStageCount = stageRule.stageCount;
            }
        }
        return stageRule1;
    }
    public StageRule[] stageRules;
    public static StageRule DefaultStageRule = new StageRule
    {
        blastRadius = 12f,
        detonationTime = 2f,
        blastDamageCoefficient = 1f
    };
    public struct StageRule
    {
        public bool useStageName;
        public string stageName;
        public bool useStageCount;
        public int stageCount;
        public float blastRadius;
        public float blastDamageCoefficient;
        public float detonationTime;
    }
    public XDocument ToXml() => ToXml(this);
    public static XDocument ToXml(BomberWispStageRules bomberWispStageRules)
    {
        List<object> list = [];
        XElement xelement = new XElement("StageRules");
        list.Add(xelement);
        if (bomberWispStageRules.stageRules != null)
            foreach (StageRule stageRule in bomberWispStageRules.stageRules)
            {
                XElement xelement1 = new XElement("StageRule");
                xelement.Add(xelement1);
                XElement xelement2 = new XElement("UseStageName", stageRule.useStageName);
                xelement1.Add(xelement2);
                XElement xelement3 = new XElement("StageName", stageRule.stageName.IsNullOrWhiteSpace() ? "" : stageRule.stageName);
                xelement1.Add(xelement3);
                XElement xelement4 = new XElement("UseStageCount", stageRule.useStageCount);
                xelement1.Add(xelement4);
                XElement xelement5 = new XElement("StageCount", stageRule.stageCount);
                xelement1.Add(xelement5);
                XElement xelement8 = new XElement("BlastRadius", stageRule.blastRadius);
                xelement1.Add(xelement8);
                XElement xelement9 = new XElement("BlastDamageCoefficient", stageRule.blastDamageCoefficient);
                xelement1.Add(xelement9);
                XElement xelement6 = new XElement("DetonationTime", stageRule.detonationTime);
                xelement1.Add(xelement6);
            }
        return new XDocument(list.ToArray());
    }
    public static BomberWispStageRules FromXml(XDocument xDocument)
    {
        try
        {
            BomberWispStageRules bomebrWispStageRules = new BomberWispStageRules();
            XElement root = xDocument.Root;
            List<StageRule> stageRules = [];
            foreach (XElement xElement in root.Elements("StageRule"))
            {
                StageRule stageRule = new StageRule();
                XElement xElement1 = xElement.Element("UseStageName");
                if (xElement1 != null && bool.TryParse(xElement1.Value, out bool useStageName)) stageRule.useStageName = useStageName;
                XElement xElement2 = xElement.Element("StageName");
                if (xElement2 != null) stageRule.stageName = xElement2.Value;
                XElement xElement3 = xElement.Element("UseStageCount");
                if (xElement3 != null && bool.TryParse(xElement3.Value, out bool useStageCount)) stageRule.useStageCount = useStageCount;
                XElement xElement4 = xElement.Element("StageCount");
                if (xElement4 != null && int.TryParse(xElement4.Value, out int stageCount)) stageRule.stageCount = stageCount;
                XElement xElement7 = xElement.Element("BlastRadius");
                if (xElement7 != null && float.TryParse(xElement7.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float blastRadius)) stageRule.blastRadius = blastRadius;
                XElement xElement8 = xElement.Element("BlastDamageCoefficient");
                if (xElement8 != null && float.TryParse(xElement8.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float blastDamageCoefficient)) stageRule.blastDamageCoefficient = blastDamageCoefficient;
                XElement xElement9 = xElement.Element("DetonationTime");
                if (xElement9 != null && float.TryParse(xElement9.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float detonationTime)) stageRule.detonationTime = detonationTime;
                stageRules.Add(stageRule);
            }
            bomebrWispStageRules.stageRules = stageRules.ToArray();
            return bomebrWispStageRules;
        }
        catch (Exception e)
        {
            CaeliImperiumPlugin.Log.LogError(e);
            return Default;
        }
    }
}
