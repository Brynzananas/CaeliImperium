using RoR2;
using RoR2.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ItemBehaviours
{
    public class SummonMercenaryBehaviour : CharacterBody.ItemBehavior
    {
        public static int maxAmount = 1;
        public void FixedUpdate()
        {
            int itemCount = stack;
            if (itemCount > 0)
            {
                int deployableCount = body.master.GetDeployableCount(CaeliImperiumAssets.mercenaryGhostDeployable);
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
                        masterPrefab = CaeliImperiumAssets.mercMaster,
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
                            body.master.AddDeployable(deployable, CaeliImperiumAssets.mercenaryGhostDeployable);
                        }
                        if (characterMaster.inventory)
                        {
                            characterMaster.inventory.GiveItemPermanent(RoR2Content.Items.HealthDecay, 30);
                            characterMaster.inventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, itemCount * 10);
                            characterMaster.inventory.GiveItemPermanent(RoR2Content.Items.BoostAttackSpeed, itemCount * 5);
                            characterMaster.inventory.GiveItemPermanent(RoR2Content.Items.Ghost);
                            characterMaster.inventory.GiveItemPermanent(CaeliImperiumContent.Items.TransferDamageOwnership);
                        }

                    }
                }

            }

        }
    }
}
