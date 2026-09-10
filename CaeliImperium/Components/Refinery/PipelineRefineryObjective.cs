using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaeliImperium.Components.Refinery;
public class PipelineRefineryObjective : ObjectivePanelController.ObjectiveTracker
{
    public override string GenerateString()
    {
        if (!sourceDescriptor.source || !(sourceDescriptor.source is PipelineRefineryController pipelineRefineryController)) return "";
        if (pipelineRefineryController.completed) return Language.GetString("CI_REFINERY_OBJECTIVE_COMPLETED");
        if (pipelineRefineryController.underSabotage) return string.Format(Language.GetString("CI_REFINERY_OBJECTIVE_SABOTAGE"), pipelineRefineryController.currentlySabotagedPipes);
        if (pipelineRefineryController.running) return string.Format(Language.GetString("CI_REFINERY_OBJECTIVE_RUNNING"), (int)(pipelineRefineryController.runningPercentage));
        if (pipelineRefineryController.transformsForPipelineBuilders == null) return "";
        if (pipelineRefineryController.currentlyCompletedBuilders >= pipelineRefineryController.transformsForPipelineBuilders.Length) return Language.GetString("CI_REFINERY_OBJECTIVE_READY");
        if (pipelineRefineryController.currentlyCompletedBuilders >= pipelineRefineryController.neededCompletedBuilders)
        {
            return string.Format(Language.GetString("CI_REFINERY_OBJECTIVE_BUILDING"), pipelineRefineryController.currentlyCompletedBuilders, pipelineRefineryController.transformsForPipelineBuilders.Length);
        }
        else
        {
            return string.Format(Language.GetString("CI_REFINERY_OBJECTIVE_BUILDING_FIRST"), pipelineRefineryController.currentlyCompletedBuilders, pipelineRefineryController.transformsForPipelineBuilders.Length, pipelineRefineryController.neededCompletedBuilders);
        }
    }
    public override bool IsDirty() => true;
}
