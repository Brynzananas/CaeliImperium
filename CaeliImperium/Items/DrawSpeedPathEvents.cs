using BrynzaAPI;
using CaeliImperium.Components;
using CaeliImperium.Configs;
using CaeliImperium.ItemBehaviours;
using R2API;
using R2API.Networking;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static BrynzaAPI.GiveItemsDelegateDef;
using static CaeliImperium.CaeliImperiumHooks;
using static R2API.BuffsAPI;
using static R2API.RecalculateStatsAPI;

namespace CaeliImperium.Items
{
    public static class DrawSpeedPathEvents
    {
        public static float SpeedPathSpeedBonusCoefficient => DrawSpeedPathConfigs.SpeedPathSpeedBonusCoefficient.Value;
        public static float SpeedPathSpeedBonusStackCoefficient => DrawSpeedPathConfigs.SpeedPathSpeedBonusStackCoefficient.Value;
        public static float SpeedPathMinDistanceBetweenPoints = 1f;
        public static float SpeedPathClusterRadius = 20f;
        public static float SpeedPathMaxLength => DrawSpeedPathConfigs.SpeedPathMaxPathLength.Value;
        public static float SpeedPathMaxLengthStack => DrawSpeedPathConfigs.SpeedPathMaxPathLengthStack.Value;
        public static float SpeedPathSearchRadius = 4f;
        public static float SpeedPathSearchRadiusExcludeFromEnd = 12f;
        public static float SpeedPathRenderDistance => DrawSpeedPathConfigs.SpeedPathRenderDistance.Value;
        public static float SpeedPathFadeDistance = 3f;
        public static GameObject SpeedPathLine;
        public static EffectDef ChalkFootstep;
        public static float gradientMaxAlpha = 2f;
        public static float gradientExtraRange = 32f;
        public static float gradientCoefficient = 32f;
        public static bool inited { private set; get; }
        private static DrawSpeedPathRunAction drawSpeedPathRunAction;
        public static void Init(ItemDef itemDef)
        {
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            GetStatCoefficients += Events_GetStatCoefficients;
            OnBuffFirstStackGained += Events_OnBuffFirstStackGained;
            OnBuffFinalStackLost += Events_OnBuffFinalStackLost;
            drawSpeedPathRunAction = new DrawSpeedPathRunAction();
            CaeliImperiumExpansionRunComponent.caeliImperiumRunActions.Add(drawSpeedPathRunAction);
            R2API.FootstepAPI.OnFootstep += FootstepAPI_OnFootstep;
            CharacterBodyAPI.AddAlwaysSprintCondition(AlwaysSprint);
            CaeliImperiumPlugin.onPluginDestroyed += OnPluginDestroyed;
            if (inited) return;
            inited = true;
            CaeliImperiumContent.Buffs.SpeedPathSpeedBonus = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/SpeedPathSpeedBonus.asset").RegisterBuffDef();
            CaeliImperiumContent.Buffs.SpeedPathGravityWell = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/SpeedPathGravityWell.asset").RegisterBuffDef();
            ChalkFootstep = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Effects/ChalkFootstep.prefab").RegisterEffect();
            SpeedPathLine = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Prefabs/SpeedPathLine.prefab");
        }

        private static bool AlwaysSprint(CharacterBody characterBody) => DrawSpeedPathConfigs.SpeedPathAutosprint.Value ? characterBody.HasBuff(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus) : false;

        public static void OnPluginDestroyed()
        {
            CaeliImperiumPlugin.onPluginDestroyed -= OnPluginDestroyed;
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
            GetStatCoefficients -= Events_GetStatCoefficients;
            OnBuffFirstStackGained -= Events_OnBuffFirstStackGained;
            OnBuffFinalStackLost -= Events_OnBuffFinalStackLost;
            CaeliImperiumExpansionRunComponent.caeliImperiumRunActions.Remove(drawSpeedPathRunAction);
            drawSpeedPathRunAction = null;
            R2API.FootstepAPI.OnFootstep -= FootstepAPI_OnFootstep;
        }
        private static void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody obj)
        {
            obj.AddItemBehavior<DrawSpeedPath2Behaviour>(obj.inventory.GetItemCountEffective(CaeliImperiumContent.Items.DrawSpeedPath));
        }
        private static void FootstepAPI_OnFootstep(R2API.FootstepAPI.FootstepReport footstepReport)
        {
            if (!footstepReport.footstepHandler.body || !footstepReport.footstepHandler.body.HasBuff(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus)) return;
            EffectData effectData = new EffectData();
            effectData.origin = footstepReport.raycastHit.point;
            effectData.rotation = Util.QuaternionSafeLookRotation(footstepReport.raycastHit.normal);
            effectData.SetChildLocatorTransformReference(footstepReport.footstepHandler.body.gameObject, footstepReport.childIndex);
            EffectManager.SpawnEffect(ChalkFootstep.index, effectData, false);
        }
        public class SpeedPathGradient
        {
            public Collider nearestSpeedPath;
            public float distance= float.MaxValue;
            public Transform globalSpeedPath;
        }
        public static void Events_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus);
            args.moveSpeedMultAdd += buffCount.Stack(SpeedPathSpeedBonusCoefficient, SpeedPathSpeedBonusStackCoefficient);
        }
        public static void Events_OnBuffFinalStackLost(CharacterBody arg1, BuffDef arg2)
        {
            if (!arg1) return;
            if (arg2.buffIndex == CaeliImperiumContent.Buffs.SpeedPathGravityWell.buffIndex) arg1.ModifyCharacterGravityParams(-1);
            if (arg2.buffIndex == CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex && DrawSpeedPathConfigs.SpeedPathAutosprint.Value)
            {
                arg1.isSprinting = true;
                if (arg1.inputBank)
                {
                    arg1.inputBank.sprint.down = true;
                }
            }
        }
        public static void Events_OnBuffFirstStackGained(CharacterBody arg1, BuffDef arg2)
        {
            if (!arg1) return;
            if (arg2.buffIndex == CaeliImperiumContent.Buffs.SpeedPathGravityWell.buffIndex) arg1.ModifyCharacterGravityParams(1);
        }
    }
}
