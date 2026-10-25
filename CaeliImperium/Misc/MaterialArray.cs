using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Misc;
[System.Serializable]
public struct MaterialArray
{
    public Material[] materials;
    public static implicit operator Material[](MaterialArray wrapper) => wrapper.materials;
    public static implicit operator MaterialArray(Material[] array) => new MaterialArray { materials = array };
}
