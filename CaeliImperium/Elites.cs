using System;
using System.Collections.Generic;
using System.Text;
using static CaeliImperium.Utils;
using static CaeliImperium.Events;
using static CaeliImperium.Buffs;
using RoR2;

namespace CaeliImperium
{
    public class Elites
    {
        public static EliteDef HastingElite = CreateElite("Hasting", AffixHasting, null, 0, 4f, 2f, 1, null, HastingEvents);
    }
}
