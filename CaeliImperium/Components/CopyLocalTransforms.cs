using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
public class CopyLocalTransforms : MonoBehaviour
{
    public Transform transformToCopy;
    public void Update()
    {
        Copy();
    }
    public void Copy()
    {
        if (!transformToCopy) return;
        transform.parent = transformToCopy.parent;
        transform.localPosition = transformToCopy.localPosition;
        transform.localRotation = transformToCopy.localRotation;
        transform.localScale = transformToCopy.localScale;
    }
}
