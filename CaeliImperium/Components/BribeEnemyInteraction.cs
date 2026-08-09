using BrynzaAPI;
using CaeliImperium.Items;
using JetBrains.Annotations;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components
{
    public class BribeEnemyInteraction : NetworkBehaviour, IInteractable
    {
        public static uint moneyMultiplier = 5U;
        public CharacterBody characterBody;
        public DeathRewards deathRewards;
        public TeamComponent teamComponent;
        public BribeColliderComponent bribeCollider;
        public Highlight highlight;
        [SyncVar] public uint cost;
        private uint previousCost;
        public uint GetCost() => cost;
        public void Awake()
        {
            if (!characterBody) characterBody = GetComponent<CharacterBody>();
            if (!deathRewards) deathRewards = GetComponent<DeathRewards>();
            if (!teamComponent) teamComponent = GetComponent<TeamComponent>();
            if (bribeCollider) return;
            bribeCollider = Instantiate(BribeEnemiesAndBuffMinionsEvents.BribeCollider, transform).GetComponent<BribeColliderComponent>();
            bribeCollider.characterBody = characterBody;
            bribeCollider.entityLocator.entity = gameObject;
        }
        public string GetContextString([NotNull] Interactor activator)
        {
            return "Bribe " + RoR2.Language.GetString(characterBody.baseNameToken) + " for " + GetCost() + " gold?";
        }
        public Interactability currentInteractibility;
        public Interactability GetInteractability([NotNull] Interactor activator)
        {
            if (!teamComponent) return HandleInteractibility(Interactability.Disabled);
            uint cost = GetCost();
            if (cost == 0U) return HandleInteractibility(Interactability.Disabled);
            CharacterBody characterBody = activator.GetCharacterBody();
            if (!characterBody) return HandleInteractibility(Interactability.Disabled);
            TeamComponent activatorTeamComponent = characterBody.teamComponent;
            if (!activatorTeamComponent || activatorTeamComponent.teamIndex == teamComponent.teamIndex) return HandleInteractibility(Interactability.Disabled);
            CharacterMaster characterMaster = characterBody.master;
            if (!characterMaster) return HandleInteractibility(Interactability.Disabled);
            Inventory inventory = characterBody.inventory;
            if (!inventory) return HandleInteractibility(Interactability.Disabled);
            int itemCount = inventory.GetItemCountEffective(CaeliImperiumContent.Items.BribeEnemiesAndBuffMinions);
            if (itemCount <= 0) return HandleInteractibility(Interactability.Disabled);
            if (characterMaster.money < cost) return HandleInteractibility(Interactability.ConditionsNotMet);
            return HandleInteractibility(Interactability.Available);
        }
        private bool dontHide;
        public void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                if (deathRewards)
                {
                    if (previousCost != deathRewards.goldReward)
                    {
                        cost = deathRewards.goldReward;
                        previousCost = cost;
                    }
                }
                else
                {
                    if (previousCost != cost)
                    {
                        cost = 0U;
                        previousCost = cost;
                    }
                }
            }
            
            if (dontHide)
            {
                dontHide = false;
            }
            else
            {
                currentInteractibility = Interactability.Disabled;
                if (!highlight) return;
                highlight.isOn = false;
                highlight.enabled = false;
            }
        }
        private Interactability HandleInteractibility(Interactability interactability)
        {
            dontHide = true;
            if (currentInteractibility == interactability) return interactability;
            currentInteractibility = interactability;
            if (highlight == null) return interactability;
            switch (interactability)
            {
                case Interactability.Disabled:
                    highlight.isOn = false;
                    highlight.enabled = false;
                    break;
                case Interactability.ConditionsNotMet:
                    highlight.enabled = true;
                    highlight.isOn = true;
                    highlight.highlightColor = Highlight.HighlightColor.unavailable;
                    break;
                case Interactability.Available:
                    highlight.enabled = true;
                    highlight.isOn = true;
                    highlight.highlightColor = Highlight.HighlightColor.interactive;
                    break;
                default:
                    break;
            }
            return interactability;
        }
        public void OnInteractionBegin([NotNull] Interactor activator)
        {
            if (!characterBody || !teamComponent) return;
            TeamComponent teamComponent2 = activator.GetComponent<TeamComponent>();
            if (!teamComponent2) return;
            teamComponent.teamIndex = teamComponent2.teamIndex;
            CharacterBody activatorBody = teamComponent2.body;
            if (!activatorBody) return;
            CharacterMaster activatorMaster = activatorBody.master;
            if (!activatorMaster) return;
            activatorMaster.money = (uint)Mathf.Max(0f, (float)activatorMaster.money - (float)cost);
            CharacterMaster characterMaster = characterBody.master;
            if (characterMaster)
            {
                characterMaster.money += cost;
                characterMaster.teamIndex = teamComponent2.teamIndex;
                BaseAI[] baseAIs = characterMaster.AiComponents;
                if (baseAIs != null)
                    foreach (BaseAI ai in baseAIs)
                    {
                        ai.currentEnemy.gameObject = null;
                        ai.bufferedTarget = null;
                        ai.customTarget.gameObject = null;
                        ai.neverRetaliateFriendlies = true;
                    }
                OnDestroyCallback[] onDestroyCallbacks = characterMaster.gameObject.GetComponents<OnDestroyCallback>();
                foreach (OnDestroyCallback onDestroyCallback in onDestroyCallbacks)
                {
                    if (!onDestroyCallback || onDestroyCallback.callback == null) continue;
                    if (!BribeEnemiesAndBuffMinionsEvents.onDestroyCallbackTypeFilter.Contains(onDestroyCallback.callback.Target.GetType())) continue;
                    if (!BribeEnemiesAndBuffMinionsEvents.onDestroyCallbackMethodFilter.Contains(onDestroyCallback.callback.Method.Name)) continue;
                    onDestroyCallback.callback.Invoke(onDestroyCallback);
                }
                MinionOwnership minionOwnership = characterMaster.minionOwnership;
                if (minionOwnership) MinionOwnership.MinionGroup.SetMinionOwner(minionOwnership, activatorMaster.netId);
            }
            if (BribeEnemiesAndBuffMinionsEvents.onBribe == null) return;
            BribeReport bribeReport = new BribeReport();
            bribeReport.activator = activator;
            bribeReport.activatorTeamComponent = teamComponent2;
            bribeReport.bribeEnemyInteraction = this;
            BribeEnemiesAndBuffMinionsEvents.onBribe?.Invoke(bribeReport);
        }
        public bool ShouldIgnoreSpherecastForInteractibility([NotNull] Interactor activator) => true;

        public bool ShouldProximityHighlight() => false;

        public bool ShouldShowOnScanner() => false;
    }
}
