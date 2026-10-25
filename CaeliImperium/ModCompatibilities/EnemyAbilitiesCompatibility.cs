using CaeliImperium.Components.Doomguy;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static CaeliImperium.Bodies.DoomguyEvents;

namespace CaeliImperium.ModCompatibilities;
public static class EnemyAbilitiesCompatibility
{
    public const string GUID = "Skeletogne.EnemyAbilities";
    public static void BishopInit()
    {
        PatchProjectileStateToParriable(typeof(EnemyAbilities.Abilities.ExtractorUnit.HookModule.Hook), nameof(EnemyAbilities.Abilities.ExtractorUnit.HookModule.Hook.FixedUpdate), 0);
        AddParriableProjectileGhostFromProjectile(EnemyAbilities.Abilities.ExtractorUnit.HookModule.projectilePrefab);
        AddParriableStateVisuals<EnemyAbilities.Abilities.ExtractorUnit.HookModule.Hook>();
        PatchOverlapStateToParriable(typeof(EnemyAbilities.Abilities.Vulture.Swoop), nameof(EnemyAbilities.Abilities.Vulture.Swoop.FixedUpdate));
        AddParriableStateVisuals<EnemyAbilities.Abilities.Vulture.Swoop>();
        Hook hook = new Hook(typeof(EnemyAbilities.Abilities.Child.TeleportToEnemyModule.TeleportToEnemy).GetMethod(nameof(EnemyAbilities.Abilities.Child.TeleportToEnemyModule.TeleportToEnemy.OnEnter), BindingFlags.Public | BindingFlags.Instance), typeof(EnemyAbilitiesCompatibility).GetMethod(nameof(TeleportToEnemyOnEnter), BindingFlags.NonPublic | BindingFlags.Static), new HookConfig());
    }
    private static void TeleportToEnemyOnEnter(Action<EnemyAbilities.Abilities.Child.TeleportToEnemyModule.TeleportToEnemy> orig, EnemyAbilities.Abilities.Child.TeleportToEnemyModule.TeleportToEnemy self)
    {
        if (CaeliImperiumUtils.IsDoom())
        {
            self.outer.SetNextStateToMain();
            return;
        }
        orig.Invoke(self);
    }
}
