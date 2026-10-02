using BrynzaAPI;
using CaeliImperium;
using CaeliImperium.Bodies;
using CaeliImperium.NetworkMessages;
using EntityStates;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using static RoR2.Skills.ComboSkillDef;

namespace CaeliImperiumEntityStates.Bishop;
public class Punch : BishopMeleeState
{
    public static float _damageCoefficient = 1f;
    public static float _damageCoefficientPerCombo = 1f;
    public static float _maxDamageCoefficient = 7f;
    public override float damageCoefficient => Mathf.Min((_damageCoefficient + (characterBody.GetBuffCount(BishopEvents.MeleeCombo) + (hasRequiredStockAndDelay ? 1 : 0)) * _damageCoefficientPerCombo), _maxDamageCoefficient);
    public static float _procCoefficient = 1f;
    public override float procCoefficient => _procCoefficient;
    public static float _minSpread = 0f;
    public override float minSpread => _minSpread;
    public static float _maxSpread = 0f;
    public override float maxSpread => _maxSpread;
    public static float _baseDuration = 0.4f;
    public override float baseDuration => _baseDuration;

    public static bool _allowTrajectoryAimAssist = false;
    public override bool allowTrajectoryAimAssist => _allowTrajectoryAimAssist;
    public static uint _bulletCount = 1;
    public override uint bulletCount => _bulletCount;
    public static DamageType _damageType = DamageType.Generic;
    public override DamageType damageType => _damageType;
    public static DamageTypeExtended _damageTypeExtended = DamageTypeExtended.Generic;
    public override DamageTypeExtended damageTypeExtended => _damageTypeExtended;
    public static BulletAttack.FalloffModel _falloffModel = BulletAttack.FalloffModel.None;
    public override BulletAttack.FalloffModel falloffModel => _falloffModel;
    public static float _force = 300f;
    public override float force => _force;
    public static float _radius = 2f;
    public override float radius => _radius;
    public static float _trajectoryAimAssistMultiplier = 0f;
    public override float trajectoryAimAssistMultiplier => _trajectoryAimAssistMultiplier;
    public static bool _smartCollision = true;
    public override bool smartCollision => _smartCollision;
    public static float _maxDistance = 2f;
    public override float maxDistance => _maxDistance;
    public static PhysForceFlags _physForceFlags = PhysForceFlags.None;
    public override PhysForceFlags physForceFlags => _physForceFlags;
    public static float _backoffVelocity = 24f;
    public override float backoffVelocity => _backoffVelocity;
    public static float _flyToTargetSpeed = 96f;
    public override float flyToTargetSpeed => _flyToTargetSpeed;
    public static float _findTargetDistance = 48f;
    public override float findTargetDistance => _findTargetDistance;
    public static float _findTargetRadius = 3f;
    public override float findTargetRadius => _findTargetRadius;
    public override bool giveEmpowermentOnExecute => true;

    public static float leftPunchTimerSet = 1f;

    public override void OnEnter()
    {
        base.OnEnter();
        bool leftHandPunch = false;
        if ((this as IBishopState).bishopComponent)
        {
            if ((this as IBishopState).bishopComponent.leftHandPunchTimer > 0f)
            {
                (this as IBishopState).bishopComponent.leftHandPunchTimer = 0f;
                leftHandPunch = true;
            }
            else
            {
                (this as IBishopState).bishopComponent.leftHandPunchTimer = leftPunchTimerSet;
            }
        }
        (this as IBishopState).PlayFirstPersonCrossfade(leftHandPunch ? "LeftArm, Override" : "RightArm, Override", "PunchStart", leftHandPunch ? "leftArm.playbackRate" : "rightArm.playbackRate", attackSpeedStat, 0.05f, true);
        (this as IBishopState).PlayFirstPersonCrossfade(!leftHandPunch ? "LeftArm, Override" : "RightArm, Override", "Idle", !leftHandPunch ? "leftArm.playbackRate" : "rightArm.playbackRate", attackSpeedStat, 0.05f, true);
    }
    public override void OnMeleeHit(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo, HealthComponent victimHealthComponent)
    {
        base.OnMeleeHit(bulletAttack, ref hitInfo, victimHealthComponent);
        Util.PlaySound("Play_DoomTDA_Punch", gameObject);
        if (!wasStaggeredOnHit && hasRequiredStockAndDelay) AddTimeBuffAndResetTimerForAllStacksNetMessage.Send(characterBody, BishopEvents.MeleeCombo.buffIndex, BishopEvents.MeleeComboDuration);
    }
}
