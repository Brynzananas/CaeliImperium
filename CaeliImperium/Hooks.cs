using System;
using System.Collections.Generic;
using System.Text;
using EntityStates.AffixVoid;
using R2API;
using RoR2;
using static RoR2.DotController;

namespace CaeliImperium
{
    public class Hooks
    {
        public static void Init()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            On.RoR2.PurchaseInteraction.OnEnable += PurchaseInteraction_OnEnable;
            On.RoR2.CombatDirector.Init += CombatDirector_Init;
            On.RoR2.GenericSkill.RecalculateMaxStock += GenericSkill_RecalculateMaxStock;
        }

        private static void GenericSkill_RecalculateMaxStock(On.RoR2.GenericSkill.orig_RecalculateMaxStock orig, GenericSkill self)
        {
            orig(self);
            OnRecalculateMaxStock?.Invoke(self);
        }
        public static event Action<GenericSkill> OnRecalculateMaxStock;
        private static void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();
            OnCombatDirectorInit?.Invoke();
        }
        public static event Action OnCombatDirectorInit;
        private static void PurchaseInteraction_OnEnable(On.RoR2.PurchaseInteraction.orig_OnEnable orig, PurchaseInteraction self)
        {
            orig(self);
            OnPurchaseInteractionEnable?.Invoke(self);
        }
        public static event Action<PurchaseInteraction> OnPurchaseInteractionEnable;

        public static event Action<HealthComponent, DamageInfo, CharacterBody> OnTakeDamageProcess;
        private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            CharacterBody characterBody = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
            OnTakeDamageProcess?.Invoke(self, damageInfo, characterBody);
            orig(self, damageInfo);
        }
        public delegate bool PerformEquipmentAction(EquipmentSlot equipmentSlot, EquipmentDef equipmentDef);
        public static Dictionary<EquipmentDef, PerformEquipmentAction> equipmentActions = new Dictionary<EquipmentDef, PerformEquipmentAction>();
        private static bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentActions.ContainsKey(equipmentDef))
            {
                if (equipmentActions[equipmentDef] != null)
                {
                    if (equipmentActions[equipmentDef](self, equipmentDef))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return orig(self, equipmentDef);
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            OnInventoryChanged?.Invoke(self);
        }
        public static event Action<CharacterBody> OnInventoryChanged;
    }
}
