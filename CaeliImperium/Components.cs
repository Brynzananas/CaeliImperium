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

namespace CaeliImperium
{
    public class Components
    {
        public class ContainedPotentialComponent : MonoBehaviour
        {
            public CharacterBody characterBody;
            public CharacterMaster characterMaster;
            public bool canExecuteEquipment = false;
            public bool opened = false;
            public EquipmentIndex[] equipmentDefs = new EquipmentIndex[] { };
            public Inventory inventory;
            public EquipmentSlot equipmentSlot;
            public int itemCount = 0;
            public int previousArraySize = 0;
            public bool useEquipment = true;
            public InputBankTest inputBankTest;

            public void Awake()
            {
                characterMaster = GetComponent<CharacterMaster>();
                characterMaster.onBodyStart += CharacterMaster_onBodyStart;
                GetBodyComponents();
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
                    itemCount = characterBody.inventory.GetItemCount(Items.ContainedPotential);
                }
                equipmentSlot = characterBody ? characterBody.equipmentSlot : null;
                inputBankTest = characterBody ? characterBody.inputBank : null;
            }
            public void FixedUpdate()
            {
                itemCount = inventory.GetItemCount(Items.ContainedPotential);
                if (inputBankTest)
                {
                    if (inputBankTest.ping.down)
                    {
                        if (inputBankTest.skill1.justPressed) MoveEquipmentUpwards();
                        if (inputBankTest.skill2.justPressed) MoveEquipmentDownwards();
                    }

                }
                if (itemCount != equipmentDefs.Length)
                {
                    if (equipmentDefs.Length < itemCount)
                    {
                        int toChange = equipmentDefs.Length;
                        Array.Resize(ref equipmentDefs, itemCount);
                        for (int i = 0; i < (itemCount - toChange); i++)
                        {
                            equipmentDefs.SetValue(EquipmentIndex.None, equipmentDefs.Length - (i + 1));

                        }
                    }
                    else
                    {
                        for (int i = 0; i < (equipmentDefs.Length - itemCount); i++)
                        {
                            EquipmentIndex pickupIndex = (EquipmentIndex)equipmentDefs.GetValue(equipmentDefs.Length - (i + 1));
                            if (pickupIndex != EquipmentIndex.None)
                            {
                                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(pickupIndex), characterBody.corePosition, Physics.gravity * -1f);

                            }

                        }
                        Array.Resize(ref equipmentDefs, itemCount);
                    }

                }
                previousArraySize = equipmentDefs.Length;
            }
            private void Inventory_onInventoryChanged()
            {
                itemCount = characterBody ? characterBody.inventory.GetItemCount(Items.ContainedPotential) : 0;
            }
            public void MoveEquipmentUpwards()
            {
                EquipmentIndex equipmentDef = equipmentDefs[0];
                Array.Copy(equipmentDefs, 1, equipmentDefs, 0, equipmentDefs.Length - 1);
                EquipmentIndex equipmentIndex = inventory.GetEquipmentIndex();
                equipmentDefs[equipmentDefs.Length - 1] = equipmentIndex;
                inventory.SetEquipmentIndex(equipmentDef);
                useEquipment = false;
            }
            public void MoveEquipmentDownwards()
            {
                Array.Reverse(equipmentDefs);
                EquipmentIndex equipmentDef = equipmentDefs[0];
                Array.Copy(equipmentDefs, 1, equipmentDefs, 0, equipmentDefs.Length - 1);
                EquipmentIndex equipmentIndex = inventory.GetEquipmentIndex();
                equipmentDefs[equipmentDefs.Length - 1] = equipmentIndex;
                Array.Reverse(equipmentDefs);
                inventory.SetEquipmentIndex(equipmentDef);
                useEquipment = false;
            }
            public void OpenWheel()
            {
                opened = !opened;
                if (useEquipment)
                    equipmentSlot.ExecuteIfReady();
            }
        }
        public class SkullMachineGunComponent : CharacterBody.ItemBehavior
        {
            public float stopwatch = 0f;
            public float fireRate = 0.2f;
            public bool firing;
            public void FixedUpdate()
            {

                if (body == null) return;
                if (stopwatch > 0f) stopwatch -= Time.fixedDeltaTime * body.attackSpeed;
                if (body.inputBank == null) return;
                if (body.inputBank.skill1.down)
                {
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
                };
            }
        }
        public class CopperBellComponent : CharacterBody.ItemBehavior
        {
            public float cooldown = 15f;
            public float stopwatch = 15f;
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
            public float stopwatch = 0f;
            public float charge = 0f;
            public bool fire = false;
            public void FixedUpdate()
            {
                if (body.HasBuff(ConvertDamageToAtomicCharge))
                {
                    int buffCount = body.GetBuffCount(ConvertDamageToAtomicCharge);
                    charge += buffCount * 20 * charge;
                    if (NetworkServer.active)
                    {
                        body.SetBuffCount(ConvertDamageToAtomicCharge.buffIndex, 0);
                    }
                }
                charge += Time.fixedDeltaTime * stack;
                if (body.inputBank && body.inputBank.skill4.justPressed)
                {
                    stopwatch = 1f;
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
                        damage = body.damage * Utils.ConvertAmplificationPercentageIntoReductionPercentage(charge, 100f),
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
                        if (hitInfo.hurtBox != null)
                        {
                            InflictDotInfo dotInfo = new InflictDotInfo()
                            {
                                attackerObject = gameObject,
                                victimObject = hitInfo.hurtBox.healthComponent.gameObject,
                                totalDamage = body.damage,
                                damageMultiplier = stack,
                                duration = 5f,
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
                if ((!body.inputBank || body.inputBank.skill1.down) && keyValuePairs[1] != null) foreach (GenericSkill skill in keyValuePairs[1]) if(skill) skill.ExecuteIfReady();
                if ((!body.inputBank || body.inputBank.skill2.down) && keyValuePairs[2] != null) foreach (GenericSkill skill in keyValuePairs[2]) if (skill) skill.ExecuteIfReady();
                if ((!body.inputBank || body.inputBank.skill3.down) && keyValuePairs[3] != null) foreach (GenericSkill skill in keyValuePairs[3]) if (skill) skill.ExecuteIfReady();
                if ((!body.inputBank || body.inputBank.skill4.down) && keyValuePairs[4] != null) foreach (GenericSkill skill in keyValuePairs[4]) if (skill) skill.ExecuteIfReady();
            }
        }
        public class BrassKnucklesComponent : CharacterBody.ItemBehavior
        {
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
                            int itemCount = body && body.inventory ? body.inventory.GetItemCount(BrassKnuckles) : 0;
                            float chance = Utils.ConvertAmplificationPercentageIntoReductionPercentage(itemCount * 10, 100);
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
        public class TaoArtifactsComponent : CharacterBody.ItemBehavior
        {
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
            }
        }
        public class ChalkComponent : CharacterBody.ItemBehavior
        {
            public void FixedUpdate()
            {
                if (!body.inputBank) return;
                if (body.inputBank.interact.down && body.inputBank.jump.justPressed)
                {
                    Ray ray = body.inputBank ? new Ray{ direction = body.inputBank.aimDirection, origin = body.inputBank.aimOrigin} : new Ray { direction = body.transform.rotation.eulerAngles, origin = body.corePosition };
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
    }
}
