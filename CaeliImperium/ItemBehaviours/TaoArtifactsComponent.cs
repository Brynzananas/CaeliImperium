using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static CaeliImperium.CaeliImperiumContent.Buffs;
using static CaeliImperium.CaeliImperiumContent.Items;

namespace CaeliImperium.ItemBehaviours
{
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
            int itemCount = obj.attackerBody && obj.attackerBody.inventory ? obj.attackerBody.inventory.GetItemCountEffective(WoundEnemyOnContiniousHits) : 0;
            int itemCount2 = obj.attackerBody && obj.attackerBody.inventory ? obj.attackerBody.inventory.GetItemCountEffective(DropHealOrbsOnContiniousHits) : 0;
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
}
