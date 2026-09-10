using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components.GooseRunner;
public class GooseRunnerPickup : MonoBehaviour
{
    public uint value;
    public void OnTriggerEnter(Collider other)
    {
        if (!TeamManager.instance || !GooseRunnerManager.instance) return;
        GooseRunnerPlayerController gooseRunnerPlayerController = other.GetComponent<GooseRunnerPlayerController>();
        if (!gooseRunnerPlayerController || !gooseRunnerPlayerController.characterMaster) return;
        TeamManager.instance.GiveTeamMoney(gooseRunnerPlayerController.characterMaster.teamIndex, value);
        gameObject.SetActive(false);
    }
}
