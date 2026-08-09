using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    public class TeamSharedInventory : MonoBehaviour
    {
        private static Dictionary<TeamIndex, TeamSharedInventory> teamIndexToTeamSharedInventory = [];
        private Inventory inventory;
        private TeamIndex teamIndex;
        public static int GetTeamItemCountEffective(TeamIndex teamIndex, ItemDef itemDef) => GetTeamItemCountEffective(teamIndex, itemDef.itemIndex);
        public static int GetTeamItemCountEffective(TeamIndex teamIndex, ItemIndex itemIndex)
        {
            TeamSharedInventory teamSharedInventory = GetTeamSharedInventory(teamIndex);
            return teamSharedInventory.inventory.GetItemCountEffective(itemIndex);
        }
        private static TeamSharedInventory GetTeamSharedInventory(TeamIndex teamIndex)
        {
            if (!teamIndexToTeamSharedInventory.TryGetValue(teamIndex, out TeamSharedInventory teamSharedInventory))
            {
                GameObject gameObject = new GameObject("TeamSharedInventory");
                GameObject.DontDestroyOnLoad(gameObject);
                teamSharedInventory = gameObject.AddComponent<TeamSharedInventory>();
                teamSharedInventory.inventory = gameObject.AddComponent<Inventory>();
                teamSharedInventory.teamIndex = teamIndex;
                teamIndexToTeamSharedInventory.Add(teamIndex, teamSharedInventory);
            }
            return teamSharedInventory;
        }
        public static void GiveTeamItemCountEffective(TeamIndex teamIndex, ItemDef itemDef, int itemCount = 1) => GiveTeamItemCountEffective(teamIndex, itemDef.itemIndex, itemCount);
        public static void GiveTeamItemCountEffective(TeamIndex teamIndex, ItemIndex itemIndex, int itemCount = 1)
        {
            TeamSharedInventory teamSharedInventory = GetTeamSharedInventory(teamIndex);
            teamSharedInventory.inventory.GiveItemPermanent(itemIndex, itemCount);
        }
        public static void RemoveTeamItemCountEffective(TeamIndex teamIndex, ItemDef itemDef, int itemCount = 1) => RemoveTeamItemCountEffective(teamIndex, itemDef.itemIndex, itemCount);
        public static void RemoveTeamItemCountEffective(TeamIndex teamIndex, ItemIndex itemIndex, int itemCount = 1)
        {
            TeamSharedInventory teamSharedInventory = GetTeamSharedInventory(teamIndex);
            teamSharedInventory.inventory.RemoveItemPermanent(itemIndex, itemCount);
        }
        public void OnDestroy()
        {
            if (teamIndexToTeamSharedInventory.ContainsKey(teamIndex)) teamIndexToTeamSharedInventory.Remove(teamIndex);
        }
    }
}
