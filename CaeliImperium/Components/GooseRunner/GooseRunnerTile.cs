using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components.GooseRunner;
public class GooseRunnerTile : MonoBehaviour
{
    public float length = 20f;
    private List<GameObject> _spawned = [];
    public void RegisterSpawned(GameObject gameObject)
    {
        _spawned.Add(gameObject);
    }
    public void ClearSpawned()
    {
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] != null) Destroy(_spawned[i]);
        }
        _spawned.Clear();
    }
}
