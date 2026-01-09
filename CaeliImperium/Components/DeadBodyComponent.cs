using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CaeliImperium.Components
{
    public class DeadBodyComponent : MonoBehaviour
    {
        public Inventory inventory;
        public Loadout loadout;
        public string bodyName;
        public GameObject indicator;
        public GameObject bodyPrefab;
        public GameObject masterPrefab;
        public void Awake()
        {
            indicator = Instantiate(CaeliImperiumAssets.childProjectileGhost, transform);
            ProjectileGhostController projectileGhostController = indicator.GetComponent<ProjectileGhostController>();
            Destroy(projectileGhostController);
        }
        public void OnEnable()
        {
            Events.deadBodyComponents.Add(this);
        }
        public void OnDisable()
        {
            Events.deadBodyComponents.Remove(this);
        }
        public void Update()
        {
            if (NetworkUser.instancesList != null && NetworkUser.instancesList[0] && NetworkUser.instancesList[0].master && NetworkUser.instancesList[0].master.inventory && NetworkUser.instancesList[0].master.inventory.currentEquipmentIndex == CaeliImperiumContent.Equipments.Necronomicon.equipmentIndex)
            {
                if (!indicator.activeSelf)
                    indicator.SetActive(true);
            }
            else
            {
                if (indicator.activeSelf)
                    indicator.SetActive(false);
            }
        }
        public void Revive(CharacterBody characterBody, bool destroy)
        {
            if (!NetworkServer.active && bodyPrefab) return;
            CharacterMaster characterMaster = new MasterSummon
            {
                position = transform.position,
                ignoreTeamMemberLimit = true,
                masterPrefab = masterPrefab,
                summonerBodyObject = characterBody.gameObject,
                rotation = transform.rotation,
                inventoryToCopy = inventory,
                //loadout = loadout,
            }.Perform();
            if (characterMaster && characterMaster.inventory)
            {
                characterMaster.inventory.GiveItem(RoR2Content.Items.HealthDecay, 30);
                characterMaster.inventory.GiveItem(RoR2Content.Items.Ghost);
            }
            if (destroy) Destroy(gameObject);
        }
    }
}
