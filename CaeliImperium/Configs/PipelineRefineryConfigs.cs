using BepInEx.Configuration;
using CaeliImperium.Components.Refinery;
using CaeliImperium.Interactables;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CaeliImperium.Configs;
public static class PipelineRefineryConfigs
{
    public const string sectionName = "Interactables: On Site Refinery";
    public const string cantUpdateExistingDisclaimer = "Changing this config will not update existing instances";
    public const string placeholderDisclaimer = "Config for placeholder feature. Will be gone soon";
    public static InteractableSpawnRules pipelineRefinerySpawnRules;
    public static void Init()
    {
        if (!PipelineRefineryEvents.PipelineRefinery) return;
        PipelineRefinerySpawnRules = CaeliImperiumUtils.CreateConfig(sectionName, "Spawn Rules", Interactables.PipelineRefineryEvents.DefaultSpawnRules.ToXml().ConvertToString(), "");
        PipelineRefinerySpawnRules.SettingChanged += PipelineRefinerySpawnRules_SettingChanged;
        ResourceWellSpawnCount = CaeliImperiumUtils.CreateConfig(sectionName, "Resource Well Spawn Count", 3, "");
        UpdatePipelineRefinerySpawnRules();
        InitPipelineRefineryController();
        InitPipelineRefineryDropTable();
        InitExtraDirector();
        InitPipelineBuilder();
        InitPipelineBuilderZiprailController();
        InitWellExtractor();
        InitPipelinePoint();
    }

    private static void PipelineRefinerySpawnRules_SettingChanged(object sender, EventArgs e) => UpdatePipelineRefinerySpawnRules();

    public static void UpdatePipelineRefinerySpawnRules()
    {
        XDocument xDocument = PipelineRefinerySpawnRules.Value.ConvertToXDocument(true);
        if (xDocument == null)
        {
            pipelineRefinerySpawnRules = PipelineRefineryEvents.DefaultSpawnRules;
        }
        else
        {
            pipelineRefinerySpawnRules = InteractableSpawnRules.FromXml(xDocument);
            if (pipelineRefinerySpawnRules == null) pipelineRefinerySpawnRules = PipelineRefineryEvents.DefaultSpawnRules;
        }
    }
    public static void InitPipelineRefineryController()
    {
        PipelineRefineryController pipelineRefineryController = PipelineRefineryEvents.PipelineRefinery.GetComponent<PipelineRefineryController>();
        if (!pipelineRefineryController) return;
        PipelineRefineryTimeToComplete = CaeliImperiumUtils.CreateConfig(sectionName, "Time To Complete", pipelineRefineryController.timeToComplete, cantUpdateExistingDisclaimer);
        PipelineRefineryTimeToComplete.SettingChanged += PipelineRefineryTimeToComplete_SettingChanged;
        PipelineRefineryNeededCompletedBuilders = CaeliImperiumUtils.CreateConfig(sectionName, "Needed Completed Builders", pipelineRefineryController.neededCompletedBuilders, cantUpdateExistingDisclaimer);
        PipelineRefineryNeededCompletedBuilders.SettingChanged += PipelineRefineryTimeToComplete_SettingChanged;
        PipelineRefineryItemsToGivePerCompletedBuilder = CaeliImperiumUtils.CreateConfig(sectionName, "Items To Give Per Completed Builder", pipelineRefineryController.placeholderItemsToGivePerCompletedBuilder, cantUpdateExistingDisclaimer + ". " + placeholderDisclaimer);
        PipelineRefineryMultiplyItemsToGiveByPlayerCount = CaeliImperiumUtils.CreateConfig(sectionName, "Multiply Items To Give By Player Count", true, placeholderDisclaimer);
        PipelineRefineryItemsToGivePerCompletedBuilder.SettingChanged += PipelineRefineryTimeToComplete_SettingChanged;
        UpdatePipelineRefineryController();
    }
    private static void PipelineRefineryTimeToComplete_SettingChanged(object sender, EventArgs e) => UpdatePipelineRefineryController();
    public static void UpdatePipelineRefineryController()
    {
        PipelineRefineryController pipelineRefineryController = PipelineRefineryEvents.PipelineRefinery.GetComponent<PipelineRefineryController>();
        if (!pipelineRefineryController) return;
        pipelineRefineryController.timeToComplete = PipelineRefineryTimeToComplete.Value;
        pipelineRefineryController.neededCompletedBuilders = PipelineRefineryNeededCompletedBuilders.Value;
        pipelineRefineryController.placeholderItemsToGivePerCompletedBuilder = PipelineRefineryItemsToGivePerCompletedBuilder.Value;
    }
    public const string dropTableName = "Drop Table ";
    public static void InitPipelineRefineryDropTable()
    {
        PipelineRefineryController pipelineRefineryController = PipelineRefineryEvents.PipelineRefinery.GetComponent<PipelineRefineryController>();
        if (!pipelineRefineryController || !pipelineRefineryController.placeholderDropTable || !(pipelineRefineryController.placeholderDropTable is BasicPickupDropTable basicPickupDropTable)) return;
        PipelineRefineryDropTableTier1Weight = CaeliImperiumUtils.CreateConfig(sectionName, dropTableName + "Tier1 Weight", basicPickupDropTable.tier1Weight, placeholderDisclaimer);
        PipelineRefineryDropTableTier1Weight.SettingChanged += PipelineRefineryDropTableTier1Weight_SettingChanged;
        PipelineRefineryDropTableTier2Weight = CaeliImperiumUtils.CreateConfig(sectionName, dropTableName + "Tier2 Weight", basicPickupDropTable.tier2Weight, placeholderDisclaimer);
        PipelineRefineryDropTableTier2Weight.SettingChanged += PipelineRefineryDropTableTier1Weight_SettingChanged;
        PipelineRefineryDropTableTier3Weight = CaeliImperiumUtils.CreateConfig(sectionName, dropTableName + "Tier3 Weight", basicPickupDropTable.tier3Weight, placeholderDisclaimer);
        PipelineRefineryDropTableTier3Weight.SettingChanged += PipelineRefineryDropTableTier1Weight_SettingChanged;
        PipelineRefineryDropTableBossWeight = CaeliImperiumUtils.CreateConfig(sectionName, dropTableName + "Boss Weight", basicPickupDropTable.bossWeight, placeholderDisclaimer);
        PipelineRefineryDropTableBossWeight.SettingChanged += PipelineRefineryDropTableTier1Weight_SettingChanged;
        UpdatePipelineRefineryDropTable();
    }
    private static void PipelineRefineryDropTableTier1Weight_SettingChanged(object sender, EventArgs e) => UpdatePipelineRefineryDropTable();
    public static void UpdatePipelineRefineryDropTable()
    {
        PipelineRefineryController pipelineRefineryController = PipelineRefineryEvents.PipelineRefinery.GetComponent<PipelineRefineryController>();
        if (!pipelineRefineryController || !pipelineRefineryController.placeholderDropTable || !(pipelineRefineryController.placeholderDropTable is BasicPickupDropTable basicPickupDropTable)) return;
        basicPickupDropTable.tier1Weight = PipelineRefineryDropTableTier1Weight.Value;
        basicPickupDropTable.tier2Weight = PipelineRefineryDropTableTier2Weight.Value;
        basicPickupDropTable.tier3Weight = PipelineRefineryDropTableTier3Weight.Value;
        basicPickupDropTable.bossWeight = PipelineRefineryDropTableBossWeight.Value;
    }
    public const string extraDirectorName = "Extra Combat Director ";
    public static void InitExtraDirector()
    {
        PipelineRefineryEnableExtraCombatDirector = CaeliImperiumUtils.CreateConfig(sectionName, "Enable Extra Combat Director?", true, "");
        PipelineRefineryStageCombatDirectorsBehaviour = CaeliImperiumUtils.CreateConfig(sectionName, "Stage Combat Directors Behaviour", PipelineRefineryController.StageCombatDirectorsBehaviour.AlwaysEnabled, "Active only when On Site Refinery is running");
        CombatDirector combatDirector = PipelineRefineryEvents.PipelineRefinery.GetComponent<CombatDirector>();
        if (!combatDirector) return;
        PipelineRefineryExtraCombatDirectorExpRewardCoefficient = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Exp Reward Coefficient", combatDirector.expRewardCoefficient, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorExpRewardCoefficient.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        PipelineRefineryExtraCombatDirectorGoldRewardCoefficient = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Gold Reward Coefficient", combatDirector.goldRewardCoefficient, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorGoldRewardCoefficient.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        PipelineRefineryExtraCombatDirectorMinSeriesSpawnInterval = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Min Series Spawn Interval", combatDirector.minSeriesSpawnInterval, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorMinSeriesSpawnInterval.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        PipelineRefineryExtraCombatDirectorMaxSeriesSpawnInterval = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Max Series Spawn Interval", combatDirector.maxSeriesSpawnInterval, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorMaxSeriesSpawnInterval.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        PipelineRefineryExtraCombatDirectorMinRerollSpawnInterval = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Min Reroll Spawn Interval", combatDirector.minRerollSpawnInterval, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorMinRerollSpawnInterval.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        PipelineRefineryExtraCombatDirectorMaxRerollSpawnInterval = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Max Reroll Spawn Interval", combatDirector.maxRerollSpawnInterval, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorMaxRerollSpawnInterval.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        PipelineRefineryExtraCombatDirectorCreditMultiplier = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Credit Multiplier", combatDirector.creditMultiplier, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorCreditMultiplier.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        PipelineRefineryExtraCombatDirectorSpawnDistanceMultiplier = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Spawn Distance Multiplier", combatDirector.spawnDistanceMultiplier, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorSpawnDistanceMultiplier.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        PipelineRefineryExtraCombatDirectorShouldSpawnOneWave = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Should Spawn One Wave", combatDirector.shouldSpawnOneWave, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorShouldSpawnOneWave.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        PipelineRefineryExtraCombatDirectorTargetPlayers = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Target Players", combatDirector.targetPlayers, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorTargetPlayers.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        PipelineRefineryExtraCombatDirectorSkipSpawnIfTooCheap = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Skip Spawn If Too Cheap", combatDirector.skipSpawnIfTooCheap, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorSkipSpawnIfTooCheap.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        PipelineRefineryExtraCombatDirectorResetMonsterCardIfFailed = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Reset Monster Card If Failed", combatDirector.resetMonsterCardIfFailed, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorResetMonsterCardIfFailed.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        PipelineRefineryExtraCombatDirectorEliteBias = CaeliImperiumUtils.CreateConfig(sectionName, extraDirectorName + "Elite Bias", combatDirector.eliteBias, cantUpdateExistingDisclaimer);
        PipelineRefineryExtraCombatDirectorEliteBias.SettingChanged += PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged;
        UpdateExtraDirector();
    }
    private static void PipelineRefineryExtraCombatDirectorExpRewardCoefficient_SettingChanged(object sender, EventArgs e) => UpdateExtraDirector();
    public static void UpdateExtraDirector()
    {
        CombatDirector combatDirector = PipelineRefineryEvents.PipelineRefinery.GetComponent<CombatDirector>();
        if (!combatDirector) return;
        combatDirector.expRewardCoefficient = PipelineRefineryExtraCombatDirectorExpRewardCoefficient.Value;
        combatDirector.goldRewardCoefficient = PipelineRefineryExtraCombatDirectorGoldRewardCoefficient.Value;
        combatDirector.minSeriesSpawnInterval = PipelineRefineryExtraCombatDirectorMinSeriesSpawnInterval.Value;
        combatDirector.maxSeriesSpawnInterval = PipelineRefineryExtraCombatDirectorMaxSeriesSpawnInterval.Value;
        combatDirector.minRerollSpawnInterval = PipelineRefineryExtraCombatDirectorMinRerollSpawnInterval.Value;
        combatDirector.maxRerollSpawnInterval = PipelineRefineryExtraCombatDirectorMaxRerollSpawnInterval.Value;
        combatDirector.creditMultiplier = PipelineRefineryExtraCombatDirectorCreditMultiplier.Value;
        combatDirector.spawnDistanceMultiplier = PipelineRefineryExtraCombatDirectorSpawnDistanceMultiplier.Value;
        combatDirector.shouldSpawnOneWave = PipelineRefineryExtraCombatDirectorShouldSpawnOneWave.Value;
        combatDirector.targetPlayers = PipelineRefineryExtraCombatDirectorTargetPlayers.Value;
        combatDirector.skipSpawnIfTooCheap = PipelineRefineryExtraCombatDirectorSkipSpawnIfTooCheap.Value;
        combatDirector.resetMonsterCardIfFailed = PipelineRefineryExtraCombatDirectorResetMonsterCardIfFailed.Value;
        combatDirector.eliteBias = PipelineRefineryExtraCombatDirectorEliteBias.Value;
    }
    public const string pipelineName = "Pipeline ";
    public static void InitPipelineBuilder()
    {
        PipelineBuilder pipelineBuilder = PipelineRefineryEvents.PipelineBuilder.GetComponent<PipelineBuilder>();
        if (!pipelineBuilder) return;
        PipelineBuilderMinLength = CaeliImperiumUtils.CreateConfig(sectionName, pipelineName + "Min Length", pipelineBuilder.minLength, cantUpdateExistingDisclaimer);
        PipelineBuilderMinLength.SettingChanged += PipelineBuilderMinLength_SettingChanged;
        PipelineBuilderMaxLength = CaeliImperiumUtils.CreateConfig(sectionName, pipelineName + "Max Length", pipelineBuilder.maxLength, cantUpdateExistingDisclaimer);
        PipelineBuilderMaxLength.SettingChanged += PipelineBuilderMinLength_SettingChanged;
        PipelineBuilderMaxBendAngle = CaeliImperiumUtils.CreateConfig(sectionName, pipelineName + "Max Bend Angle", pipelineBuilder.maxBendAngle, cantUpdateExistingDisclaimer);
        PipelineBuilderMaxBendAngle.SettingChanged += PipelineBuilderMinLength_SettingChanged;
        UpdatePipelineBuilder();
    }
    private static void PipelineBuilderMinLength_SettingChanged(object sender, EventArgs e) => UpdatePipelineBuilder();
    public static void UpdatePipelineBuilder()
    {
        PipelineBuilder pipelineBuilder = PipelineRefineryEvents.PipelineBuilder.GetComponent<PipelineBuilder>();
        if (!pipelineBuilder) return;
        pipelineBuilder.minLength = PipelineBuilderMinLength.Value;
        pipelineBuilder.maxLength = PipelineBuilderMaxLength.Value;
        pipelineBuilder.maxBendAngle = PipelineBuilderMaxBendAngle.Value;
    }
    public static void InitPipelineBuilderZiprailController()
    {
        ZiprailController ziprailController = PipelineRefineryEvents.PipelineBuilder.GetComponent<ZiprailController>();
        if (!ziprailController) return;
        PipelineBuilderZiprailControllerSpeed = CaeliImperiumUtils.CreateConfig(sectionName, pipelineName + "Ziprail Speed", ziprailController._speed, cantUpdateExistingDisclaimer);
        PipelineBuilderZiprailControllerSpeed.SettingChanged += PipelineBuilderZiprailControllerSpeed_SettingChanged;
        UpdatePipelineBuilderZiprailController();
    }
    private static void PipelineBuilderZiprailControllerSpeed_SettingChanged(object sender, EventArgs e) => UpdatePipelineBuilderZiprailController();
    public static void UpdatePipelineBuilderZiprailController()
    {
        ZiprailController ziprailController = PipelineRefineryEvents.PipelineBuilder.GetComponent<ZiprailController>();
        if (!ziprailController) return;
        ziprailController._speed = PipelineBuilderZiprailControllerSpeed.Value;
    }
    public static void InitWellExtractor()
    {
        WellExtractorController wellExtractorController = PipelineRefineryEvents.WellExtractor.GetComponent<WellExtractorController>();
        if (!wellExtractorController) return;
        WellExtractorShaveRunTimer = CaeliImperiumUtils.CreateConfig(sectionName, "Deduct Run Timer On Finishing Pipeline In Seconds", wellExtractorController.shaveRunTimer, cantUpdateExistingDisclaimer);
        WellExtractorShaveRunTimer.SettingChanged += WellExtractorShaveRunTimer_SettingChanged;
        UpdateWellExtractor();
    }
    private static void WellExtractorShaveRunTimer_SettingChanged(object sender, EventArgs e) => UpdateWellExtractor();
    public static void UpdateWellExtractor()
    {
        WellExtractorController wellExtractorController = PipelineRefineryEvents.WellExtractor.GetComponent<WellExtractorController>();
        if (!wellExtractorController) return;
        wellExtractorController.shaveRunTimer = WellExtractorShaveRunTimer.Value;
    }
    public static void InitPipelinePoint()
    {
        PipelineInteractor pipelineInteractor = PipelineRefineryEvents.PipelinePoint.GetComponent<PipelineInteractor>();
        if (!pipelineInteractor) return;
        PipelinePointMonsterCredits = CaeliImperiumUtils.CreateConfig(sectionName, "Monster Credits To Spend On Sabotage", pipelineInteractor.monsterCredits, cantUpdateExistingDisclaimer);
        PipelinePointMonsterCredits.SettingChanged += PipelinePointMonsterCredits_SettingChanged;
        UpdatePipelinePoint();
    }
    private static void PipelinePointMonsterCredits_SettingChanged(object sender, EventArgs e) => UpdatePipelinePoint();
    public static void UpdatePipelinePoint()
    {
        PipelineInteractor pipelineInteractor = PipelineRefineryEvents.PipelinePoint.GetComponent<PipelineInteractor>();
        if (!pipelineInteractor) return;
        pipelineInteractor.monsterCredits = PipelinePointMonsterCredits.Value;
    }
    public static ConfigEntry<string> PipelineRefinerySpawnRules;
    public static ConfigEntry<int> ResourceWellSpawnCount;
    public static ConfigEntry<float> PipelineRefineryTimeToComplete;
    public static ConfigEntry<int> PipelineRefineryNeededCompletedBuilders;
    public static ConfigEntry<int> PipelineRefineryItemsToGivePerCompletedBuilder;
    public static ConfigEntry<bool> PipelineRefineryMultiplyItemsToGiveByPlayerCount;
    public static ConfigEntry<float> PipelineRefineryDropTableTier1Weight;
    public static ConfigEntry<float> PipelineRefineryDropTableTier2Weight;
    public static ConfigEntry<float> PipelineRefineryDropTableTier3Weight;
    public static ConfigEntry<float> PipelineRefineryDropTableBossWeight;
    public static ConfigEntry<bool> PipelineRefineryEnableExtraCombatDirector;
    public static ConfigEntry<PipelineRefineryController.StageCombatDirectorsBehaviour> PipelineRefineryStageCombatDirectorsBehaviour;
    public static ConfigEntry<float> PipelineRefineryExtraCombatDirectorExpRewardCoefficient;
    public static ConfigEntry<float> PipelineRefineryExtraCombatDirectorGoldRewardCoefficient;
    public static ConfigEntry<float> PipelineRefineryExtraCombatDirectorMinSeriesSpawnInterval;
    public static ConfigEntry<float> PipelineRefineryExtraCombatDirectorMaxSeriesSpawnInterval;
    public static ConfigEntry<float> PipelineRefineryExtraCombatDirectorMinRerollSpawnInterval;
    public static ConfigEntry<float> PipelineRefineryExtraCombatDirectorMaxRerollSpawnInterval;
    public static ConfigEntry<float> PipelineRefineryExtraCombatDirectorCreditMultiplier;
    public static ConfigEntry<float> PipelineRefineryExtraCombatDirectorSpawnDistanceMultiplier;
    public static ConfigEntry<bool> PipelineRefineryExtraCombatDirectorShouldSpawnOneWave;
    public static ConfigEntry<bool> PipelineRefineryExtraCombatDirectorTargetPlayers;
    public static ConfigEntry<bool> PipelineRefineryExtraCombatDirectorSkipSpawnIfTooCheap;
    public static ConfigEntry<bool> PipelineRefineryExtraCombatDirectorResetMonsterCardIfFailed;
    public static ConfigEntry<float> PipelineRefineryExtraCombatDirectorEliteBias;
    public static ConfigEntry<float> PipelineBuilderMinLength;
    public static ConfigEntry<float> PipelineBuilderMaxLength;
    public static ConfigEntry<float> PipelineBuilderMaxBendAngle;
    public static ConfigEntry<float> PipelineBuilderZiprailControllerSpeed;
    public static ConfigEntry<float> WellExtractorShaveRunTimer;
    public static ConfigEntry<float> PipelinePointMonsterCredits;
}
