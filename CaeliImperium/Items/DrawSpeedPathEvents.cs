using CaeliImperium.Configs;
using CaeliImperium.ItemBehaviours;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static BrynzaAPI.GiveItemsDelegateDef;
using static CaeliImperium.Hooks;
using static R2API.RecalculateStatsAPI;

namespace CaeliImperium.Items
{
    public static class DrawSpeedPathEvents
    {
        public static float SpeedPathSpeedBonusCoefficient => DrawSpeedPathConfigs.SpeedPathSpeedBonusCoefficient.Value;
        public static float SpeedPathSpeedBonusStackCoefficient => DrawSpeedPathConfigs.SpeedPathSpeedBonusStackCoefficient.Value;
        public static void Init(ItemDef itemDef)
        {
            CaeliImperiumContent.Buffs.SpeedPathSpeedBonus = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/SpeedPathSpeedBonus.asset").RegisterBuffDef();
            OnInventoryChanged += Events_OnInventoryChanged;
            GetStatCoefficients += Events_GetStatCoefficients;
            OnBuffFirstStackGained += Events_OnBuffFirstStackGained;
            OnBuffFinalStackLost += Events_OnBuffFinalStackLost;
            CaeliImperiumPlugin.OnPluginDestroyed += OnPluginDestroyed;
        }
        public static void Events_OnInventoryChanged(CharacterBody obj)
        {
            int stacks = obj.inventory ? obj.inventory.GetItemCountEffective(CaeliImperiumContent.Items.DrawSpeedPath) : 0;
            obj.AddItemBehavior<DrawSpeedPathBehaviour>(stacks);
        }
        public static void Events_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus);
            args.moveSpeedMultAdd += buffCount.Stack(SpeedPathSpeedBonusCoefficient, SpeedPathSpeedBonusStackCoefficient);
        }
        public static void Events_OnBuffFinalStackLost(CharacterBody arg1, BuffDef arg2)
        {
            if (arg2.buffIndex == CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex) arg1.ModifyCharacterGravityParams(-1);
        }
        public static void Events_OnBuffFirstStackGained(CharacterBody arg1, BuffDef arg2)
        {
            if (arg2.buffIndex == CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex) arg1.ModifyCharacterGravityParams(1);
        }
        public static void OnPluginDestroyed()
        {
            CaeliImperiumPlugin.OnPluginDestroyed -= OnPluginDestroyed;
            OnInventoryChanged -= Events_OnInventoryChanged;
            GetStatCoefficients -= Events_GetStatCoefficients;
            OnBuffFirstStackGained -= Events_OnBuffFirstStackGained;
            OnBuffFinalStackLost -= Events_OnBuffFinalStackLost;
        }
    }
}
