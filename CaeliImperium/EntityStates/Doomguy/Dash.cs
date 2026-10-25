using CaeliImperium;
using CaeliImperium.Bodies;
using CaeliImperium.Components.Doomguy;
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

namespace CaeliImperiumEntityStates.Doomguy;
public class Dash : DoomguyMainState
{
    public static float baseSpeed = 48f;
    public static float baseDashDuration = 0.2f;
    public static float baseDuration = 0.6f;
    public static float dashRadius = 5f;
    public Vector3 direction;
    public float speed;
    public float dashDuration;
    public float duration;
    public bool sprintInputBuffer;
    public bool stoppedDashing;
    public override void OnEnter()
    {
        base.OnEnter();
        Util.PlaySound("Play_DoomTDA_Player_Dash", gameObject);
        GatherInputs();
        DestroyNearbyProjectilesAndGainSpecialSpearStocks();
        SetValues();
        if (NetworkServer.active) characterBody.AddTimedBuff(DoomguyEvents.Dodge, duration);
    }
    public override void OnExit()
    {
        base.OnExit();
        if (!isAuthority) return;
        if (sprintInputBuffer && inputBank) inputBank.rawSprintInput.down = true;
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
                bool isParriable = projectileDamage.damageColorIndex == DoomguyEvents.ParriableDamageColor;
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
                if (!characterBody || !characterBody.HasBuff(DoomguyEvents.PrepareParriableAttackCount)) continue;
                parriableCount++;
            }
        }
        DoomguySpecialSpearSkillDef.RechargeStocksForSpecialSpearSkills(characterBody, parriableCount);
        DoomguyRechargeSpecialSpearSkillsNetMessage.SendToClients(parriableCount, characterBody.netId);
    }
    public void SetValues()
    {
        direction = moveVector;
        speed = baseSpeed;
        dashDuration = baseDashDuration;
        DoomguyComponent bishopComponent = (this as IDoomguyState).bishopComponent;
        duration = baseDuration / (bishopComponent ? bishopComponent.moveSpeedCoof : 1f);
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
        else if (!stoppedDashing)
        {
            this.SetVelocity(direction * (characterMotor ? characterMotor.walkSpeed : characterBody.moveSpeed));
            stoppedDashing = true;
        }
        if (!sprintInputBuffer && inputBank && inputBank.rawSprintInput.justPressed) sprintInputBuffer = true;
        if (fixedAge < duration) return;
        outer.SetNextStateToMain();
    }
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
}
