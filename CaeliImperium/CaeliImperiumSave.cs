using RoR2;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CaeliImperium;
public class CaeliImperiumSave
{
    public static FixedConditionalWeakTable<UserProfile, CaeliImperiumSave> keyValuePairs = [];
    public const string saveName = "CaeliImperiumSave";
    public const string pipelineRefineriesCompletedCountName = "PipelineRefineriesCompletedCount";
    public int pipelineRefineriesCompletedCount;
    public static void Init()
    {
        //R2API.SaveAPI.RegisterModdedUserProfileSaveData<CaeliImperiumSave>(CreateXElement, ReadXElement, Copy);
    }
    public static XDocument CreateXElement(object save, UserProfile userProfile)
    {
        if (save is not CaeliImperiumSave caeliImperiumSave) return null;
        XElement xelement = new XElement(saveName);
        XElement xElement2 = new XElement(pipelineRefineriesCompletedCountName, caeliImperiumSave.pipelineRefineriesCompletedCount);
        xelement.Add(xElement2);
        return new XDocument(xelement);
    }
    public static void ReadXElement(object save, XDocument xDocument, UserProfile userProfile)
    {
        if (save is not CaeliImperiumSave caeliImperiumSave) return;
        XElement xelement = xDocument.Element(saveName);
        if (xelement == null) return;
        XElement xElement2 = xelement.Element(pipelineRefineriesCompletedCountName);
        if (xElement2 != null && int.TryParse(xElement2.Value, out int pipelineRefineriesCompletedCount)) caeliImperiumSave.pipelineRefineriesCompletedCount = pipelineRefineriesCompletedCount;

    }
    private static void Copy(object srcSave, object destSave)
    {
        if (srcSave is not CaeliImperiumSave srcCaeliImperiumSave || destSave is not CaeliImperiumSave destCaeliImperiumSave) return;
        destCaeliImperiumSave.pipelineRefineriesCompletedCount = srcCaeliImperiumSave.pipelineRefineriesCompletedCount;
    }
    public static CaeliImperiumSave GetOrCreateCaeliImperiumSaveFromUserProfile(UserProfile userProfile)
    {
        if (!keyValuePairs.TryGetValue(userProfile, out CaeliImperiumSave caeliImperiumSave))
        {
            caeliImperiumSave = new CaeliImperiumSave();
            keyValuePairs.Add(userProfile, caeliImperiumSave);
        }
        return caeliImperiumSave;
    }
    public static CaeliImperiumSave GetCaeliImperiumSaveFromUserProfile(UserProfile userProfile)
    {
        if (keyValuePairs.TryGetValue(userProfile, out CaeliImperiumSave caeliImperiumSave)) return caeliImperiumSave;
        return null;
    }
}
