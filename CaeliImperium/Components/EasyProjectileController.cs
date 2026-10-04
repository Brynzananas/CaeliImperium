using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
public class EasyProjectileController : MonoBehaviour
{
    public float addGravityOnStart = -0.2f;
    public bool teleportFilterToSniperHurtbox;
    public bool teleportFilterTeam;
    public TeamFilter teamFilter;
    public Rigidbody rigidbody;
    public void Awake()
    {
        if (!teamFilter) teamFilter = GetComponent<TeamFilter>();
        if (!rigidbody) rigidbody = GetComponent<Rigidbody>();
    }
    public void Start()
    {
        if (!rigidbody) return;
        if (rigidbody.useGravity) rigidbody.velocity += Physics.gravity * addGravityOnStart;
    }
    public void OnTriggerEnter(Collider collider)
    {
        HurtBox hurtBox = collider.GetComponent<HurtBox>();
        if (!hurtBox) return;
        if (teleportFilterToSniperHurtbox && !hurtBox.isSniperTarget) return;
        if (teleportFilterTeam && teamFilter && hurtBox.healthComponent)
        {
            CharacterBody characterBody = hurtBox.healthComponent.body;
            if (characterBody.teamComponent && !TeamManager.IsTeamEnemy(characterBody.teamComponent.teamIndex, teamFilter.teamIndex)) return;
        }
        Vector3 vector3 = collider.ClosestPoint(transform.position);
        if (rigidbody)
        {
            rigidbody.MovePosition(vector3);
        }
        else
        {
            transform.position = vector3;
        }
    }
}
