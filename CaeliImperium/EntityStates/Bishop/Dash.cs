using CaeliImperium;
using CaeliImperium.Bodies;
using CaeliImperium.Components.Bishop;
using CaeliImperium.NetworkMessages;
using CaeliImperium.ScriptableObjects;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates.Bishop;
public class Dash : BishopMainState
{
    public static float baseSpeed = 64f;
    public static float baseDashDuration = 0.2f;
    public static float baseDuration = 0.6f;
    public static float dashRadius = 5f;
    public Vector3 direction;
    public float speed;
    public float dashDuration;
    public float duration;
    public bool sprintInputBuffer;
    public override void OnEnter()
    {
        base.OnEnter();
        Util.PlaySound("Play_DoomTDA_Player_Dash", gameObject);
        GatherInputs();
        DestroyNearbyProjectilesAndGainSpecialSpearStocks();
        SetValues();
    }
    public override void OnExit()
    {
        base.OnExit();
        if (!isAuthority) return;
        if (sprintInputBuffer && inputBank) inputBank.sprint.down = true;
    }
    public void DestroyNearbyProjectilesAndGainSpecialSpearStocks()
    {
        if (!isAuthority) return;
        Collider[] colliders = Physics.OverlapSphere(transform.position, dashRadius, LayerIndex.projectile.mask | LayerIndex.CommonMasks.characterBodies, QueryTriggerInteraction.UseGlobal);
        int parriableCount = 0;
        foreach (Collider collider in colliders)
        {
            Transform root = collider.transform.root;
            ProjectileController projectileController = root.GetComponent<ProjectileController>();
            if (projectileController)
            {
                ProjectileDamage projectileDamage = projectileController.GetComponent<ProjectileDamage>();
                if (!projectileDamage) continue;
                bool isParriable = projectileDamage.damageColorIndex == BishopEvents.ParriableDamageColor;
                bool canDestroy = projectileController.cannotBeDeleted ? isParriable : true;
                if (!canDestroy) continue;
                if (isParriable) parriableCount++;
                if (NetworkServer.active)
                {
                    Destroy(projectileDamage.gameObject);
                }
                else
                {
                    DestroyNetworkObjectNetMessage.SendToServer(projectileController.netId);
                }
            }
            else
            {
                CharacterBody characterBody = root.GetComponent<CharacterBody>();
                if (!characterBody || !characterBody.HasBuff(BishopEvents.PrepareParriableAttackCount)) continue;
                parriableCount++;
            }
        }
        BishopSpecialSpearSkillDef.RechargeStocksForSpecialSpearSkills(characterBody, parriableCount);
        BishopRechargeSpecialSpearSkillsNetMessage.SendToClients(parriableCount, characterBody.netId);
    }
    public void SetValues()
    {
        direction = moveVector;
        speed = baseSpeed;
        dashDuration = baseDashDuration;
        BishopComponent bishopComponent = (this as IBishopState).bishopComponent;
        duration = baseDuration / (bishopComponent ? bishopComponent.previousMoveSpeed / characterBody.moveSpeed : 1f);
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isAuthority) return;
        if (fixedAge < dashDuration)
        {
            this.SetVelocity(Vector3.zero);
            this.AddRootMotion(direction * speed * Time.fixedDeltaTime);
        }
        if (!sprintInputBuffer && inputBank && inputBank.sprint.justPressed) sprintInputBuffer = true;
        if (fixedAge < duration) return;
        outer.SetNextStateToMain();
    }
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
}
