using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components.GooseRunner;

public class GooseRunnerObstacle : MonoBehaviour
{
    public GooseRunnerObstacleType type;
    public void OnTriggerEnter(Collider other)
    {
        if (!GooseRunnerManager.instance) return;
        GooseRunnerPlayerController gooseRunnerPlayerController = other.GetComponent<GooseRunnerPlayerController>();
        if (!gooseRunnerPlayerController) return;
        bool safe = type switch
        {
            GooseRunnerObstacleType.Slide => gooseRunnerPlayerController.IsSliding,
            GooseRunnerObstacleType.Jump => gooseRunnerPlayerController.IsAirborne,
            GooseRunnerObstacleType.FullBlock => false,
            _ => false
        };
        if (!safe) GooseRunnerManager.instance.GameOver();
    }
}
public enum GooseRunnerObstacleType
{
    FullBlock,
    Slide,
    Jump
}
