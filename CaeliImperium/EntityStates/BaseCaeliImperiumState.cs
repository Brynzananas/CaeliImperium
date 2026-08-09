using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaeliImperiumEntityStates
{
    public abstract class BaseCaeliImperiumState : BaseSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            UpdateValues();
        }
        public virtual void UpdateValues()
        {

        }
    }
}
