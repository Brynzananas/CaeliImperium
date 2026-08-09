using BrynzaAPI;
using CaeliImperium.Components;
using CaeliImperium.Configs;
using CaeliImperium.ItemBehaviours;
using R2API;
using R2API.Networking;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static BrynzaAPI.GiveItemsDelegateDef;
using static CaeliImperium.CaeliImperiumHooks;
using static R2API.BuffsAPI;
using static R2API.RecalculateStatsAPI;

namespace CaeliImperium.Items
{
    public static class DrawSpeedPathEvents
    {
        public static float SpeedPathSpeedBonusCoefficient => DrawSpeedPathConfigs.SpeedPathSpeedBonusCoefficient.Value;
        public static float SpeedPathSpeedBonusStackCoefficient => DrawSpeedPathConfigs.SpeedPathSpeedBonusStackCoefficient.Value;
        public static float SpeedPathMinDistanceBetweenPoints = 1f;
        public static float SpeedPathClusterRadius = 20f;
        public static float SpeedPathMaxLength => DrawSpeedPathConfigs.SpeedPathMaxPathLength.Value;
        public static float SpeedPathMaxLengthStack => DrawSpeedPathConfigs.SpeedPathMaxPathLengthStack.Value;
        public static float SpeedPathSearchRadius = 6f;
        public static float SpeedPathSearchRadiusExcludeFromEnd = 9f;
        public static float SpeedPathRenderDistance => DrawSpeedPathConfigs.SpeedPathRenderDistance.Value;
        public static float SpeedPathFadeDistance = 3f;
        public static GameObject SpeedPathLine;
        public static EffectDef ChalkFootstep;
        public static float gradientMaxAlpha = 2f;
        public static float gradientExtraRange = 32f;
        public static float gradientCoefficient = 32f;
        public static bool inited { private set; get; }
        public static void Init(ItemDef itemDef)
        {
            CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChangedGlobal;
            GetStatCoefficients += Events_GetStatCoefficients;
            OnBuffFirstStackGained += Events_OnBuffFirstStackGained;
            OnBuffFinalStackLost += Events_OnBuffFinalStackLost;
            CaeliImperiumExpansionRunComponent.onFixedUpdate += FixedUpdate;
            //BrynzaAPI.BrynzaAPI.onFootstep += OnFootstep;
            R2API.FootstepAPI.OnFootstep += FootstepAPI_OnFootstep;
            CharacterBodyAPI.AddAlwaysSprintCondition(AlwaysSprint);
            CaeliImperiumPlugin.onPluginDestroyed += OnPluginDestroyed;
            if (inited) return;
            inited = true;
            CaeliImperiumContent.Buffs.SpeedPathSpeedBonus = CaeliImperiumAssets.assetBundle.LoadAsset<BuffDef>("Assets/CaeliImperium/Buffs/SpeedPathSpeedBonus.asset").RegisterBuffDef();
            ChalkFootstep = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Effects/ChalkFootstep.prefab").RegisterEffect();
            SpeedPathLine = CaeliImperiumAssets.assetBundle.LoadAsset<GameObject>("Assets/CaeliImperium/Prefabs/SpeedPathLine.prefab");
        }

        private static bool AlwaysSprint(CharacterBody characterBody) => characterBody.HasBuff(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus);

        public static void OnPluginDestroyed()
        {
            CaeliImperiumPlugin.onPluginDestroyed -= OnPluginDestroyed;
            CharacterBody.onBodyInventoryChangedGlobal -= CharacterBody_onBodyInventoryChangedGlobal;
            GetStatCoefficients -= Events_GetStatCoefficients;
            OnBuffFirstStackGained -= Events_OnBuffFirstStackGained;
            OnBuffFinalStackLost -= Events_OnBuffFinalStackLost;
            CaeliImperiumExpansionRunComponent.onFixedUpdate -= FixedUpdate;
            //BrynzaAPI.BrynzaAPI.onFootstep -= OnFootstep;
            R2API.FootstepAPI.OnFootstep -= FootstepAPI_OnFootstep;
        }
        private static void CharacterBody_onBodyInventoryChangedGlobal(CharacterBody obj)
        {
            obj.AddItemBehavior<DrawSpeedPath2Behaviour>(obj.inventory.GetItemCountEffective(CaeliImperiumContent.Items.DrawSpeedPath));
        }
        private static void FootstepAPI_OnFootstep(R2API.FootstepAPI.FootstepReport footstepReport)
        {
            if (!footstepReport.footstepHandler.body || !footstepReport.footstepHandler.body.HasBuff(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus)) return;
            EffectData effectData = new EffectData();
            effectData.origin = footstepReport.raycastHit.point;
            effectData.rotation = Util.QuaternionSafeLookRotation(footstepReport.raycastHit.normal);
            effectData.SetChildLocatorTransformReference(footstepReport.footstepHandler.body.gameObject, footstepReport.childIndex);
            EffectManager.SpawnEffect(ChalkFootstep.index, effectData, false);
        }
        public class SpeedPathGradient
        {
            public Collider nearestSpeedPath;
            public float distance= float.MaxValue;
            public Transform globalSpeedPath;
        }
        public static List<SpeedPathGradient> FindNearestSpeedPath(Vector3 position)
        {
            Collider[] colliders;
            int num = HGPhysics.OverlapSphere(out colliders, position, gradientExtraRange, LayerIndex.pickups.mask, QueryTriggerInteraction.Collide);
            if (colliders == null || colliders.Length == 0)
            {
                HGPhysics.ReturnResults(colliders);
                return null;
            }
            List<SpeedPathGradient> speedPathGradients = [];
            Dictionary<Transform, SpeedPathGradient> keyValuePairs = [];
            //float nearDistance = float.MaxValue;
            for (int i = 0; i < num; i++)
            {
                Collider collider2 = colliders[i];
                if (!collider2.name.StartsWith("SpeedPath")) continue;
                Transform parent = collider2.transform.parent;
                if (!parent) continue;
                SpeedPathGradient speedPathGradient;
                if (keyValuePairs.TryGetValue(parent, out speedPathGradient))
                {

                }
                else
                {
                    speedPathGradient = new SpeedPathGradient
                    {
                        nearestSpeedPath = collider2,
                        globalSpeedPath = parent,
                        distance = float.MaxValue,
                    };
                    keyValuePairs.Add(parent, speedPathGradient);
                    speedPathGradients.Add(speedPathGradient);
                }
                Vector3 vector3 = collider2.transform.position - position;
                float sqrMagn = vector3.sqrMagnitude;
                if (sqrMagn < speedPathGradient.distance)
                {
                    speedPathGradient.nearestSpeedPath = collider2;
                    speedPathGradient.distance = sqrMagn;
                }
            }
            HGPhysics.ReturnResults(colliders);
            return speedPathGradients;
        }
        public static void HandleFunctionality2()
        {
            if (!Run.instance) return;
            ReadOnlyCollection<DrawSpeedPath2Behaviour> drawSpeedPath2Behaviours = DrawSpeedPath2Behaviour.readOnlyInstances;
            if (drawSpeedPath2Behaviours == null) return;
            ReadOnlyCollection<PlayerCharacterMasterController> playerCharacterMasterControllers = PlayerCharacterMasterController.instances;
            if (playerCharacterMasterControllers == null || playerCharacterMasterControllers.Count <= 0) return;
            PlayerCharacterMasterController playerCharacterMasterController = playerCharacterMasterControllers[0];
            if (!playerCharacterMasterController) return;
            CharacterBody characterBody = playerCharacterMasterController.body;
            if (!characterBody) return;
            int buffCount = 0;
            foreach (DrawSpeedPath2Behaviour drawSpeedPath in drawSpeedPath2Behaviours)
            {
                if (!drawSpeedPath.TeamCheck(characterBody)) continue;
                drawSpeedPath.UpdateLineGradient(characterBody.transform.position);
                if (!drawSpeedPath.IsNearPathExcludingEnd(characterBody.transform.position, SpeedPathSearchRadius, 0f, SpeedPathSearchRadiusExcludeFromEnd)) continue;
                if (buffCount < drawSpeedPath.stack) buffCount = drawSpeedPath.stack;
            }
            if (buffCount == 0 && characterBody.HasBuff(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus))
            {
                characterBody.ApplyBuff(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex, 0);
            }
            else if (buffCount != characterBody.GetBuffCount(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus))
            {
                characterBody.ApplyBuff(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex, buffCount);
            }
        }
        public static void HandleFunctionality()
        {
            if (!Run.instance || !NetworkServer.active) return;
            ReadOnlyCollection<PlayerCharacterMasterController> playerCharacterMasterControllers = PlayerCharacterMasterController.instances;
            if (playerCharacterMasterControllers == null) return;
            ReadOnlyCollection<DrawSpeedPath2Behaviour> drawSpeedPath2Behaviours = DrawSpeedPath2Behaviour.readOnlyInstances;
            if (drawSpeedPath2Behaviours == null) return;
            foreach (PlayerCharacterMasterController playerCharacterMasterController in playerCharacterMasterControllers)
            {
                if (!playerCharacterMasterController) continue;
                CharacterBody characterBody = playerCharacterMasterController.body;
                if (!characterBody) continue;
                int buffCount = 0;
                foreach (DrawSpeedPath2Behaviour drawSpeedPath in drawSpeedPath2Behaviours)
                {
                    if (!drawSpeedPath.TeamCheck(characterBody) || !drawSpeedPath.IsNearPathExcludingEnd(characterBody.transform.position, SpeedPathSearchRadius, 0f, SpeedPathSearchRadiusExcludeFromEnd)) continue;
                    buffCount = drawSpeedPath.stack;
                    break;
                }
                if (buffCount == 0 && characterBody.HasBuff(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus))
                {
                    characterBody.SetBuffCount(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex, 0);
                }
                else if (buffCount != characterBody.GetBuffCount(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus))
                {
                    characterBody.SetBuffCount(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex, buffCount);
                }
            }
        }
        public static void FixedUpdate(CaeliImperiumExpansionRunComponent component)
        {
            HandleFunctionality2();
            /*CharacterBody body = Utils.GetPlayerBody();
            List<SpeedPathGradient> speedPathGradients = body ? FindNearestSpeedPath(body.transform.position) : null;
            if (speedPathGradients != null)
            {
                foreach (SpeedPathGradient speedPathGradient in speedPathGradients)
                {
                    GlobalSpeedPath globalSpeedPath = GlobalSpeedPath.instances[speedPathGradient.globalSpeedPath.transform.GetSiblingIndex()];
                    if (globalSpeedPath) globalSpeedPath.SetGradientValues(speedPathGradient, body.transform.position);
                }
            }*/
        }
        public static void Events_GetStatCoefficients(CharacterBody sender, StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(CaeliImperiumContent.Buffs.SpeedPathSpeedBonus);
            args.moveSpeedMultAdd += buffCount.Stack(SpeedPathSpeedBonusCoefficient, SpeedPathSpeedBonusStackCoefficient);
        }
        public static void Events_OnBuffFinalStackLost(CharacterBody arg1, BuffDef arg2)
        {
            if (arg2.buffIndex == CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex) arg1.ModifyCharacterGravityParams(-1);
        }
        public static void Events_OnBuffFirstStackGained(CharacterBody arg1, BuffDef arg2)
        {
            if (arg2.buffIndex == CaeliImperiumContent.Buffs.SpeedPathSpeedBonus.buffIndex) arg1.ModifyCharacterGravityParams(1);
        }
    }
}
