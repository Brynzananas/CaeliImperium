using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static CaeliImperium.Utils;
using static CaeliImperium.Hooks; 
using static CaeliImperium.Events;

namespace CaeliImperium
{
    public class Items
    {
        public static ItemDef Keychain = CreateItem("Keychain", null, null, true, ItemTier.Tier1, new ItemTag[] { ItemTag.Damage }, null, KeychainEvents);
        public static ItemDef ContainedPotential = CreateItem("Contained Potential", null, null, true, ItemTier.Tier2, new ItemTag[] { ItemTag.Utility }, null, ContainedPotentialEvent);
        public static ItemDef SkullMachineGun = CreateItem("Skull Machine Gun", null, null, true, ItemTier.Tier3, new ItemTag[] { ItemTag.Damage }, null, SkullMachineGunEvents);
        public static ItemDef CopperBell = CreateItem("Copper Bell", null, null, true, ItemTier.Tier2, new ItemTag[] { ItemTag.Damage }, null, CopperBellEvents);
        public static ItemDef GuardianFlesh = CreateItem("Guardian Flesh", null, null, true, ItemTier.Tier3, new ItemTag[] { ItemTag.Utility }, null, GuardianFleshEvents);
        public static ItemDef AtomicHeart = CreateItem("Atomic Heart", null, null, true, ItemTier.Tier3, new ItemTag[] { ItemTag.Damage }, null, AtomicHeartEvents);
        public static ItemDef SewingMachine = CreateItem("Sewing Machine", null, null, true, ItemTier.Tier3, new ItemTag[] { ItemTag.Damage }, null, GeneModificationEvents);
        public static ItemDef Medicine = CreateItem("Medicine", null, null, true, ItemTier.Tier1, new ItemTag[] { ItemTag.Healing }, null, MedicineEvents);
        public static ItemDef PossessedSword = CreateItem("Possessed Sword", null, null, true, ItemTier.Tier2, new ItemTag[] { ItemTag.Damage }, null, PossessedSwordEvents);
        public static ItemDef FireAxe = CreateItem("Fire Axe", null, null, true, ItemTier.Tier3, new ItemTag[] { ItemTag.Damage }, null, FireAxeEvents);
        public static ItemDef BrassKnuckles = CreateItem("Brass Knuckles", null, null, true, ItemTier.Tier1, new ItemTag[] { ItemTag.Damage }, null, BrassKnucklesEvents);
        public static ItemDef TaoManuscript = CreateItem("Tao Manuscript", null, null, true, ItemTier.Tier2, new ItemTag[] { ItemTag.Damage }, null, TaoManuscriptEvents);
        public static ItemDef TaoFruit = CreateItem("Tao Fruit", null, null, true, ItemTier.Tier2, new ItemTag[] { ItemTag.Healing }, null);
        public static ItemDef Chalk = CreateItem("Chalk", null, null, true, ItemTier.Tier2, new ItemTag[] { ItemTag.Utility }, null, ChalkEvents);
        public static ItemDef TransferDamageOwnership = CreateItem("TransferDamageOwnership", null, null, false, ItemTier.NoTier, new ItemTag[] { ItemTag.Damage }, null, TransferDamageOwnershipEvents);
    }
}
