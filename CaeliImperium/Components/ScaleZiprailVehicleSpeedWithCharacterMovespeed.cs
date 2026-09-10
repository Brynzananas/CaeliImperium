using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
[RequireComponent(typeof(ZiprailVehicle))]
public class ScaleZiprailVehicleSpeedWithCharacterMovespeed : MonoBehaviour
{
    public ZiprailVehicle ziprailVehicle;
    public void Awake()
    {
        if (!ziprailVehicle) ziprailVehicle = GetComponent<ZiprailVehicle>();
    }
    public void FixedUpdate()
    {
        if (!ziprailVehicle) return;
        CharacterBody characterBody = ziprailVehicle._passengerBody;
        if (!characterBody) return;
        ziprailVehicle._speed = characterBody.moveSpeed;
    }
}
