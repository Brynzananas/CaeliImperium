using System;
using System.Collections.Generic;
using System.Text;
using CaeliImperium;
using RoR2;
using static CaeliImperium.Hooks;
using static CaeliImperium.Items;
using static CaeliImperium.Buffs;
using static CaeliImperium.Utils;
using static R2API.RecalculateStatsAPI;
using UnityEngine.Networking;
using Rewired;
using UnityEngine;
using RoR2.Navigation;
using Mono.Cecil;
using R2API.Utils;
using System.Security.Cryptography;
using System.Linq;
using System.Runtime.InteropServices;
using BrynzaAPI;
using R2API;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Numerics;

namespace CaeliImperium
{
    public class Events
    {
        public static void CritUpgradeOnKillEvents(ItemDef itemDef)
        {
            OnCharacterDeath += Events_OnCharacterDeathAfter;
            void Events_OnCharacterDeathAfter(GlobalEventManager arg1, DamageReport arg2, CharacterBody attackerBody, CharacterBody victimBody)
            {
                if (!NetworkServer.active) return;
                if (attackerBody == null) return;
                if (attackerBody.inventory == null) return;
                int itemCount = attackerBody.inventory.GetItemCount(CritUpgradeOnKill);
                if (itemCount > 0)
                {
                    int rolls = SuperRoll(itemCount * 5f);
                    for(int i = 0; i < rolls; i++)
                    attackerBody.AddBuff(IncreaseCritChanceAndDamage);
                }
            }
            GetStatCoefficients += Events_GetStatCoefficients;
            void Events_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
            {
                int buffCount = sender ? sender.GetBuffCount(IncreaseCritChanceAndDamage) : 0;
                args.critAdd += buffCount * 5f;
                args.critDamageMultAdd += buffCount * 0.05f;
            }
        }
        public static void ExtraEquipmentSlotEvents(ItemDef itemDef)
        {
            OnInventoryChanged += Events_OnInventoryChangedAfter;
            IL.RoR2.EquipmentSlot.MyFixedUpdate += EquipmentSlot_MyFixedUpdate;
            void EquipmentSlot_MyFixedUpdate(ILContext il)
            {
                ILCursor c = new ILCursor(il);
                ILLabel iLLabel = null;
                int newVariable = il.Body.Variables.Count;
                il.Body.Variables.Add(new(il.Import(typeof(bool))));
                if (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchCall(typeof(EquipmentSlot).GetPropertyGetter(nameof(EquipmentSlot.characterBody))),
                    x => x.MatchCallvirt(typeof(CharacterBody).GetPropertyGetter(nameof(CharacterBody.isEquipmentActivationAllowed))),
                    x => x.MatchStloc(out _),
                    x => x.MatchLdloc(out _),
                    x => x.MatchAnd(),
                    x => x.MatchBrfalse(out iLLabel)
                    ))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate(OpenPicker);
                    bool OpenPicker(EquipmentSlot equipmentSlot)
                    {
                        Inventory inventory = equipmentSlot.inventory;
                        if (inventory && inventory.GetItemCount(itemDef) > 0)
                        {
                            EquipmentPicker equipmentPicker = ExtraEquipmentSlotBehaviour.equipmentPicker;
                            if (equipmentPicker && !equipmentPicker.gameObject.activeSelf)
                            {
                                equipmentPicker.gameObject.SetActive(true);
                                return true;
                            }
                            else
                            {
                                bool flag = equipmentPicker.blockEquipmentUse;
                                equipmentPicker.gameObject.SetActive(false);
                                return flag;
                            }
                        }
                        return false;
                    }
                    c.Emit(OpCodes.Brtrue_S, iLLabel);
                }
                else
                {
                    Debug.LogError(il.Method + "IL Hook failed");
                }
            }
            void Events_OnInventoryChangedAfter(CharacterBody characterBody)
            {
                if (characterBody.inventory && characterBody.master && characterBody.inventory.GetItemCount(itemDef) > 0)
                {
                    ExtraEquipmentSlotBehaviour containedPotentialComponent = characterBody.masterObject.GetOrAddComponent<ExtraEquipmentSlotBehaviour>();
                }
            }
        }
        public static void FireBulletOnPrimarySkillEvents(ItemDef itemDef)
        {
            OnInventoryChanged += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(itemDef) : 0;
                obj.AddItemBehavior<FireBulletOnPrimarySkillBehaviour>(stacks);
            }
        }
        public static void PeriodicDamageIncreaseEvents(ItemDef itemDef)
        {
            OnInventoryChanged += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(itemDef) : 0;
                obj.AddItemBehavior<PeriodicDamageIncreaseBehaviour>(stacks);
            }
            GetStatCoefficients += Events_GetStatCoefficients;
            void Events_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
            {
                int buffCount = sender ? sender.GetBuffCount(IncreaseDamagePereodically) : 0;
                args.damageMultAdd += buffCount / 2;
            }
        }
        public static void StunEnemyOnItsAttackEvents(ItemDef itemDef)
        {
            //On.RoR2.GenericSkill.ExecuteIfReady += GenericSkill_ExecuteIfReady;
            On.RoR2.Skills.SkillDef.OnExecute += SkillDef_OnExecute;
            bool GenericSkill_ExecuteIfReady(On.RoR2.GenericSkill.orig_ExecuteIfReady orig, GenericSkill self)
            {
                int itemCount = Util.GetItemCountGlobal(itemDef.itemIndex, true) - (self.characterBody.teamComponent ? Util.GetItemCountForTeam(self.characterBody.teamComponent.teamIndex, itemDef.itemIndex, true) : 0);
                if (itemCount <= 0) return orig(self);
                bool rolled = Util.CheckRoll(Utils.ConvertAmplificationPercentageIntoReductionPercentage(itemCount * 15, 50));
                if (!rolled) return orig(self);
                EffectData effectData = new EffectData
                {
                    origin = self.characterBody.corePosition,
                };
                EffectManager.SpawnEffect(Assets.stunEffect, effectData, true);
                if (!self.stateMachine) return false;
                self.stateMachine.state = new EntityStates.StunState { stunDuration = 1f};
                return false;
            }
            void SkillDef_OnExecute(On.RoR2.Skills.SkillDef.orig_OnExecute orig, RoR2.Skills.SkillDef self, GenericSkill skillSlot)
            {
                int itemCount = Util.GetItemCountGlobal(itemDef.itemIndex, true) - (skillSlot.characterBody.teamComponent ? Util.GetItemCountForTeam(skillSlot.characterBody.teamComponent.teamIndex, itemDef.itemIndex, true) : 0);
                if (itemCount <= 0) { orig(self, skillSlot); return; }
                bool rolled = Util.CheckRoll(Utils.ConvertAmplificationPercentageIntoReductionPercentage(itemCount * 15, 50));
                if (!rolled) { orig(self, skillSlot); return; }
                EffectData effectData = new EffectData
                {
                    origin = skillSlot.characterBody.corePosition,
                };
                EffectManager.SpawnEffect(Assets.stunEffect, effectData, true);
                if (!skillSlot.stateMachine) return;
                EntityStates.StunState stunState = new EntityStates.StunState();
                stunState.stunDuration = 1f;
                //skillSlot.stateMachine.state = stunState;
                skillSlot.stateMachine.SetInterruptState(stunState, EntityStates.InterruptPriority.Stun);
                return;
            }
        }
        public static void NecronomiconEvents(EquipmentDef equip)
        {
            OnCharacterDeath += Events_OnCharacterDeathAfter;
            void Events_OnCharacterDeathAfter(GlobalEventManager arg1, DamageReport arg2, CharacterBody attackerBody, CharacterBody victimBody)
            {
                GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(victimBody.bodyIndex);
                GameObject masterPrefab = MasterCatalog.GetMasterPrefab(victimBody.master.masterIndex);
                if (masterPrefab == null) return;
                GameObject gameObject = new GameObject("DeadBody(" + BodyCatalog.GetBodyName(victimBody.bodyIndex) + ")");
                DeadBodyComponent deadBodyComponent = gameObject.AddComponent<DeadBodyComponent>();
                deadBodyComponent.bodyPrefab = bodyPrefab;
                deadBodyComponent.bodyName = bodyPrefab.name;
                deadBodyComponent.inventory = victimBody.inventory;
                deadBodyComponent.masterPrefab = masterPrefab;
                gameObject.transform.position = victimBody.transform.position;
                gameObject.transform.rotation = victimBody.transform.rotation;
                //deadBodyComponent.loadout = 
            }
            equipmentActions.Add(equip, FireNecronomicon);
            bool FireNecronomicon(EquipmentSlot equipmentSlot, EquipmentDef equipmentDef)
            {
                int i = 0;
                while(deadBodyComponents.Count > 0)
                for (i = 0; i < deadBodyComponents.Count; i++)
                {
                    DeadBodyComponent deadBodyComponent = deadBodyComponents[i];
                    if (deadBodyComponent != null)
                    {
                        deadBodyComponent.Revive(equipmentSlot.characterBody, true);
                        i++;
                    }
                    else
                    {
                        deadBodyComponents.Remove(deadBodyComponent);
                    }
                }
                if(i > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
        }
        public static List<DeadBodyComponent> deadBodyComponents = new List<DeadBodyComponent>();
        public static void ChargeAtomicBeamOnSpecialSkillEvents(ItemDef item)
        {
            OnInventoryChanged += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(item) : 0;
                obj.AddItemBehavior<AtomicHeartComponent>(stacks);
            }
        }
        public static void CopyNearbyCharactersSkillsOnDeathEvents(ItemDef item)
        {
            OnInventoryChanged += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(item) : 0;
                obj.AddItemBehavior<SewingMachineComponent>(stacks);
            }
            OnCharacterDeath += Events_OnCharacterDeathAfter;
            void Events_OnCharacterDeathAfter(GlobalEventManager arg1, DamageReport arg2, CharacterBody attackerBody, CharacterBody victimBody)
            {
                if (!attackerBody || !attackerBody.inventory || attackerBody.inventory.GetItemCount(item) <= 0) return;
                if (!victimBody.skillLocator) return;

                SewingMachineComponent geneModificationComponent = attackerBody.GetComponent<SewingMachineComponent>();
                if (geneModificationComponent != null)
                    foreach (GenericSkill genericSkill in victimBody.skillLocator.allSkills)
                    {
                        GenericSkill genericSkill1 = Utils.CopyGenericSkill(genericSkill, attackerBody, "Sewed");
                        if (genericSkill1 != null)
                        {
                            if (victimBody.skillLocator.primary && genericSkill == victimBody.skillLocator.primary)
                            {
                                AddSkill(1);
                                if (attackerBody.skillLocator && attackerBody.skillLocator.primary)
                                {
                                    //attackerBody.skillLocator.primary.AddAlongsideSkill(genericSkill1);
                                    genericSkill1.LinkSkill(attackerBody.skillLocator.primary);
                                }
                                    
                            }
                            if (victimBody.skillLocator.secondary && genericSkill == victimBody.skillLocator.secondary)
                            {
                                AddSkill(2);
                                if (attackerBody.skillLocator && attackerBody.skillLocator.secondary)
                                {
                                    //attackerBody.skillLocator.secondary.AddAlongsideSkill(genericSkill1);
                                    genericSkill1.LinkSkill(attackerBody.skillLocator.secondary);
                                }
                            }
                            if (victimBody.skillLocator.utility && genericSkill == victimBody.skillLocator.utility)
                            {
                                AddSkill(3);
                                if (attackerBody.skillLocator && attackerBody.skillLocator.utility)
                                {
                                    //attackerBody.skillLocator.utility.AddAlongsideSkill(genericSkill1);
                                    genericSkill1.LinkSkill(attackerBody.skillLocator.utility);
                                }
                            }
                            if (victimBody.skillLocator.special && genericSkill == victimBody.skillLocator.special)
                            {
                                AddSkill(4);
                                if (attackerBody.skillLocator && attackerBody.skillLocator.special)
                                {
                                    //attackerBody.skillLocator.special.AddAlongsideSkill(genericSkill1);
                                    genericSkill1.LinkSkill(attackerBody.skillLocator.special);
                                }
                            }
                            void AddSkill(int id)
                            {
                                if (geneModificationComponent.keyValuePairs.ContainsKey(id))
                                {
                                    geneModificationComponent.keyValuePairs[id].Add(genericSkill1);
                                }
                                else
                                {
                                    List<GenericSkill> list = new List<GenericSkill>();
                                    list.Add(genericSkill1);
                                    geneModificationComponent.keyValuePairs.Add(id, list);
                                }
                            }
                            geneModificationComponent.skillList.Add(genericSkill1);
                        }
                    }
            }
        }

        public static void ImproveHealingAndRegenEvents(ItemDef item)
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            void GlobalEventManager_onServerDamageDealt(DamageReport obj)
            {
                int itemCount = obj.victimBody && obj.victimBody.inventory ? obj.victimBody.inventory.GetItemCount(item) : 0;
                if (itemCount > 0 && obj.victimBody.healthComponent)
                {
                    obj.victimBody.healthComponent.Heal(obj.damageDealt * 0.1f * itemCount, default);
                }
            }
        }

        public static void IrradiatedDotBehaviour(DotController self, DotController.DotStack dotStack)
        {
        }
        public static float irradiatedRadius = 12f;
        public static void IrradiatedDotEvaluation(DotController self, DotController.PendingDamage pendingDamage)
        {
            BlastAttack blastAttack = new BlastAttack
            {
                attacker = pendingDamage.attackerObject,
                inflictor = pendingDamage.attackerObject,
                teamIndex = TeamComponent.GetObjectTeam(pendingDamage.attackerObject),
                baseDamage = pendingDamage.totalDamage,
                baseForce = 0f,
                position = self.victimBody.corePosition,
                radius = irradiatedRadius,
                damageColorIndex = DamageColorIndex.Nearby,
                falloffModel = BlastAttack.FalloffModel.None,
            };
            blastAttack.Fire();
            EffectData effectData = new EffectData
            {
                origin = blastAttack.position,
                scale = blastAttack.radius,
            };
            EffectManager.SpawnEffect(Assets.igniteOnkillExplosion, effectData, true);
        }
        public static void SummonMercenaryEvents(ItemDef item)
        {
            OnInventoryChanged += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                if(NetworkServer.instance == null) return;
                int stacks = obj.inventory ? obj.inventory.GetItemCount(item) : 0;
                obj.AddItemBehavior<SummonMercenaryComponent>(stacks);
            }
        }
        public static void TransferDamageOwnershipEvents(ItemDef item)
        {
            OnTakeDamageProcess += Events_OnTakeDamageProcessBefore;
            void Events_OnTakeDamageProcessBefore(HealthComponent arg1, DamageInfo arg2, CharacterBody attackerBody)
            {
                if (attackerBody && attackerBody.master && attackerBody.master.minionOwnership && attackerBody.master.minionOwnership.ownerMaster && attackerBody.inventory && attackerBody.inventory.GetItemCount(item) > 0)
                {
                    arg2.attacker = attackerBody.master.minionOwnership.ownerMaster.GetBodyObject();
                    arg2.damageColorIndex = DamageColorIndex.Item;
                }
            }
        }
        public static void DuplicateMainSkillsEvents(ItemDef item)
        {
            OnInventoryChanged += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(item) : 0;
                obj.AddItemBehavior<BrassKnucklesComponent>(stacks);
            }
        }
        public static void DamageAllEnemiesEvents(ItemDef item)
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            void GlobalEventManager_onServerDamageDealt(DamageReport obj)
            {
                if (obj.damageInfo.procChainMask.HasProc(ProcType.AACannon)) return;
                int itemCount = obj.attackerBody && obj.attackerBody.inventory ? obj.attackerBody.inventory.GetItemCount(item) : 0;
                if (itemCount > 0)
                {
                    for (int i = 0; i < CharacterBody.readOnlyInstancesList.Count; i++)
                    {
                        CharacterBody characterBody = CharacterBody.readOnlyInstancesList[i];
                        if (characterBody != null && characterBody != obj.victimBody && characterBody.teamComponent && characterBody.teamComponent.teamIndex != obj.attackerBody.teamComponent.teamIndex)
                        {
                            ProcChainMask procChainMask = new ProcChainMask();
                            procChainMask.AddProc(ProcType.AACannon);
                            DamageInfo damageInfo = new DamageInfo
                            {
                                attacker = obj.attacker,
                                canRejectForce = true,
                                crit = obj.damageInfo.crit,
                                damageColorIndex = DamageColorIndex.Item,
                                damageType = DamageTypeCombo.Generic,
                                damage = obj.damageDealt * 0.1f * itemCount,
                                inflictor = obj.attacker,
                                position = characterBody.corePosition,
                                procChainMask = procChainMask,
                                procCoefficient = 0f,
                            };
                            characterBody.healthComponent.TakeDamageProcess(damageInfo);
                        }
                    }
                }
            }
        }
        private static bool TaoRegistered;
        public static void TaoEvents(ItemDef itemDef)
        {
            if (TaoRegistered) return;
            TaoRegistered = true;
            OnInventoryChanged += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(WoundEnemyOnContiniousHits) + obj.inventory.GetItemCount(DropHealOrbsOnContiniousHits) : 0;
                obj.AddItemBehavior<TaoArtifactsComponent>(stacks);
            }
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            void GlobalEventManager_onServerDamageDealt(DamageReport obj)
            {
               TaoArtifactsComponent taoArtifactsComponent = obj.attackerBody && obj.attackerBody.inventory && obj.attackerBody.inventory.GetItemCount(WoundEnemyOnContiniousHits) + obj.attackerBody.inventory.GetItemCount(DropHealOrbsOnContiniousHits) > 0 ? obj.attacker.GetComponent<TaoArtifactsComponent>() : null;
                if (taoArtifactsComponent != null) taoArtifactsComponent.RegisterHit(obj);
            }
        }
        public static void TeleportAroundOpenedChestsEvents(ItemDef item)
        {
            OnInventoryChanged += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(item) : 0;
                obj.AddItemBehavior<ChalkComponent>(stacks);
            }
            OnPurchaseInteractionEnable += Events_OnPurchaseInteractionEnableAfter;
            void Events_OnPurchaseInteractionEnableAfter(PurchaseInteraction obj)
            {
                GameObject gameObject = new GameObject("Wormhole");
                gameObject.transform.SetParent(obj.transform, false);
                SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.radius = 1f;
                gameObject.layer = LayerIndex.noCollision.intVal;
                WormholeComponent wormholeComponent = gameObject.AddComponent<WormholeComponent>();
            }
        }
        public static void NegativeLuckEvents(ItemDef item)
        {
            void Events_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
            {
                int itemCount = sender && sender.inventory ? sender.inventory.GetItemCount(item) : 0;
                if (itemCount > 0)
                {
                    args.luckReductionAdd += itemCount;
                    args.luckReductionMult += itemCount;
                }
            }
            GetStatCoefficients += Events_GetStatCoefficients;
        }
        public static void HastingEvents(EliteDef eliteDef)
        {
            GetStatCoefficients += Events_GetStatCoefficients;
            void Events_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
            {
                if (!sender.HasBuff(AffixSpeedster)) return;
                args.moveSpeedMultAdd += 1;
                args.primaryCooldownMultAdd -= 0.5f;
                args.secondaryCooldownMultAdd -= 0.5f;
                args.utilityCooldownMultAdd -= 0.5f;
                args.specialCooldownMultAdd -= 0.5f;
                args.attackSpeedMultAdd += 1f;
            }
        }
    }
}
