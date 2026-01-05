using EntityStates.AffixVoid;
using HG;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using static RoR2.DotController;

namespace CaeliImperium
{
    public static class Hooks
    {
        private static int _OnBuffFinalStackLostHookAdded;
        private static event Action<CharacterBody, BuffDef> _OnBuffFinalStackLost;
        public static event Action<CharacterBody, BuffDef> OnBuffFinalStackLost
        {
            add
            {
                if (_OnBuffFinalStackLostHookAdded == 0) On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
                _OnBuffFinalStackLost += value;
                _OnBuffFinalStackLostHookAdded++;
            }
            remove
            {
                if (_OnBuffFinalStackLostHookAdded == 1) On.RoR2.CharacterBody.OnBuffFinalStackLost -= CharacterBody_OnBuffFinalStackLost;
                _OnBuffFinalStackLost -= value;
                _OnBuffFinalStackLostHookAdded--;
            }
        }
        private static void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            try
            {
                _OnBuffFinalStackLost?.Invoke(self, buffDef);
            }
            catch (Exception e)
            {
            }
        }
        private static int _OnBuffFirstStackGainedHookAdded;
        private static event Action<CharacterBody, BuffDef> _OnBuffFirstStackGained;
        public static event Action<CharacterBody, BuffDef> OnBuffFirstStackGained
        {
            add
            {
                if (_OnBuffFirstStackGainedHookAdded == 0) On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
                _OnBuffFirstStackGained += value;
                _OnBuffFirstStackGainedHookAdded++;
            }
            remove
            {
                if (_OnBuffFirstStackGainedHookAdded == 1) On.RoR2.CharacterBody.OnBuffFirstStackGained -= CharacterBody_OnBuffFirstStackGained;
                _OnBuffFirstStackGained -= value;
                _OnBuffFirstStackGainedHookAdded--;
            }
        }
        private static void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            try
            {
                _OnBuffFirstStackGained?.Invoke(self, buffDef);
            }
            catch (Exception ex)
            {
            }
        }
        private static void GenericSkill_RecalculateMaxStock(On.RoR2.GenericSkill.orig_RecalculateMaxStock orig, GenericSkill self)
        {
            orig(self);
            try
            {
                _OnRecalculateMaxStock?.Invoke(self);
            }
            catch (Exception ex)
            {
            }
        }
        private static int _OnRecalculateMaxStockHookAdded;
        private static event Action<GenericSkill> _OnRecalculateMaxStock;
        public static event Action<GenericSkill> OnRecalculateMaxStock
        {
            add
            {
                if (_OnRecalculateMaxStockHookAdded == 0) On.RoR2.GenericSkill.RecalculateMaxStock += GenericSkill_RecalculateMaxStock;
                _OnRecalculateMaxStock += value;
                _OnRecalculateMaxStockHookAdded++;
            }
            remove
            {
                if (_OnRecalculateMaxStockHookAdded == 1) On.RoR2.GenericSkill.RecalculateMaxStock -= GenericSkill_RecalculateMaxStock;
                _OnRecalculateMaxStock -= value;
                _OnRecalculateMaxStockHookAdded--;
            }
        }
        private static void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();
            try
            {
                _OnCombatDirectorInit?.Invoke();
            }
            catch (Exception ex)
            {
            }
        }
        private static int _OnCombatDirectorInitHookAdded;
        private static event Action _OnCombatDirectorInit;
        public static event Action OnCombatDirectorInit
        {
            add
            {
                if (_OnCombatDirectorInitHookAdded == 0) On.RoR2.CombatDirector.Init += CombatDirector_Init;
                _OnCombatDirectorInit += value;
                _OnRecalculateMaxStockHookAdded++;
            }
            remove
            {
                if (_OnCombatDirectorInitHookAdded == 1) On.RoR2.CombatDirector.Init -= CombatDirector_Init;
                _OnCombatDirectorInit -= value;
                _OnRecalculateMaxStockHookAdded--;
            }
        }
        private static void PurchaseInteraction_OnEnable(On.RoR2.PurchaseInteraction.orig_OnEnable orig, PurchaseInteraction self)
        {
            orig(self);
            try
            {
                _OnPurchaseInteractionEnable?.Invoke(self);
            }
            catch (Exception ex)
            {
            }
        }
        private static int _OnPurchaseInteractionEnableHookAdded;
        private static event Action<PurchaseInteraction> _OnPurchaseInteractionEnable;
        public static event Action<PurchaseInteraction> OnPurchaseInteractionEnable
        {
            add
            {
                if (_OnPurchaseInteractionEnableHookAdded == 0) On.RoR2.PurchaseInteraction.OnEnable += PurchaseInteraction_OnEnable;
                _OnPurchaseInteractionEnable += value;
                _OnPurchaseInteractionEnableHookAdded++;
            }
            remove
            {
                if (_OnPurchaseInteractionEnableHookAdded == 1) On.RoR2.PurchaseInteraction.OnEnable -= PurchaseInteraction_OnEnable;
                _OnPurchaseInteractionEnable -= value;
                _OnPurchaseInteractionEnableHookAdded--;
            }
        }
        private static int _OnTakeDamageProcessHookAdded;
        private static event Action<HealthComponent, DamageInfo, CharacterBody> _OnTakeDamageProcess;
        public static event Action<HealthComponent, DamageInfo, CharacterBody> OnTakeDamageProcess
        {
            add
            {
                if (_OnTakeDamageProcessHookAdded == 0) On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
                _OnTakeDamageProcess += value;
                _OnTakeDamageProcessHookAdded++;
            }
            remove
            {
                if (_OnTakeDamageProcessHookAdded == 1) On.RoR2.HealthComponent.TakeDamageProcess -= HealthComponent_TakeDamageProcess;
                _OnTakeDamageProcess -= value;
                _OnTakeDamageProcessHookAdded--;
            }
        }
        private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            CharacterBody characterBody = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
            _OnTakeDamageProcess?.Invoke(self, damageInfo, characterBody);
            orig(self, damageInfo);
        }
        private static bool _equipmentActionsAdded;
        public delegate bool PerformEquipmentAction(EquipmentSlot equipmentSlot, EquipmentDef equipmentDef);
        private static Dictionary<EquipmentDef, PerformEquipmentAction> _equipmentActions = [];
        public static Dictionary<EquipmentDef, PerformEquipmentAction> equipmentActions
        {
            get => _equipmentActions;
            set
            {
                if (!_equipmentActionsAdded)
                {
                    On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
                    _equipmentActionsAdded = true;
                }
                _equipmentActions = value;
            }
        }
        private static bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            try
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
            }
            catch (Exception ex)
            {
            }
            return orig(self, equipmentDef);
        }

        private static void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            try
            {
                _OnInventoryChanged?.Invoke(self);
            }
            catch (Exception ex)
            {
            }
        }
        private static int _OnInventoryChangedHookAdded;
        private static event Action<CharacterBody> _OnInventoryChanged;
        public static event Action<CharacterBody> OnInventoryChanged
        {
            add
            {
                if (_OnInventoryChangedHookAdded == 0) On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
                _OnInventoryChanged += value;
                _OnInventoryChangedHookAdded++;
            }
            remove
            {
                if (_OnInventoryChangedHookAdded == 1) On.RoR2.CharacterBody.OnInventoryChanged -= CharacterBody_OnInventoryChanged;
                _OnInventoryChanged -= value;
                _OnInventoryChangedHookAdded--;
            }
        }
    }
}
