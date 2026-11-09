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
        PatchProjectileStateToParriable(typeof(EnemyAbilities.Modules.ExtractorUnit.HookSkill.Hook), nameof(EnemyAbilities.Modules.ExtractorUnit.HookSkill.Hook.FixedUpdate), 0);
        AddParriableProjectileGhostFromProjectile(EnemyAbilities.Modules.ExtractorUnit.HookSkill.projectilePrefab);
        AddParriableStateVisuals<EnemyAbilities.Modules.ExtractorUnit.HookSkill.Hook>();
        PatchOverlapStateToParriable(typeof(EnemyAbilities.Modules.Vulture.Swoop), nameof(EnemyAbilities.Modules.Vulture.Swoop.FixedUpdate));
        AddParriableStateVisuals<EnemyAbilities.Modules.Vulture.Swoop>();
    }
}
