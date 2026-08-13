using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaeliImperium
{
    public class CaeliImperiumRunAction
    {
        public CaeliImperiumExpansionRunComponent caeliImperiumExpansionRunComponent;
        public PlayerCharacterMasterController currentPlayerCharacterMasterController => caeliImperiumExpansionRunComponent?.currentPlayerCharacterMasterController;
        public CharacterMaster currentCharacterMaster => caeliImperiumExpansionRunComponent?.currentCharacterMaster;
        public CharacterBody currentCharacterBody => caeliImperiumExpansionRunComponent?.currentCharacterBody;
        public virtual void FixedUpdate()
        {

        }
        public virtual void Update()
        {

        }
    }
}
