using CaeliImperium.Components;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.NetworkMessages
{
    public class HealReceivedDamageHealRateReportMessage : INetMessage
    {
        public NetworkIdentity networkIdentity;
        public float healRate;
        public float healthCoefficient;
        public HealReceivedDamageHealRateReportMessage()
        {

        }
        public HealReceivedDamageHealRateReportMessage(NetworkIdentity networkIdentity, float healRate, float healthCoefficient)
        {
            this.networkIdentity = networkIdentity;
            this.healRate = healRate;
            this.healthCoefficient = healthCoefficient;
        }
        public void Deserialize(NetworkReader reader)
        {
            networkIdentity = reader.ReadNetworkIdentity();
            healRate = reader.ReadSingle();
            healthCoefficient = reader.ReadSingle();
        }
        public void OnReceived()
        {
            if (!networkIdentity) return;
            HealReceivedDamageVisual healReceivedDamageVisual = networkIdentity.GetOrAddComponent<HealReceivedDamageVisual>();
            healReceivedDamageVisual.UpdateVisuals(healRate, healthCoefficient);
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(networkIdentity);
            writer.Write(healRate);
            writer.Write(healthCoefficient);
        }
    }
}
