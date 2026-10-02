using CaeliImperium;
using EntityStates;
using HG;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace CaeliImperiumEntityStates.Bishop;
public class SpearOrbitThrow : BishopState
{
    public static float baseDamageCoefficient = 0f;
    public static float procCoefficient = 0f;
    public static float minSpread = 0f;
    public static float maxSpread = 0f;
    public static float baseDuration = 1f;
    public static bool allowTrajectoryAimAssist = true;
    public static uint bulletCount = 1;
    public static DamageType damageType = DamageType.IgniteOnHit;
    public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
    public static BulletAttack.FalloffModel falloffModel = BulletAttack.FalloffModel.None;
    public static float force = 0f;
    public static float radius = 1f;
    public static float trajectoryAimAssistMultiplier = 0.75f;
    public static bool smartCollision = true;
    public static float maxDistance = 128f;
    public static PhysForceFlags physForceFlags = PhysForceFlags.None;
    public float damage;
    public float duration;
    public bool found;
    public override void OnEnter()
    {
        base.OnEnter();
        SetValues();
        FireHook();
    }
    public void SetValues()
    {
        damage = baseDamageCoefficient * characterBody.damage;
        duration = baseDuration;
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!isAuthority || fixedAge > duration) return;
        outer.SetNextStateToMain();
    }
    public void FireHook()
    {
        (this as IBishopState).PlayFirstPersonCrossfade("LeftArm, Override", "SpearThrow", "leftArm.playbackRate", attackSpeedStat, 0.05f, true);
        Util.PlaySound("Play_DoomTDA_Spear_ThrowOrbit", gameObject);
        if (!isAuthority) return;
        Ray ray = GetAimRay();
        BulletAttack bulletAttack = new BulletAttack
        {
            owner = gameObject,
            weapon = gameObject,
            origin = ray.origin,
            aimVector = ray.direction,
            minSpread = minSpread,
            maxSpread = maxSpread,
            damage = damage,
            allowTrajectoryAimAssist = allowTrajectoryAimAssist,
            bulletCount = bulletCount,
            damageType = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource(DamageSource.Primary)),
            falloffModel = falloffModel,
            force = force,
            tracerEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab,
            isCrit = RollCrit(),
            trajectoryAimAssistMultiplier = trajectoryAimAssistMultiplier,
            radius = radius,
            hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
            smartCollision = smartCollision,
            maxDistance = maxDistance,
            physForceFlags = physForceFlags,
            procCoefficient = procCoefficient,
            hitCallback = HitCallback,
            filterCallback = FilterCallback,
        };
        bulletAttack.Fire();
    }
    public bool FilterCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
    {
        CaeliImperiumPlugin.Log.LogMessage("filter hit info hit hurtbox: " + hitInfo.hitHurtBox);
        if (teamComponent && hitInfo.hitHurtBox)
        {
            HealthComponent healthComponent = hitInfo.hitHurtBox.healthComponent;
            CaeliImperiumPlugin.Log.LogMessage("filter hit info health component: " + healthComponent);
            if (healthComponent)
            {
                CharacterBody characterBody = healthComponent.body;
                if (characterBody && characterBody.teamComponent)
                {
                    bool enemyTeam = TeamManager.IsTeamEnemy(characterBody.teamComponent.teamIndex, GetTeam());
                    CaeliImperiumPlugin.Log.LogMessage("filter hit info team check: " + enemyTeam);
                    if (!enemyTeam) return false;
                }
            }
        }
        CaeliImperiumPlugin.Log.LogMessage("default filter");
        return BulletAttack.DefaultFilterCallbackImplementation(bulletAttack, ref hitInfo);
    }
    public bool HitCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
    {
        CaeliImperiumPlugin.Log.LogMessage("hit info hit hurtbox: " + hitInfo.hitHurtBox);
        if (teamComponent && hitInfo.hitHurtBox)
        {
            HealthComponent healthComponent = hitInfo.hitHurtBox.healthComponent;
            CaeliImperiumPlugin.Log.LogMessage("filter hit info health component: " + healthComponent);
            if (healthComponent)
            {
                CharacterBody characterBody = healthComponent.body;
                if (characterBody && characterBody.teamComponent)
                {
                    bool enemyTeam = TeamManager.IsTeamEnemy(characterBody.teamComponent.teamIndex, GetTeam());
                    CaeliImperiumPlugin.Log.LogMessage("filter hit info team check: " + enemyTeam);
                    if (enemyTeam)
                    {
                        found = true;
                        outer.SetNextStateToMain();
                        EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");
                        if (entityStateMachine) entityStateMachine.SetNextState(new SpearOrbit { hitBodyNetId = characterBody.netId});
                    CaeliImperiumPlugin.Log.LogMessage(1);
                    }
                    else
                    {
                    CaeliImperiumPlugin.Log.LogMessage(2);
                        return false;
                    }
                }
            }
        }
        CaeliImperiumPlugin.Log.LogMessage("default hit");
        bool hit = BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
        return hit;
    }
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
}
