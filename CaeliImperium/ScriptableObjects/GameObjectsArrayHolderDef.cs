using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.ScriptableObjects;
[CreateAssetMenu(menuName = "CaeliImperium/GameObjectsArrayHolderDef")]
public class GameObjectsArrayHolderDef : ScriptableObject
{
    public GameObject[] gameObjects;
    public void Add(GameObject gameObject) => HG.ArrayUtils.ArrayAppend(ref gameObjects, gameObject);
}
