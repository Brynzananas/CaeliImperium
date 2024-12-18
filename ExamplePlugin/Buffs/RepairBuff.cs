﻿using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperiumPlugin.CaeliImperium;

namespace CaeliImperium.Buffs
{
    internal static class RepairBuff
    {
        public static BuffDef RepairBuffDef;
        internal static Sprite RepairIcon;
        internal static void Init()
        {


            RepairIcon = MainAssets.LoadAsset<Sprite>("Assets/Icons/PainkillersIcon.png");

            Buff();

        }
        private static void Buff()
        {
            RepairBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            RepairBuffDef.name = "Repair";
            RepairBuffDef.buffColor = Color.grey;
            RepairBuffDef.canStack = true;
            RepairBuffDef.isDebuff = false;
            RepairBuffDef.ignoreGrowthNectar = false;
            RepairBuffDef.iconSprite = RepairIcon;
            RepairBuffDef.isHidden = false;
            RepairBuffDef.isCooldown = false;
            ContentAddition.AddBuffDef(RepairBuffDef);
        }
    }
}