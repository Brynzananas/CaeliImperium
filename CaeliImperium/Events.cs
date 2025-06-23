using System;
using System.Collections.Generic;
using System.Text;
using CaeliImperium;
using RoR2;
using static CaeliImperium.Hooks;
using static CaeliImperium.Items;
using static CaeliImperium.Buffs;
using static CaeliImperium.Utils;
using static CaeliImperium.Components;
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

namespace CaeliImperium
{
    public class Events
    {
        public static void KeychainEvents(ItemDef itemDef)
        {
            OnCharacterDeathAfter += Events_OnCharacterDeathAfter;
            void Events_OnCharacterDeathAfter(GlobalEventManager arg1, DamageReport arg2, CharacterBody attackerBody, CharacterBody victimBody)
            {
                if (!NetworkServer.active) return;
                if (attackerBody == null) return;
                if (attackerBody.inventory == null) return;
                int itemCount = attackerBody.inventory.GetItemCount(Keychain);
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
        public static void ContainedPotentialEvent(ItemDef itemDef)
        {
            OnInventoryChangedAfter += Events_OnInventoryChangedAfter;
            //On.RoR2.EquipmentSlot.ExecuteIfReady += EquipmentSlot_ExecuteIfReady;
            //On.EntityStates.GenericCharacterMain.CanExecuteSkill += GenericCharacterMain_CanExecuteSkill;
            bool EquipmentSlot_ExecuteIfReady(On.RoR2.EquipmentSlot.orig_ExecuteIfReady orig, EquipmentSlot self)
            {
                if (self.characterBody && self.characterBody.master && self.characterBody.inventory && self.inventory.GetItemCount(ContainedPotential) > 0)
                {
                    ContainedPotentialComponent containedPotentialComponent = self.characterBody.masterObject.GetComponent<ContainedPotentialComponent>();
                    if (containedPotentialComponent)
                        if (containedPotentialComponent.canExecuteEquipment)
                        {
                            return orig(self);
                        }
                        else
                        {
                            containedPotentialComponent.OpenWheel();
                            return false;
                        }

                }
                return orig(self);
            }
            bool GenericCharacterMain_CanExecuteSkill(On.EntityStates.GenericCharacterMain.orig_CanExecuteSkill orig, EntityStates.GenericCharacterMain self, GenericSkill skillSlot)
            {
                if (self.characterBody && self.characterBody.master && self.characterBody.inventory && self.characterBody.inventory.GetItemCount(ContainedPotential) > 0)
                {
                    ContainedPotentialComponent containedPotentialComponent = self.characterBody.masterObject.GetComponent<ContainedPotentialComponent>();
                    if (containedPotentialComponent && self.skillLocator && self.inputBank)
                    {
                        if (!self.inputBank.ping.down)
                        {
                            return orig(self, skillSlot);
                        }
                        else
                        {
                            if (skillSlot == self.skillLocator.primary)
                            {
                                containedPotentialComponent.MoveEquipmentUpwards();
                                return false;
                            }
                            if (skillSlot == self.skillLocator.secondary)
                            {
                                containedPotentialComponent.MoveEquipmentDownwards();
                                return false;
                            }
                            return orig(self, skillSlot);
                        }
                    }
                }
                return orig(self, skillSlot);
            }
            void Events_OnInventoryChangedAfter(CharacterBody characterBody)
            {
                if (characterBody.inventory && characterBody.master && characterBody.inventory.GetItemCount(ContainedPotential) > 0)
                {
                    ContainedPotentialComponent containedPotentialComponent = characterBody.masterObject.GetOrAddComponent<ContainedPotentialComponent>();
                }
            }
        }
        public static void SkullMachineGunEvents(ItemDef itemDef)
        {
            OnInventoryChangedAfter += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(itemDef) : 0;
                obj.AddItemBehavior<SkullMachineGunComponent>(stacks);
            }
        }
        public static void CopperBellEvents(ItemDef itemDef)
        {
            OnInventoryChangedAfter += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(itemDef) : 0;
                obj.AddItemBehavior<CopperBellComponent>(stacks);
            }
            GetStatCoefficients += Events_GetStatCoefficients;
            void Events_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
            {
                int buffCount = sender ? sender.GetBuffCount(IncreaseDamagePereodically) : 0;
                args.damageMultAdd += buffCount / 2;
            }
        }
        public static void GuardianFleshEvents(ItemDef itemDef)
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
            OnCharacterDeathAfter += Events_OnCharacterDeathAfter;
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
        public static void AtomicHeartEvents(ItemDef item)
        {
            OnInventoryChangedAfter += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(item) : 0;
                obj.AddItemBehavior<AtomicHeartComponent>(stacks);
            }
        }
        public static void GeneModificationEvents(ItemDef item)
        {
            OnInventoryChangedAfter += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(item) : 0;
                obj.AddItemBehavior<SewingMachineComponent>(stacks);
            }
            OnCharacterDeathAfter += Events_OnCharacterDeathAfter;
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

        public static void MedicineEvents(ItemDef item)
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

        public static void IrradiatedDotBehaviour(RoR2.DotController self, RoR2.DotController.DotStack dotStack)
        {
            dotStack.dotIndex = IrradiatedDotIndex;
            dotStack.attackerObject = null;
            dotStack.attackerTeam = TeamIndex.Neutral;
            dotStack.timer = 0f;
            dotStack.damage = 0f;
            dotStack.damageType = default(DamageTypeCombo);
        }
        public static void IrradiatedEvents(DotController.DotDef dotDef)
        {
            OnEvaluateDotStacksForTypeBefore += Events_OnEvaluateDotStacksForTypeBefore;
            void Events_OnEvaluateDotStacksForTypeBefore(DotController arg1, DotController.DotIndex arg2, float arg3)
            {
                if (arg2 == IrradiatedDotIndex)
                {
                    for (int i = 0; i < arg1.dotStackList.Count; i++)
                    {
                        DotController.DotStack dotStack = arg1.dotStackList[i];
                        if(dotStack.dotIndex == IrradiatedDotIndex)
                        {
                            Chat.AddMessage("Explode");
                            BlastAttack blastAttack = new BlastAttack
                            {
                                attacker = dotStack.attackerObject,
                                attackerFiltering = AttackerFiltering.Default,
                                baseDamage = dotStack.damage,
                                baseForce = 0f,
                                damageColorIndex = DamageColorIndex.Poison,
                                damageType = DamageTypeCombo.Generic,
                                losType = BlastAttack.LoSType.None,
                                inflictor = arg1.gameObject,
                                falloffModel = BlastAttack.FalloffModel.None,
                                position = arg1.victimBody.corePosition,
                                radius = 6f,
                                procCoefficient = 0f,
                                teamIndex = dotStack.attackerTeam,
                            };
                            BlastAttack.Result result = blastAttack.Fire();
                        }
                        
                    }
                    EffectData effectData = new EffectData
                    {
                        origin = arg1.victimBody.corePosition,
                        scale = 6f
                    };
                    EffectManager.SpawnEffect(Assets.igniteOnkillExplosion, effectData, true);
                }
            }
        }
        public static void PossessedSwordEvents(ItemDef item)
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            void GlobalEventManager_onServerDamageDealt(DamageReport obj)
            {
                int itemCount = obj.attackerBody && obj.attackerBody.inventory ? obj.attackerBody.inventory.GetItemCount(item) : 0;
                if (itemCount > 0)
                {
                    float chance = 1 * (obj.attackerBody.GetBuffCount(IncreaseMercenaryCloneSummonChance) + 1);
                    bool roll = Util.CheckRoll(chance);
                    if (roll)
                    {
                        Vector3 destination = obj.damageInfo.position;
                        Vector3 startPosition = obj.damageInfo.position;
                        NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                        NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(startPosition, HullClassification.Human, float.PositiveInfinity);
                        groundNodes.GetNodePosition(nodeIndex, out destination);
                        CharacterMaster characterMaster = new MasterSummon
                        {
                            position = destination,
                            ignoreTeamMemberLimit = true,
                            masterPrefab = Assets.mercMaster,
                            summonerBodyObject = obj.attacker,
                            //teamIndexOverride = TeamIndex.Player,
                            rotation = Quaternion.identity,
                            //inventoryToCopy = inventory,
                            //loadout = loadout,
                        }.Perform();
                        if (characterMaster)
                        {
                            EntityStateMachine entityStateMachine = characterMaster.GetBody() ? characterMaster.GetBody().GetComponent<EntityStateMachine>() : null;
                            if (entityStateMachine != null)
                            {
                                entityStateMachine.SetNextStateToMain();
                            }
                            if (characterMaster.inventory)
                            {
                                characterMaster.inventory.GiveItem(RoR2Content.Items.HealthDecay, 30);
                                characterMaster.inventory.GiveItem(RoR2Content.Items.BoostDamage, itemCount * 10);
                                characterMaster.inventory.GiveItem(RoR2Content.Items.BoostAttackSpeed, itemCount * 5);
                                characterMaster.inventory.GiveItem(RoR2Content.Items.Ghost);
                                characterMaster.inventory.GiveItem(TransferDamageOwnership);
                            }
                            
                        }
                        obj.attackerBody.SetBuffCount(IncreaseMercenaryCloneSummonChance.buffIndex, 0);
                    }
                    else
                    {
                        obj.attackerBody.AddBuff(IncreaseMercenaryCloneSummonChance);
                    }
                }
            }
        }
        public static void TransferDamageOwnershipEvents(ItemDef item)
        {
            OnTakeDamageProcessBefore += Events_OnTakeDamageProcessBefore;
            void Events_OnTakeDamageProcessBefore(HealthComponent arg1, DamageInfo arg2, CharacterBody attackerBody)
            {
                if (attackerBody && attackerBody.master && attackerBody.master.minionOwnership && attackerBody.master.minionOwnership.ownerMaster && attackerBody.inventory && attackerBody.inventory.GetItemCount(item) > 0)
                {
                    arg2.attacker = attackerBody.master.minionOwnership.ownerMaster.GetBodyObject();
                    arg2.damageColorIndex = DamageColorIndex.Item;
                }
            }
        }
        public static void BrassKnucklesEvents(ItemDef item)
        {
            OnInventoryChangedAfter += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(item) : 0;
                obj.AddItemBehavior<BrassKnucklesComponent>(stacks);
            }
        }
        public static void FireAxeEvents(ItemDef item)
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
        public static void TaoManuscriptEvents(ItemDef itemDef)
        {
            OnInventoryChangedAfter += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(TaoManuscript) + obj.inventory.GetItemCount(TaoFruit) : 0;
                obj.AddItemBehavior<TaoArtifactsComponent>(stacks);
            }
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            void GlobalEventManager_onServerDamageDealt(DamageReport obj)
            {
                bool hasFlag = obj.damageInfo.damageType.damageSource.HasFlag(DamageSource.Primary);
                Debug.Log("has Flag: " + hasFlag);
                if (!hasFlag) return;
                int itemCount = obj.attackerBody && obj.attackerBody.inventory ? obj.attackerBody.inventory.GetItemCount(TaoManuscript) : 0;
                int itemCount2 = obj.attackerBody && obj.attackerBody.inventory ? obj.attackerBody.inventory.GetItemCount(TaoFruit) : 0;
                if (itemCount + itemCount2 > 0)
                {
                    obj.victimBody.AddBuff(TaoCount);
                    if (obj.victimBody.GetBuffCount(TaoCount) >= 7)
                    {
                        if(itemCount > 0)
                        {
                            DamageInfo damageInfo = new DamageInfo
                            {
                                attacker = obj.attacker,
                                canRejectForce = true,
                                crit = false,
                                damageColorIndex = DamageColorIndex.Item,
                                damageType = DamageTypeCombo.Generic,
                                damage = obj.attackerBody.damage * itemCount * 10,
                                inflictor = obj.attacker,
                                position = obj.damageInfo.position,
                                //procChainMask = procChainMask,
                                procCoefficient = 1f,
                            };
                            obj.victimBody.healthComponent.TakeDamageProcess(damageInfo);
                        }
                        if (itemCount2 > 0)
                        {
                            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TreebotFruitDeathEffect"), new EffectData
                            {
                                origin = obj.victimBody.corePosition,
                                rotation = UnityEngine.Random.rotation
                            }, true);
                            int num2 = itemCount2;
                            GameObject original = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/TreebotFruitPack");
                            for (int i = 0; i < num2; i++)
                            {
                                GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(original, obj.victimBody.corePosition + UnityEngine.Random.insideUnitSphere * obj.victimBody.radius * 0.5f, UnityEngine.Random.rotation);
                                TeamFilter component4 = gameObject4.GetComponent<TeamFilter>();
                                if (component4)
                                {
                                    component4.teamIndex = obj.attackerTeamIndex;
                                }
                                gameObject4.GetComponentInChildren<HealthPickup>();
                                gameObject4.transform.localScale = new Vector3(1f, 1f, 1f);
                                NetworkServer.Spawn(gameObject4);
                            }
                        }
                        obj.victimBody.SetBuffCount(TaoCount.buffIndex, 0);
                        
                        
                    }
                }
            }
        }
        public static void ChalkEvents(ItemDef item)
        {
            OnInventoryChangedAfter += Events_OnInventoryChangedAfter;
            void Events_OnInventoryChangedAfter(CharacterBody obj)
            {
                int stacks = obj.inventory ? obj.inventory.GetItemCount(item) : 0;
                obj.AddItemBehavior<ChalkComponent>(stacks);
            }
            OnPurchaseInteractionEnableAfter += Events_OnPurchaseInteractionEnableAfter;
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
        public static void HastingEvents(EliteDef eliteDef)
        {
            GetStatCoefficients += Events_GetStatCoefficients;
            void Events_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
            {
                if (!sender.HasBuff(AffixHasting)) return;
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
