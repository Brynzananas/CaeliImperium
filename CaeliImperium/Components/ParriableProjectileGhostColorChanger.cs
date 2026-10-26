using CaeliImperium.Misc;
using RoR2.Projectile;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace CaeliImperium.Components;
public class ParriableProjectileGhostColorChanger : MonoBehaviour
{
    public static float ParriableBrightnessMultiplier = 2f;
    public static float NonParriableBrightnessMultiplier = 1f;
    private static HashSet<string> remapShaderNames = new HashSet<string> { "Hopoo Games/FX/Cloud Remap", "Hopoo Games/FX/Cloud Intersection Remap", "Hopoo Games/FX/Opaque Cloud Remap", "Hopoo Games/Optimized/Switch/FX/CloudIntersectionRemap", "Hopoo Games/Optimized/Switch/FX/CloudIntersectionRemap_TwoSides", "Hopoo Games/Optimized/Switch/FX/OpaqueCloudRemap", "Hopoo Games/Optimized/Switch/FX/OpaqueCloudRemap_Specular", "Hopoo Games/Optimized/Switch/FX/CloudRemap", "Hopoo Games/UI/UI Bar Remap" };
    public static Dictionary<Texture2D, Texture2D> defaultTextureToParriableTexture = [];
    public static Dictionary<Texture2D, Texture2D> defaultTextureToNonParriableTexture = [];
    public static Dictionary<Material, Material> defaultMaterialToParriableMaterial = [];
    public static Dictionary<Material, Material> defaultMaterialToNonParriableMaterial = [];
    public ProjectileGhostController projectileGhostController;
    public Renderer[] renderers;
    public Light[] lights;
    public bool init;
    public SerializableDictionary<Light, Color> defaultLightColors = [];
    public SerializableDictionary<Renderer, MaterialArray> defaultMaterials = [];
    public SerializableDictionary<Renderer, MaterialArray> parriabletMaterials = [];
    public SerializableDictionary<Renderer, MaterialArray> nonParriableMaterials = [];
    public Color parriableLightColor;
    public Color nonParriableLightColor;
    private AppliedType appliedParriable;
    private uint soundId;
    
    public void Init(Gradient gradient, Gradient gradient1)
    {
        parriableLightColor = gradient.colorKeys.Last().color;
        nonParriableLightColor = gradient1.colorKeys.Last().color;
        projectileGhostController = GetComponent<ProjectileGhostController>();
        renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (!renderer) continue;
            Material[] materials = new Material[renderer.sharedMaterials.Length];
            Material[] materials1 = new Material[renderer.sharedMaterials.Length];
            Material[] materials2 = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                Material material = renderer.sharedMaterials[i];
                if (!material) continue;
                Handle(defaultMaterialToParriableMaterial, defaultTextureToParriableTexture, material, gradient, ParriableBrightnessMultiplier, out Material parriableMaterial);
                Handle(defaultMaterialToNonParriableMaterial, defaultTextureToNonParriableTexture, material, gradient1, NonParriableBrightnessMultiplier, out Material nonParriableMaterial);
                materials[i] = material;
                materials1[i] = parriableMaterial;
                materials2[i] = nonParriableMaterial;
            }
            defaultMaterials[renderer] = materials;
            parriabletMaterials[renderer] = materials1;
            nonParriableMaterials[renderer] = materials2;
        }
        lights = GetComponentsInChildren<Light>();
        foreach (Light light in lights) defaultLightColors[light] = light.color;
        init = true;
    }
    private void Handle(Dictionary<Material, Material> keyValuePairs, Dictionary<Texture2D, Texture2D> keyValuePairs1, Material material, Gradient gradient, float brightnessMultiplier, out Material parriableMaterial)
    {
        parriableMaterial = null;
        if (!keyValuePairs.TryGetValue(material, out parriableMaterial))
        {
            if (material.shader.name == "Hopoo Games/FX/Distortion")
            {
                parriableMaterial = material;
                keyValuePairs[material] = parriableMaterial;
            }
            bool isRemap = remapShaderNames.Contains(material.shader.name);
            Texture2D texture2D;
            if (isRemap)
            {
                Texture texture = material.GetTexture("_RemapTex");
                if (texture is Texture2D texture2)
                {
                    texture2D = texture2;
                }
                else
                {
                    texture2D = null;
                }
            }
            else if (material.HasTexture("_MainTex") && material.mainTexture && material.mainTexture is Texture2D texture21)
            {
                texture2D = texture21;
            }
            else
            {
                texture2D = null;
            }
            if (!texture2D)
            {
                keyValuePairs[material] = material;
                return;
            }
            if (!keyValuePairs1.TryGetValue(texture2D, out Texture2D texture2D1))
            {
                texture2D1 = texture2D.ApplyGradientToTexture(gradient);
                keyValuePairs1[texture2D] = texture2D1;
            }
            parriableMaterial = GameObject.Instantiate(material);
            if (isRemap)
            {
                parriableMaterial.SetTexture("_RemapTex", texture2D1);
            }
            else if (material.HasTexture("_MainTex"))
            {
                parriableMaterial.mainTexture = texture2D1;
                if (parriableMaterial.HasTexture("_EmTex"))
                {
                    parriableMaterial.SetTexture("_EmTex", texture2D1);
                    parriableMaterial.SetColor("_EmColor", Color.white);
                }
            }
            if (parriableMaterial.HasFloat("_Boost"))
            {
                parriableMaterial.SetFloat("_Boost", parriableMaterial.GetFloat("_Boost") * brightnessMultiplier);
            }
            keyValuePairs[material] = parriableMaterial;
        }
        if (parriableMaterial == null) parriableMaterial = material;
    }
    public void ApplyDefaultMaterial()
    {
        if (appliedParriable == AppliedType.Default) return;
        foreach (Renderer renderer in renderers)
        {
            if (!renderer) continue;
            if (!defaultMaterials.TryGetValue(renderer, out MaterialArray materials)) continue;
            renderer.sharedMaterials = materials;
        }
        foreach (Light light in lights)
        {
            if (defaultLightColors.TryGetValue(light, out Color color)) light.color = color;
        }
        appliedParriable = AppliedType.Default;
    }
    public void ApplyParriableMaterial()
    {
        if (appliedParriable == AppliedType.Parriable) return;
        foreach (Renderer renderer in renderers)
        {
            if (!renderer) continue;
            if (!parriabletMaterials.TryGetValue(renderer, out MaterialArray materials)) continue;
            renderer.sharedMaterials = materials;
        }
        foreach (Light light in lights)
        {
            light.color = parriableLightColor;
        }
        appliedParriable = AppliedType.Parriable;
    }
    public void ApplyNonParriableMaterial()
    {
        if (appliedParriable == AppliedType.NonParriable) return;
        foreach (Renderer renderer in renderers)
        {
            if (!renderer) continue;
            if (!nonParriableMaterials.TryGetValue(renderer, out MaterialArray materials)) continue;
            renderer.sharedMaterials = materials;
        }
        foreach (Light light in lights)
        {
            light.color = nonParriableLightColor;
        }
        appliedParriable = AppliedType.NonParriable;
    }
    private enum AppliedType
    {
        Default,
        Parriable,
        NonParriable
    }
}
