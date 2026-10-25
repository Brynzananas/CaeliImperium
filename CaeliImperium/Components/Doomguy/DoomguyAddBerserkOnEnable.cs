using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components.Doomguy;
public class DoomguyAddBerserkOnEnable : MonoBehaviour
{
    public void OnEnable()
    {
        DoomguyComponent.AddBerserk();
    }
    public void OnDisable()
    {
        DoomguyComponent.RemoveBerserk();
    }
}
