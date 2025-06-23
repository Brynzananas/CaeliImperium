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
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
            On.RoR2.DotController.EvaluateDotStacksForType += DotController_EvaluateDotStacksForType;
            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            On.RoR2.GenericSkill.Awake += GenericSkill_Awake;
            On.RoR2.PurchaseInteraction.OnEnable += PurchaseInteraction_OnEnable;
            On.RoR2.CombatDirector.Init += CombatDirector_Init;
            On.RoR2.GenericSkill.RecalculateMaxStock += GenericSkill_RecalculateMaxStock;
            //On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
            On.RoR2.GenericSkill.CalculateFinalRechargeInterval += GenericSkill_CalculateFinalRechargeInterval;

        }

        public static event Action<GenericSkill> OnCalculateFinalRechargeIntervalBefore;
        private static float GenericSkill_CalculateFinalRechargeInterval(On.RoR2.GenericSkill.orig_CalculateFinalRechargeInterval orig, GenericSkill self)
        {
            OnCalculateFinalRechargeIntervalBefore?.Invoke(self);
            return orig(self);
        }

        public static event Action<GenericSkill> OnRecalculateMaxStockBefore;
        private static void GenericSkill_RecalculateMaxStock(On.RoR2.GenericSkill.orig_RecalculateMaxStock orig, GenericSkill self)
        {
            OnRecalculateMaxStockBefore?.Invoke(self);
            orig(self);
            OnRecalculateMaxStockAfter?.Invoke(self);
        }
        public static event Action<GenericSkill> OnRecalculateMaxStockAfter;
        public static event Action OnCombatDirectorInitBefore;
        private static void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            OnCombatDirectorInitBefore?.Invoke();
            orig();
            OnCombatDirectorInitAfter?.Invoke();
        }
        public static event Action OnCombatDirectorInitAfter;
        public static event Action<PurchaseInteraction> OnPurchaseInteractionEnableBefore;
        private static void PurchaseInteraction_OnEnable(On.RoR2.PurchaseInteraction.orig_OnEnable orig, PurchaseInteraction self)
        {   
            OnPurchaseInteractionEnableBefore?.Invoke(self);
            orig(self);
            OnPurchaseInteractionEnableAfter?.Invoke(self);
        }
        public static event Action<PurchaseInteraction> OnPurchaseInteractionEnableAfter;
        private static void GenericSkill_Awake(On.RoR2.GenericSkill.orig_Awake orig, GenericSkill self)
        {
            if (self.skillFamily == null) return;
            orig(self);
        }

        public static event Action<HealthComponent, DamageInfo, CharacterBody> OnTakeDamageProcessBefore;
        private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            CharacterBody characterBody = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
            OnTakeDamageProcessBefore?.Invoke(self, damageInfo, characterBody);
            orig(self, damageInfo);
        }

        public static event Action<DotController, DotController.DotIndex, float> OnEvaluateDotStacksForTypeBefore;
        private static void DotController_EvaluateDotStacksForType(On.RoR2.DotController.orig_EvaluateDotStacksForType orig, DotController self, DotController.DotIndex dotIndex, float dt, out int remainingActive)
        {
            OnEvaluateDotStacksForTypeBefore?.Invoke(self, dotIndex, dt);
            orig(self, dotIndex, dt, out remainingActive);
            OnEvaluateDotStacksForTypeAfter?.Invoke(self, dotIndex, dt);
        }
        public static event Action<DotController, DotController.DotIndex, float> OnEvaluateDotStacksForTypeAfter;
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

        private static void CharacterMaster_OnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
        {
            throw new NotImplementedException();
        }

        public static event Action<CharacterBody> OnInventoryChangedBefore;
        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            OnInventoryChangedBefore?.Invoke(self);
            orig(self);
            OnInventoryChangedAfter?.Invoke(self);
        }
        public static event Action<CharacterBody> OnInventoryChangedAfter;
        public static event Action<GlobalEventManager, DamageReport, CharacterBody, CharacterBody> OnCharacterDeathBefore;
        private static void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            OnCharacterDeathBefore?.Invoke(self, damageReport, damageReport.attackerBody, damageReport.victimBody);
            orig(self, damageReport);
            OnCharacterDeathAfter?.Invoke(self, damageReport, damageReport.attackerBody, damageReport.victimBody);
        }
        public static event Action<GlobalEventManager, DamageReport, CharacterBody, CharacterBody> OnCharacterDeathAfter;
    }
}
