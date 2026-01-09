using CaeliImperium.Components;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ItemBehaviours
{
    public class TeleportAroundOpenedChestsBehaviour : CharacterBody.ItemBehavior
    {
        public void FixedUpdate()
        {
            if (!body.inputBank) return;
            if (body.inputBank.interact.down && body.inputBank.jump.justPressed)
            {
                Ray ray = body.inputBank ? new Ray { direction = body.inputBank.aimDirection, origin = body.inputBank.aimOrigin } : new Ray { direction = body.transform.rotation.eulerAngles, origin = body.corePosition };
                Leap(ray);
            }
        }
        public void Leap(Ray ray)
        {
            RaycastHit[] raycastHits = Physics.RaycastAll(ray, 9999f, LayerIndex.noCollision.mask, QueryTriggerInteraction.UseGlobal);
            float distance = 9999f;
            WormholeComponent wormholeComponent2 = null;
            foreach (RaycastHit hit in raycastHits)
            {
                WormholeComponent wormholeComponent = hit.collider.GetComponent<WormholeComponent>();
                if (!wormholeComponent) continue;
                if (distance > hit.distance)
                {
                    wormholeComponent2 = wormholeComponent;
                    distance = hit.distance;
                }
            }
            if (wormholeComponent2)
            {
                wormholeComponent2.Teleport(body);
            }
        }
    }
}
