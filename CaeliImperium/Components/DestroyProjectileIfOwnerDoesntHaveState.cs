using EntityStates;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components;
[RequireComponent(typeof(ProjectileController))]
public class DestroyProjectileIfOwnerDoesntHaveState : MonoBehaviour
{
    public SerializableEntityStateType state;
    public ProjectileController projectileController;
    private NetworkStateMachine networkStateMachine;
    public void Awake()
    {
        if (!projectileController) projectileController = GetComponent<ProjectileController>();
    }
    public void FixedUpdate()
    {
        if (!NetworkServer.active) return;
        if (!projectileController)
        {
            Destroy(gameObject);
            return;
        }
        if (projectileController.owner)
        {
            if (!networkStateMachine || networkStateMachine.gameObject != projectileController.owner)
            {
                networkStateMachine = projectileController.owner.GetComponent<NetworkStateMachine>();
            }
            if (networkStateMachine && networkStateMachine.stateMachines != null)
            {
                bool foundState = false;
                foreach (EntityStateMachine entityStateMachine in networkStateMachine.stateMachines)
                {
                    if (!entityStateMachine || entityStateMachine.state == null) continue;
                    if (entityStateMachine.state.GetType() == state.stateType)
                    {
                        foundState = true;
                        break;
                    }
                }
                if (!foundState)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
