using BrynzaAPI;
using CaeliImperium;
using CaeliImperium.Items;
using EntityStates;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperiumEntityStates.ClaySwordsman;

public class Swing : BaseClaySwordsmanState
{
    public static bool debug = true;
    public static float overlayAlpha = 2f;
    public static float debugDistance = 6f;
    public static float debugRadius = 0.5f;
    public static float baseDuration = 0.6f;
    public static float baseAttackDuration = 0.3f;
    public static float baseCrossfadeDuration = 0.01f;
    public static int maxSwings = 5;
    public static float damageCoefficient = 5f;
    public static float procCoefficient = 1f;
    public static AttackerFiltering attackerFiltering = AttackerFiltering.Default;
    public static DamageType damageType = DamageType.Generic;
    public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
    public static PhysForceFlags physForceFlags = PhysForceFlags.None;
    public static float pushAwayForce = 100f;
    public float duration;
    public float attackDuration;
    public float crossfadeDuration;
    public float cachedAttackSpeed;
    public int swing;
    public HitBoxGroup hitBoxGroup;
    public OverlapAttack overlapAttack;
    public Ray aimRay;
    public TemporaryOverlayInstance temporaryOverlayInstance;
    public CharacterModel characterModel;

    public override void OnSerialize(NetworkWriter writer)
    {
        base.OnSerialize(writer);
        writer.Write(cachedAttackSpeed);
    }
    public override void OnDeserialize(NetworkReader reader)
    {
        base.OnDeserialize(reader);
        cachedAttackSpeed = reader.ReadSingle();
    }
    public override void OnEnter()
    {
        base.OnEnter();
        duration = baseDuration / cachedAttackSpeed;
        attackDuration = baseAttackDuration / cachedAttackSpeed;
        crossfadeDuration = baseCrossfadeDuration / cachedAttackSpeed;
        Transform modelTransform = GetModelTransform();
        if (modelTransform) characterModel = modelTransform.GetComponent<CharacterModel>();
        Vector3 aimVector;
        aimRay = GetAimRay();
        GameObject target = GetTarget();
        if (target)
        {
            bool predict = swing == 0 || swing % 2 == 0;
            if (predict)
            {
                CharacterBody characterBody = target.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    Vector3 targetPosition = target.transform.position + (characterBody.GetPositionDelta() * attackDuration);
                    aimVector = targetPosition - aimRay.origin;
                }
                else
                {
                    aimVector = target.transform.position - aimRay.origin;
                }
            }
            else
            {
                aimVector = target.transform.position - aimRay.origin;
            }
            AddOverlay(predict);
        }
        else
        {
            aimVector = aimRay.direction * debugDistance;
        }

        if (debug) BrynzaAPI.Utils.CreateDebugCapsule(aimRay.origin, aimRay.origin + aimVector, debugRadius, Color.red, duration);
        aimVector.Normalize();
        aimRay.direction = aimVector;
        StartAimMode(aimRay, 2f, true);
        PlayAnimation("FullBody, Override", "SwingDown", "SwingDown.playbackRate", duration);
        //PlayCrossfadeOnSword("Body", "SwingDown", "SwingDown.playbackRate", duration, crossfadeDuration);
        hitBoxGroup = FindHitBoxGroup("Sword");
        if (!isAuthority) return;
        if (hitBoxGroup)
        {
            overlapAttack = new OverlapAttack
            {
                damage = characterBody.damage * damageCoefficient,
                attacker = gameObject,
                attackerFiltering = attackerFiltering,
                hitBoxGroup = hitBoxGroup,
                damageType = new DamageTypeCombo(damageType, damageTypeExtended, this.GetDamageSource(DamageSource.Primary)),
                inflictor = gameObject,
                isCrit = RollCrit(),
                physForceFlags = physForceFlags,
                procCoefficient = procCoefficient,
                pushAwayForce = pushAwayForce,
                teamIndex = GetTeam()
            };
        }
    }
    public void AddOverlay(bool predict)
    {
        if (!characterModel) return;
        temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(characterModel.gameObject);
        temporaryOverlayInstance.originalMaterial = predict ? CaeliImperiumAssets.Predict : CaeliImperiumAssets.DoesntPredict;
        temporaryOverlayInstance.inspectorCharacterModel = characterModel;
        temporaryOverlayInstance.assignedCharacterModel = characterModel;
        temporaryOverlayInstance.AddToCharacterModel(characterModel);
    }
    public GameObject GetTarget()
    {
        if (!characterBody) return null;
        CharacterMaster characterMaster = characterBody.master;
        if (!characterMaster) return null;
        BaseAI[] baseAIs = characterMaster.aiComponents;
        if (baseAIs == null || baseAIs.Length == 0) return null;
        BaseAI baseAI = baseAIs[0];
        if (!baseAI || baseAI.currentEnemy == null) return null;
        return baseAI.currentEnemy.gameObject;
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        StartAimMode(aimRay, 2f, true);
        if (overlapAttack != null && fixedAge < attackDuration) overlapAttack.Fire();
        if (!isAuthority) return;
        if (fixedAge < duration) return;
        if (swing >= maxSwings)
        {
            outer.SetNextStateToMain();
        }
        else
        {
            outer.SetNextState(new Swing { activatorSkillSlot = activatorSkillSlot, cachedAttackSpeed = cachedAttackSpeed, swing = swing + 1});
        }
    }
    public override void OnExit()
    {
        base.OnExit();
        if (temporaryOverlayInstance != null)
        {
            temporaryOverlayInstance.RemoveFromCharacterModel();
            TemporaryOverlayManager.RemoveOverlay(temporaryOverlayInstance.managerIndex);
        }
        
    }
}
