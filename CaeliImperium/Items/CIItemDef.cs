using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Items
{
    [CreateAssetMenu(menuName = "RoR2/CIItemDef")]
    public class CIItemDef : ItemDef
    {
        public string configName;
        public ConfigItemTier configItemTier;
        public Sprite commonTierSprite;
        public Sprite uncommonTierSprite;
        public Sprite legendaryTierSprite;
        public enum ConfigItemTier
        {
            WhiteCommon,
            GreenUncommon,
            RedLegendary
        }
    }
}
