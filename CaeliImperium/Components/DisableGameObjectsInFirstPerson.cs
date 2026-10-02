using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
public class DisableGameObjectsInFirstPerson : MonoBehaviour
{
    public GameObject[] gameObjects;
    private bool disabled;
    private void EnableGameObjects()
    {
        if (!disabled) return;
        disabled = false;
        if (gameObjects == null) return;
        foreach (GameObject go in gameObjects)
        {
            if (!go) continue;
            if (!go.activeSelf) go.SetActive(true);
        }
    }
    private void DisableGameObjects()
    {
        if (disabled) return;
        disabled = true;
        if (gameObjects == null) return;
        foreach (GameObject go in gameObjects)
        {
            if (!go) continue;
            if (go.activeSelf) go.SetActive(false);
        }
    }
    private void Update()
    {
        if (FirstPersonCameraController.instance)
        {
            DisableGameObjects();
        }
        else
        {
            EnableGameObjects();
        }
    }
}
