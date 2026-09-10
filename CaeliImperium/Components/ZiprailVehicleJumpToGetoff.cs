using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components;
[RequireComponent(typeof(ZiprailVehicle))]
public class ZiprailVehicleJumpToGetoff : MonoBehaviour
{
    public ZiprailVehicle ziprailVehicle;
    public void Awake()
    {
        if (!ziprailVehicle) ziprailVehicle = GetComponent<ZiprailVehicle>();
    }
    public void FixedUdpate()
    {
        if (!ziprailVehicle) return;
        VehicleSeat vehicleSeat = ziprailVehicle.vehicleSeat;
        if (!vehicleSeat) return;
        CharacterBody characterBody = vehicleSeat.currentPassengerBody;
        if (!characterBody || !characterBody.hasEffectiveAuthority) return;
        InputBankTest inputBankTest = characterBody.inputBank;
        if (!inputBankTest || !inputBankTest.jump.justPressed) return;
        if (NetworkServer.active)
        {
            vehicleSeat.EjectPassenger();
        }
        else
        {
            vehicleSeat.CallCmdEjectPassenger();
        }
    }
}
