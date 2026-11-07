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
    public static void DoomguyInit()
    {
        PatchProjectileStateToParriable(typeof(EnemyAbilities.Abilities.ExtractorUnit.HookModule.Hook), nameof(EnemyAbilities.Abilities.ExtractorUnit.HookModule.Hook.FixedUpdate), 0);
        AddParriableProjectileGhostFromProjectile(EnemyAbilities.Abilities.ExtractorUnit.HookModule.projectilePrefab);
        AddParriableStateVisuals<EnemyAbilities.Abilities.ExtractorUnit.HookModule.Hook>();
        PatchOverlapStateToParriable(typeof(EnemyAbilities.Abilities.Vulture.Swoop), nameof(EnemyAbilities.Abilities.Vulture.Swoop.FixedUpdate));
        AddParriableStateVisuals<EnemyAbilities.Abilities.Vulture.Swoop>();
    }
}
