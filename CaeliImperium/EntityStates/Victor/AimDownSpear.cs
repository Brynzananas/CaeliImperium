using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaeliImperiumEntityStates.Victor
{
    public class AimDownSpear : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Play_Deadlock_Venator_Crossbow_Down", gameObject);
            if (!isAuthority) return;
            outer.SetNextStateToMain();
        }
    }
}
