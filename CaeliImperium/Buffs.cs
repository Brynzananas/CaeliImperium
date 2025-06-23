using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CaeliImperium.Utils;
using static CaeliImperium.Events;

namespace CaeliImperium
{
    public class Buffs
    {
        public static BuffDef IncreaseCritChanceAndDamage = CreateBuff("IncreaseCritChanceAndDamage", null, Color.white, canStack: true, isDebuff: false, isCooldown: false, isHidden: false, ignoreGrowthNectar: false);
        public static BuffDef IncreaseDamagePereodically = CreateBuff("IncreaseDamagePereodically", null, Color.white, canStack: true, isDebuff: false, isCooldown: false, isHidden: false, ignoreGrowthNectar: false);
        public static BuffDef ConvertDamageToAtomicCharge = CreateBuff("ConvertDamageToAtomicCharge", null, Color.white, canStack: true, isDebuff: false, isCooldown: false, isHidden: true, ignoreGrowthNectar: true);
        public static BuffDef IncreaseMercenaryCloneSummonChance = CreateBuff("IncreaseMercenaryCloneSummonChance", null, Color.white, canStack: true, isDebuff: false, isCooldown: false, isHidden: true, ignoreGrowthNectar: true);
        public static BuffDef TaoCount = CreateBuff("TaoCount", null, Color.white, canStack: true, isDebuff: false, isCooldown: true, isHidden: true, ignoreGrowthNectar: true);
        public static BuffDef TaoPunchReady = CreateBuff("TaoPunchReady", null, Color.white, canStack: true, isDebuff: false, isCooldown: false, isHidden: false, ignoreGrowthNectar: true);
        public static BuffDef TaoPunchCooldown = CreateBuff("TaoPunchCooldown", null, Color.white, canStack: true, isDebuff: false, isCooldown: true, isHidden: false, ignoreGrowthNectar: true);
        public static BuffDef AffixHasting = CreateBuff("AffixHasting", null, Color.white, canStack: false, isDebuff: false, isCooldown: false, isHidden: false, ignoreGrowthNectar: true);
        public static BuffDef IrradiatedBuff = CreateBuff("Irradiated", null, Color.white, true, false, false, false, true);
        public static DotController.DotDef IrradiatedDot = CreateDOT(IrradiatedBuff, out IrradiatedDotIndex, false, 0.5f, 0.5f, DamageColorIndex.Default, IrradiatedDotBehaviour, null, IrradiatedEvents);
        public static DotController.DotIndex IrradiatedDotIndex;
    }
}
