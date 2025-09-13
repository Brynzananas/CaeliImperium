using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static CaeliImperium.Events;
using static CaeliImperium.Buffs;
using static CaeliImperium.Items;
using System.Diagnostics;
using RoR2.Navigation;
using RoR2.UI;
using UnityEngine.UI;
using BrynzaAPI;

namespace CaeliImperium
{
    public class ExtraEquipmentSlotBehaviour : MonoBehaviour
    {
        public CharacterBody characterBody;
        public CharacterMaster characterMaster;
        public bool canExecuteEquipment = false;
        public bool opened = false;
        public EquipmentIndex[] equipments = new EquipmentIndex[] { };
        public Inventory inventory;
        public EquipmentSlot equipmentSlot;
        public int itemCount = 0;
        public int previousArraySize = 0;
        public bool useEquipment = true;
        public InputBankTest inputBankTest;
        public static EquipmentPicker equipmentPicker;

        public void Awake()
        {
            characterMaster = GetComponent<CharacterMaster>();
            characterMaster.onBodyStart += CharacterMaster_onBodyStart;
            GetBodyComponents();
            if (equipmentPicker == null) equipmentPicker = Utils.CreateEquipmentPicker();
            if (equipmentPicker) equipmentPicker.extraEquipmentSlotBehaviour = this;
        }
        public void Start()
        {
            
        }
        private void CharacterMaster_onBodyStart(CharacterBody obj)
        {
            GetBodyComponents();
        }

        public void Destroy()
        {
            characterMaster.onBodyStart -= CharacterMaster_onBodyStart;
        }
        public void GetBodyComponents()
        {
            characterBody = characterMaster ? characterMaster.GetBody() : null;
            inventory = characterBody ? characterBody.inventory : null;
            if (inventory != null)
            {
                //inventory.onInventoryChanged += Inventory_onInventoryChanged;
                itemCount = characterBody.inventory.GetItemCount(Items.ExtraEquipmentSlot);
            }
            equipmentSlot = characterBody ? characterBody.equipmentSlot : null;
            inputBankTest = characterBody ? characterBody.inputBank : null;
        }
        public void FixedUpdate()
        {
            itemCount = inventory.GetItemCount(Items.ExtraEquipmentSlot);
            //if (inputBankTest)
            //{
            //    if (inputBankTest.ping.down)
            //    {
            //        if (inputBankTest.skill1.justPressed) MoveEquipmentUpwards();
            //        if (inputBankTest.skill2.justPressed) MoveEquipmentDownwards();
            //    }

            //}
            if (itemCount != equipments.Length)
            {
                if (equipments.Length < itemCount)
                {
                    int toChange = equipments.Length;
                    Array.Resize(ref equipments, itemCount);
                    for (int i = 0; i < (itemCount - toChange); i++)
                    {
                        equipments.SetValue(EquipmentIndex.None, equipments.Length - (i + 1));

                    }
                }
                else
                {
                    for (int i = 0; i < (equipments.Length - itemCount); i++)
                    {
                        EquipmentIndex pickupIndex = (EquipmentIndex)equipments.GetValue(equipments.Length - (i + 1));
                        if (pickupIndex != EquipmentIndex.None)
                        {
                            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(pickupIndex), characterBody.corePosition, Physics.gravity * -1f);

                        }

                    }
                    Array.Resize(ref equipments, itemCount);
                }
                equipmentPicker.UpdatePicker();
            }
            previousArraySize = equipments.Length;
        }
        private void Inventory_onInventoryChanged()
        {
            itemCount = characterBody ? characterBody.inventory.GetItemCount(Items.ExtraEquipmentSlot) : 0;
        }
        public void MoveEquipmentUpwards()
        {
            EquipmentIndex equipmentDef = equipments[0];
            Array.Copy(equipments, 1, equipments, 0, equipments.Length - 1);
            EquipmentIndex equipmentIndex = inventory.GetEquipmentIndex();
            equipments[equipments.Length - 1] = equipmentIndex;
            inventory.SetEquipmentIndex(equipmentDef);
            useEquipment = false;
        }
        public void MoveEquipmentDownwards()
        {
            Array.Reverse(equipments);
            EquipmentIndex equipmentDef = equipments[0];
            Array.Copy(equipments, 1, equipments, 0, equipments.Length - 1);
            EquipmentIndex equipmentIndex = inventory.GetEquipmentIndex();
            equipments[equipments.Length - 1] = equipmentIndex;
            Array.Reverse(equipments);
            inventory.SetEquipmentIndex(equipmentDef);
            useEquipment = false;
        }
        public EquipmentIndex SelectEquipment(int index)
        {
            EquipmentIndex equipmentDef = equipments[index];
            EquipmentIndex equipmentIndex = inventory.GetEquipmentIndex();
            equipments[index] = equipmentIndex;
            inventory.SetEquipmentIndex(equipmentDef);
            return equipmentIndex;
        }
    }
    public class FireBulletOnPrimarySkillBehaviour : CharacterBody.ItemBehavior
    {
        public static float windUpTime = 0.5f;
        public static float fireRate = 0.2f;
        public float stopwatch = 0f;
        public bool firing;
        public void FixedUpdate()
        {
            if (body == null) return;
            if (body.inputBank == null) return;
            if (body.inputBank.skill1.down)
            {
                if (stopwatch > 0f) stopwatch -= Time.fixedDeltaTime;
                if (stopwatch <= 0f)
                {
                    Ray ray = body.inputBank ? body.inputBank.GetAimRay() : new Ray { direction = transform.eulerAngles, origin = transform.position };
                    Util.PlaySound(EntityStates.Commando.CommandoWeapon.FirePistol2.firePistolSoundString, base.gameObject);
                    if (EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = ray.origin,
                            rotation = Quaternion.LookRotation(ray.direction),
                        };
                        EffectManager.SpawnEffect(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, effectData, true);
                    }

                    if (Util.HasEffectiveAuthority(body.networkIdentity))
                    {
                        new BulletAttack
                        {
                            owner = base.gameObject,
                            weapon = base.gameObject,
                            origin = ray.origin,
                            aimVector = ray.direction,
                            minSpread = 0f,
                            maxSpread = 0f,
                            damage = body.damage * stack,
                            force = 0f,
                            tracerEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab,
                            //muzzleName = targetMuzzle,
                            hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                            isCrit = Util.CheckRoll(body.crit, body.master),
                            radius = 0.1f,
                            smartCollision = true,
                            trajectoryAimAssistMultiplier = EntityStates.Commando.CommandoWeapon.FirePistol2.trajectoryAimAssistMultiplier,
                            damageType = DamageTypeCombo.GenericPrimary
                        }.Fire();
                    }
                    stopwatch = fireRate;
                }
            }
            else
            {
                stopwatch = windUpTime;
            };
        }
    }
    public class PeriodicDamageIncreaseBehaviour : CharacterBody.ItemBehavior
    {
        public static float cooldown = 15f;
        public float stopwatch = cooldown;
        public void FixedUpdate()
        {
            if (stopwatch > 0f) stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0f)
            {
                if (NetworkServer.active) body.AddTimedBuff(Buffs.IncreaseDamagePereodically, 2f);
                stopwatch = cooldown / stack;
            }
        }
    }
    public class DeadBodyComponent : MonoBehaviour
    {
        public Inventory inventory;
        public Loadout loadout;
        public string bodyName;
        public GameObject indicator;
        public GameObject bodyPrefab;
        public GameObject masterPrefab;
        public void Awake()
        {
            indicator = Instantiate(Assets.childProjectileGhost, transform);
            ProjectileGhostController projectileGhostController = indicator.GetComponent<ProjectileGhostController>();
            Destroy(projectileGhostController);
        }
        public void OnEnable()
        {
            deadBodyComponents.Add(this);
        }
        public void OnDisable()
        {
            deadBodyComponents.Remove(this);
        }
        public void Update()
        {
            if (NetworkUser.instancesList != null && NetworkUser.instancesList[0] && NetworkUser.instancesList[0].master && NetworkUser.instancesList[0].master.inventory && NetworkUser.instancesList[0].master.inventory.currentEquipmentIndex == Equipments.Necronomicon.equipmentIndex)
            {
                if (!indicator.activeSelf)
                    indicator.SetActive(true);
            }
            else
            {
                if (indicator.activeSelf)
                    indicator.SetActive(false);
            }
        }
        public void Revive(CharacterBody characterBody, bool destroy)
        {
            if (!NetworkServer.active && bodyPrefab) return;
            CharacterMaster characterMaster = new MasterSummon
            {
                position = transform.position,
                ignoreTeamMemberLimit = true,
                masterPrefab = masterPrefab,
                summonerBodyObject = characterBody.gameObject,
                rotation = transform.rotation,
                inventoryToCopy = inventory,
                //loadout = loadout,
            }.Perform();
            if (characterMaster && characterMaster.inventory)
            {
                characterMaster.inventory.GiveItem(RoR2Content.Items.HealthDecay, 30);
                characterMaster.inventory.GiveItem(RoR2Content.Items.Ghost);
            }
            if (destroy) Destroy(gameObject);
        }
    }
    public class AtomicHeartComponent : CharacterBody.ItemBehavior
    {
        public static float timeToFire = 1f;
        public static float maxDamageMultiplier = 100f;
        public static float dotDuration = 5f;
        public float stopwatch = 0f;
        public float charge = 0f;
        public bool fire = false;
        public void FixedUpdate()
        {
            charge += Time.fixedDeltaTime * stack;
            if (body.inputBank && body.inputBank.skill4.justPressed)
            {
                stopwatch = timeToFire / body.attackSpeed;
                fire = true;
            }
            if (stopwatch > 0f && fire) stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0f && fire)
            {
                FireLaser();
            }
        }
        public void FireLaser()
        {
            Ray ray = body.inputBank ? body.inputBank.GetAimRay() : new Ray { direction = transform.eulerAngles, origin = transform.position };
            if (Util.HasEffectiveAuthority(body.networkIdentity))
            {
                new BulletAttack
                {
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = ray.origin,
                    aimVector = ray.direction,
                    minSpread = 0f,
                    maxSpread = 0f,
                    damage = body.damage * Utils.ConvertAmplificationPercentageIntoReductionPercentage(charge, maxDamageMultiplier),
                    force = 0f,
                    tracerEffectPrefab = Assets.fireSnipeSuperTracer,
                    //muzzleName = targetMuzzle,
                    hitEffectPrefab = null,
                    isCrit = Util.CheckRoll(body.crit, body.master),
                    radius = 2f,
                    smartCollision = true,
                    trajectoryAimAssistMultiplier = EntityStates.Commando.CommandoWeapon.FirePistol2.trajectoryAimAssistMultiplier,
                    damageType = DamageTypeCombo.GenericSpecial,
                    hitCallback = IrradiateOnHit,

                }.Fire();
                bool IrradiateOnHit(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
                {
                    if (hitInfo.hitHurtBox)
                    {
                        InflictDotInfo dotInfo = new InflictDotInfo()
                        {
                            attackerObject = gameObject,
                            victimObject = hitInfo.hitHurtBox.healthComponent.gameObject,
                            totalDamage = body.damage,
                            damageMultiplier = stack,
                            duration = dotDuration,
                            dotIndex = IrradiatedDotIndex,

                        };
                        DotController.InflictDot(ref dotInfo);
                    }
                    return BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
                }
            }
            charge = 0f;
            fire = false;
        }
    }
    public class SewingMachineComponent : CharacterBody.ItemBehavior
    {
        public Dictionary<int, List<GenericSkill>> keyValuePairs = new Dictionary<int, List<GenericSkill>>();
        public List<GenericSkill> skillList = new List<GenericSkill>();
        public void OnEnable()
        {
            if (!keyValuePairs.ContainsKey(0))
                keyValuePairs.Add(0, new List<GenericSkill>());
            if (!keyValuePairs.ContainsKey(1))
                keyValuePairs.Add(1, new List<GenericSkill>());
            if (!keyValuePairs.ContainsKey(2))
                keyValuePairs.Add(2, new List<GenericSkill>());
            if (!keyValuePairs.ContainsKey(3))
                keyValuePairs.Add(3, new List<GenericSkill>());
            if (!keyValuePairs.ContainsKey(4))
                keyValuePairs.Add(4, new List<GenericSkill>());
        }
        public void FixedUpdate()
        {

            if (keyValuePairs[0] != null) foreach (GenericSkill skill in keyValuePairs[0]) skill.ExecuteIfReady();
            if ((!body.inputBank || body.inputBank.skill1.down) && keyValuePairs[1] != null) foreach (GenericSkill skill in keyValuePairs[1]) if (skill) skill.ExecuteIfReady();
            if ((!body.inputBank || body.inputBank.skill2.down) && keyValuePairs[2] != null) foreach (GenericSkill skill in keyValuePairs[2]) if (skill) skill.ExecuteIfReady();
            if ((!body.inputBank || body.inputBank.skill3.down) && keyValuePairs[3] != null) foreach (GenericSkill skill in keyValuePairs[3]) if (skill) skill.ExecuteIfReady();
            if ((!body.inputBank || body.inputBank.skill4.down) && keyValuePairs[4] != null) foreach (GenericSkill skill in keyValuePairs[4]) if (skill) skill.ExecuteIfReady();
        }
    }
    public class BrassKnucklesComponent : CharacterBody.ItemBehavior
    {
        public static float chancePerStack = 10f;
        public List<GenericSkill> genericSkills = new List<GenericSkill>();
        public List<Action<GenericSkill>> onSkillActivatedEvents = new List<Action<GenericSkill>>();
        public SkillLocator skillLocator;
        public void OnEnable()
        {
            skillLocator = GetComponent<SkillLocator>();
            if (skillLocator == null) return;
            foreach (var skill in skillLocator.allSkills)
            {
                if (skill == null) continue;
                CharacterBody body = skill.characterBody;
                if (body == null) continue;
                GenericSkill genericSkill = Utils.CopyGenericSkill(skill, body, "DragonStyle");
                genericSkills.Add(genericSkill);
                body.onSkillActivatedServer += Body_onSkillActivatedServer;
                onSkillActivatedEvents.Add(Body_onSkillActivatedServer);
                void Body_onSkillActivatedServer(GenericSkill obj)
                {
                    if (obj == skill)
                    {
                        //int itemCount = body && body.inventory ? body.inventory.GetItemCount(DuplicateMainSkills) : 0;
                        float chance = stack * chancePerStack;//Utils.ConvertAmplificationPercentageIntoReductionPercentage(itemCount * 10, 100);
                        if (Util.CheckRoll(chance))
                            genericSkill.Invoke("OnExecute", UnityEngine.Random.Range(0.2f, 0.3f));
                    }
                }
            }
        }

        public void OnDisable()
        {
            for (int i = 0; i < onSkillActivatedEvents.Count; i++)
            {
                Action<GenericSkill> skill = onSkillActivatedEvents[i];
                body.onSkillActivatedServer -= skill;
            }
            for (int i = 0; i < genericSkills.Count; i++)
            {
                GenericSkill skill = genericSkills[i];
                if (skill != null)
                {
                    //if (skill.characterBody.skillLocator != null) skill.characterBody.skillLocator.RemoveBonusSkill(skill);
                    Destroy(skill.stateMachine);
                    Destroy(skill);
                }
            }
            genericSkills.Clear();
        }
    }
    public class SummonMercenaryComponent : CharacterBody.ItemBehavior
    {
        public static int maxAmount = 1;
        private void FixedUpdate()
        {
            int itemCount = stack;
            if (itemCount > 0)
            {
                int deployableCount = body.master.GetDeployableCount(Assets.mercenaryGhostDeployable);
                if (deployableCount < maxAmount)
                {
                    Vector3 destination = body.footPosition;
                    Vector3 startPosition = body.footPosition;
                    NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                    NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(startPosition, HullClassification.Human, float.PositiveInfinity);
                    groundNodes.GetNodePosition(nodeIndex, out destination);
                    CharacterMaster characterMaster = new MasterSummon
                    {
                        position = destination,
                        ignoreTeamMemberLimit = true,
                        masterPrefab = Assets.mercMaster,
                        summonerBodyObject = body.gameObject,
                        teamIndexOverride = body.teamComponent ? body.teamComponent.teamIndex : TeamIndex.None,
                        rotation = Quaternion.identity,
                        //inventoryToCopy = inventory,
                        //loadout = loadout,
                    }.Perform();
                    if (characterMaster)
                    {
                        CharacterBody characterBody = characterMaster.GetBody();
                        if (characterBody)
                        {
                            EntityStateMachine entityStateMachine = characterBody.GetComponent<EntityStateMachine>();
                            entityStateMachine?.SetNextStateToMain();
                            Deployable deployable = characterMaster.GetBody().gameObject.AddComponent<Deployable>();
                            body.master.AddDeployable(deployable, Assets.mercenaryGhostDeployable);
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
                }
                
            }

        }
    }
    public class TaoArtifactsComponent : CharacterBody.ItemBehavior
    {
        public static int neededAmount = 7;
        public static float damageCoefficient = 10f;
        public static float timeToRemoveHit = 2.5f;
        public Dictionary<CharacterBody, int> consecutiveHits = new Dictionary<CharacterBody, int>();
        private List<CharacterBody> actualHits = new List<CharacterBody>();
        private float stopwatch;
        private bool CanApply(CharacterBody characterBody)
        {
            if (consecutiveHits.ContainsKey(characterBody)) if (consecutiveHits[characterBody] >= neededAmount) return true; return false;
        }
        private void ApplyHit(CharacterBody characterBody)
        {
            if (consecutiveHits.ContainsKey(characterBody))
            {
                consecutiveHits[characterBody]++;
            }
            else
            {
                consecutiveHits.Add(characterBody, 1);
                actualHits.Add(characterBody);
            }
        }
        private void UpdateHits()
        {
            for (int i = 0; i < actualHits.Count; i++)
            {
                CharacterBody characterBody = actualHits[i];
                if (characterBody)
                {
                    consecutiveHits[characterBody]--;
                }
                else
                {
                    actualHits.Remove(characterBody);
                    consecutiveHits.Remove(characterBody);
                }
            }
        }
        public void RegisterHit(DamageReport obj)
        {
            bool hasFlag = obj.damageInfo.damageType.damageSource.HasFlag(DamageSource.Primary);
            if (!hasFlag) return;
            int itemCount = obj.attackerBody && obj.attackerBody.inventory ? obj.attackerBody.inventory.GetItemCount(WoundEnemyOnContiniousHits) : 0;
            int itemCount2 = obj.attackerBody && obj.attackerBody.inventory ? obj.attackerBody.inventory.GetItemCount(DropHealOrbsOnContiniousHits) : 0;
            CharacterBody victimBody = obj.attackerBody;
            if (itemCount + itemCount2 > 0)
            {
                ApplyHit(victimBody);
                if (CanApply(victimBody))
                {
                    if (itemCount > 0)
                    {
                        DamageInfo damageInfo = new DamageInfo
                        {
                            attacker = obj.attacker,
                            canRejectForce = true,
                            crit = false,
                            damageColorIndex = DamageColorIndex.Item,
                            damageType = DamageTypeCombo.Generic,
                            damage = obj.attackerBody.damage * itemCount * damageCoefficient,
                            inflictor = obj.attacker,
                            position = obj.damageInfo.position,
                            //procChainMask = procChainMask,
                            procCoefficient = 1f,
                        };
                        victimBody.healthComponent.TakeDamageProcess(damageInfo);
                    }
                    if (itemCount2 > 0)
                    {
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/TreebotFruitDeathEffect"), new EffectData
                        {
                            origin = victimBody.corePosition,
                            rotation = UnityEngine.Random.rotation
                        }, true);
                        int num2 = itemCount2;
                        GameObject original = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/TreebotFruitPack");
                        for (int i = 0; i < num2; i++)
                        {
                            GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(original, victimBody.corePosition + UnityEngine.Random.insideUnitSphere * victimBody.radius * 0.5f, UnityEngine.Random.rotation);
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
                }
            }
        }
        private void OnDisable()
        {
            if (this.body)
            {
                if (this.body.HasBuff(TaoPunchReady))
                {
                    this.body.RemoveBuff(TaoPunchReady);
                }
                if (this.body.HasBuff(TaoPunchCooldown))
                {
                    this.body.RemoveBuff(TaoPunchCooldown);
                }
            }
        }
        private void FixedUpdate()
        {
            bool flag = this.body.HasBuff(TaoPunchCooldown);
            bool flag2 = this.body.HasBuff(TaoPunchReady);
            if (!flag && !flag2)
            {
                this.body.AddBuff(TaoPunchReady);
            }
            if (flag2 && flag)
            {
                this.body.RemoveBuff(TaoPunchReady);
            }
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= timeToRemoveHit)
            {
                UpdateHits();
                stopwatch = 0f;
            }
        }
    }
    public class ChalkComponent : CharacterBody.ItemBehavior
    {
        public void FixedUpdate()
        {
            if (!body.inputBank) return;
            if (body.inputBank.interact.down && body.inputBank.jump.justPressed)
            {
                Ray ray = body.inputBank ? new Ray { direction = body.inputBank.aimDirection, origin = body.inputBank.aimOrigin } : new Ray { direction = body.transform.rotation.eulerAngles, origin = body.corePosition };
                Leap(ray);
            }
        }
        public void Leap(Ray ray)
        {

            RaycastHit[] raycastHits = Physics.RaycastAll(ray, 9999f, LayerIndex.noCollision.mask, QueryTriggerInteraction.UseGlobal);
            float distance = 9999f;
            WormholeComponent wormholeComponent2 = null;
            foreach (RaycastHit hit in raycastHits)
            {
                WormholeComponent wormholeComponent = hit.collider.GetComponent<WormholeComponent>();
                if (!wormholeComponent) continue;
                if (distance > hit.distance)
                {
                    wormholeComponent2 = wormholeComponent;
                    distance = hit.distance;
                }
            }
            if (wormholeComponent2)
            {
                wormholeComponent2.Teleport(body);
            }
        }
    }
    public class HealReceivedDamageBehaviour : CharacterBody.ItemBehavior
    {
        public List<HealReceivedDamage> healReceivedDamageBits = [];
        public class HealReceivedDamage
        {
            public HealReceivedDamage(float healAmount, float healTime)
            {
                healRate = healAmount / healTime;
                this.healTime = healTime;
            }
            public float healRate;
            public float healTime;
        }
        public void AddHealReceivedDamageBit(float healAmount, float time)
        {
            HealReceivedDamage healReceivedDamageBit = new(healAmount, time);
            healReceivedDamageBits.Add(healReceivedDamageBit);
        }
        public void FixedUpdate()
        {
            HealthComponent healthComponent = body.healthComponent;
            for (int i = 0; i < healReceivedDamageBits.Count; i++)
            {
                HealReceivedDamage healReceivedDamageBit = healReceivedDamageBits[i];
                if (healthComponent) healthComponent.health += healReceivedDamageBit.healRate * Time.fixedDeltaTime;
                healReceivedDamageBit.healTime -= Time.fixedDeltaTime;
                if (healReceivedDamageBit.healTime <= 0) healReceivedDamageBits.Remove(healReceivedDamageBit);
            }
        }
    }
    public class WormholeComponent : MonoBehaviour
    {
        public float timer = 10f;
        public float stopwatch = 0f;
        public bool isRecharged
        {
            get { return stopwatch >= timer; }
            set { stopwatch = timer; }

        }
        public void FixedUpdate()
        {
            if (stopwatch < timer) stopwatch += Time.fixedDeltaTime;
        }
        public void Teleport(CharacterBody characterBody)
        {
            TeleportHelper.TeleportBody(characterBody, transform.position, false);
        }
    }
    public class EquipmentPicker : MonoBehaviour
    {
        public Transform grid;
        [HideInInspector] public ExtraEquipmentSlotBehaviour extraEquipmentSlotBehaviour;
        [HideInInspector] public bool blockEquipmentUse;
        public void OnDisable()
        {
            blockEquipmentUse = false;
        }
        public void UpdatePicker()
        {
            foreach (Transform slot in grid)
            {
                Destroy(slot.gameObject);
            }
            for (int i = 0; i < extraEquipmentSlotBehaviour.equipments.Length; i++)
            {
                EquipmentPickerSlot equipmentPickerSlot = Instantiate(Assets.EquipmentPickerSlot, grid).GetComponent<EquipmentPickerSlot>();
                EquipmentIndex equipmentIndex = extraEquipmentSlotBehaviour.equipments[i];
                equipmentPickerSlot.UpdatePickerSlot(equipmentIndex);
                equipmentPickerSlot.index = i;
                equipmentPickerSlot.hGButton.onClick.AddListener(SelectEquipment);
                void SelectEquipment()
                {
                    EquipmentIndex newEquipmentIndex = extraEquipmentSlotBehaviour.SelectEquipment(equipmentPickerSlot.index);
                    equipmentPickerSlot.UpdatePickerSlot(newEquipmentIndex);
                    blockEquipmentUse = true;
                }
            }
        }
    }
    public class EquipmentPickerSlot : MonoBehaviour
    {
        [HideInInspector] public int index = -1;
        public HGButton hGButton;
        [HideInInspector] public EquipmentIndex equipmentIndex;
        public void UpdatePickerSlot(EquipmentIndex equipmentIndex)
        {
            this.equipmentIndex = equipmentIndex;
            Sprite sprite = equipmentIndex != EquipmentIndex.None ? EquipmentCatalog.GetEquipmentDef(equipmentIndex).pickupIconSprite : null;
            hGButton.image.sprite = sprite;
        }
    }
    public class EquipmentPickerRadialLayout : LayoutGroup
    {
        public float fDistance;
        [Range(0f, 360f)]
        public float MinAngle, MaxAngle, StartAngle;
        public bool OnlyLayoutVisible = false;
        public override void OnEnable() { base.OnEnable(); CalculateRadial(); }
        public override void SetLayoutHorizontal()
        {
        }
        public override void SetLayoutVertical()
        {
        }
        public override void CalculateLayoutInputVertical()
        {
            CalculateRadial();
        }
        public override void CalculateLayoutInputHorizontal()
        {
            CalculateRadial();
        }
        public override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
        void CalculateRadial()
        {
            m_Tracker.Clear();
            if (transform.childCount == 0)
                return;

            int ChildrenToFormat = 0;
            if (OnlyLayoutVisible)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    RectTransform child = (RectTransform)transform.GetChild(i);
                    if ((child != null) && child.gameObject.activeSelf)
                        ++ChildrenToFormat;
                }
            }
            else
            {
                ChildrenToFormat = transform.childCount;
            }

            float fOffsetAngle = (MaxAngle - MinAngle) / ChildrenToFormat;

            float fAngle = StartAngle;
            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform child = (RectTransform)transform.GetChild(i);
                if ((child != null) && (!OnlyLayoutVisible || child.gameObject.activeSelf))
                {
                    m_Tracker.Add(this, child,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.Pivot);
                    Vector3 vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
                    child.localPosition = vPos * fDistance;
                    child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);
                    fAngle += fOffsetAngle;
                }

            }
        }
    }
    
    [CreateAssetMenu(menuName = "RoR2/CIItemDef")]
    public class CIItemDef : ItemDef
    {
    }
}
