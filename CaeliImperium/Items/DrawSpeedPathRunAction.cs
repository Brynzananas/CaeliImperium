using CaeliImperium.Configs;
using CaeliImperium.ItemBehaviours;
using R2API.Networking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CaeliImperium.Items;

public class DrawSpeedPathRunAction : CaeliImperiumRunAction
{
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        ReadOnlyCollection<DrawSpeedPath2Behaviour> drawSpeedPath2Behaviours = DrawSpeedPath2Behaviour.readOnlyInstances;
        if (drawSpeedPath2Behaviours == null) return;
        if (!currentCharacterBody) return;
        int buffCount = 0;
        foreach (DrawSpeedPath2Behaviour drawSpeedPath in drawSpeedPath2Behaviours)
        {
            if (!drawSpeedPath.TeamCheck(currentCharacterBody)) continue;
            drawSpeedPath.UpdateLineGradient(currentCharacterBody.transform.position);
            if (!drawSpeedPath.IsNearPathExcludingEnd(currentCharacterBody.transform.position, DrawSpeedPathEvents.SpeedPathSearchRadius, 0f, DrawSpeedPathEvents.SpeedPathSearchRadiusExcludeFromEnd)) continue;
            if (buffCount < drawSpeedPath.stack) buffCount = drawSpeedPath.stack;
        }
        if (buffCount == 0 && currentCharacterBody.HasBuff(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus))
        {
            currentCharacterBody.ApplyBuff(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex, 0);
        }
        else if (buffCount != currentCharacterBody.GetBuffCount(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus))
        {
            currentCharacterBody.ApplyBuff(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex, buffCount);
        }
        bool hasGravityWellBuff = currentCharacterBody.HasBuff(CaeliImperiumContent.Buffs.SpeedPathGravityWell);
        if (!DrawSpeedPathConfigs.SpeedPathFlight.Value)
        {
            if (hasGravityWellBuff) currentCharacterBody.ApplyBuff(CaeliImperiumContent.Buffs.SpeedPathGravityWell.buffIndex, 0);
            return;
        }
        if (buffCount == 0 && hasGravityWellBuff)
        {
            currentCharacterBody.ApplyBuff(CaeliImperiumContent.Buffs.SpeedPathGravityWell.buffIndex, 0);
        }
        else if (buffCount != currentCharacterBody.GetBuffCount(CaeliImperiumContent.Buffs.SpeedPathGravityWell))
        {
            currentCharacterBody.ApplyBuff(CaeliImperiumContent.Buffs.SpeedPathGravityWell.buffIndex, buffCount);
        }
    }
}
