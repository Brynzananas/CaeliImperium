using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaeliImperium.ItemBehaviours
{
    public class InflictIrradiatedOnHitBehaviour : CharacterBody.ItemBehavior
    {
        private void OnDisable()
        {
            if (!this.body)  return;
            if (this.body.HasBuff(CaeliImperium.Items.InflictIrradiatedOnHitEvents.Ready))
            {
                this.body.RemoveBuff(CaeliImperium.Items.InflictIrradiatedOnHitEvents.Ready);
            }
            if (this.body.HasBuff(CaeliImperium.Items.InflictIrradiatedOnHitEvents.Recharging))
            {
                this.body.RemoveBuff(CaeliImperium.Items.InflictIrradiatedOnHitEvents.Recharging);
            }
        }
        private void FixedUpdate()
        {
            bool flag = this.body.HasBuff(CaeliImperium.Items.InflictIrradiatedOnHitEvents.Recharging);
            bool flag2 = this.body.HasBuff(CaeliImperium.Items.InflictIrradiatedOnHitEvents.Ready);
            if (!flag && !flag2)
            {
                this.body.AddBuff(CaeliImperium.Items.InflictIrradiatedOnHitEvents.Ready);
            }
            if (flag2 && flag)
            {
                this.body.RemoveBuff(CaeliImperium.Items.InflictIrradiatedOnHitEvents.Ready);
            }
        }
    }
}
