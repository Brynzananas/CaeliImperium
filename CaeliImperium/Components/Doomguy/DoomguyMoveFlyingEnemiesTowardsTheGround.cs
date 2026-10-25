using CaeliImperium.Components.Doomguy;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components.Doomguy;
public class DoomguyMoveFlyingEnemiesTowardsTheGround : MonoBehaviour
{
    public float raycastLengthForGround;
    public float raycastLengthForCeiling;
    public float distanceAboveGround;
    public float distanceBelowCeiling;
    public float maxSpeed;
    public LayerMask collisionMask;
    public CharacterMotor characterMotor;
    public RigidbodyMotor rigidbodyMotor;
    public CharacterBody characterBody;
    public float updateInterval;
    public float stopwatch;
    public float targetY;
    public bool move;
    public void Awake()
    {
        characterMotor = GetComponent<CharacterMotor>();
        rigidbodyMotor = GetComponent<RigidbodyMotor>();
        characterBody = GetComponent<CharacterBody>();
    }
    public void FixedUpdate()
    {
        if (!CaeliImperiumUtils.IsDoom()) return;
        if (!characterBody) return;
        if (stopwatch > 0f)
        {
            stopwatch -= Time.fixedDeltaTime;
        }
        else
        {
            UpdateSituation();
            stopwatch = updateInterval;
        }
        if (!move) return;
        float deltaY = Mathf.Clamp(targetY - transform.position.y, -maxSpeed, maxSpeed);
        Vector3 rootMotion = new Vector3(0f, deltaY, 0f) * Time.fixedDeltaTime;
        if (characterMotor)
        {
            characterMotor.rootMotion += rootMotion;
        }
        else if (rigidbodyMotor)
        {
            rigidbodyMotor.rootMotion += rootMotion;
        }
    }
    public void UpdateSituation()
    {
        if (Physics.SphereCast(transform.position, characterBody.radius, Vector3.down, out RaycastHit hitInfo, raycastLengthForGround, collisionMask))
        {
            Vector3 vector3 = hitInfo.point + (Vector3.up * distanceAboveGround);
            if (Physics.SphereCast(transform.position, characterBody.radius, Vector3.up, out RaycastHit hitInfo2, raycastLengthForCeiling, collisionMask))
            {
                vector3 -= Vector3.down * distanceBelowCeiling;
            }
            targetY = vector3.y;
            move = true;
        }
        else
        {
            move = false;
        }
    }
}
