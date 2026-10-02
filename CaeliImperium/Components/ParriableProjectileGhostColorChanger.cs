using RoR2.Projectile;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;
public class ParriableProjectileGhostColorChanger : MonoBehaviour
{
    public static float BrightnessMultiplier;
    private static HashSet<string> remapShaderNames = new HashSet<string> { "Hopoo Games/FX/Cloud Remap", "Hopoo Games/FX/Cloud Intersection Remap", "Hopoo Games/FX/Opaque Cloud Remap", "Hopoo Games/Optimized/Switch/FX/CloudIntersectionRemap", "Hopoo Games/Optimized/Switch/FX/CloudIntersectionRemap_TwoSides", "Hopoo Games/Optimized/Switch/FX/OpaqueCloudRemap", "Hopoo Games/Optimized/Switch/FX/OpaqueCloudRemap_Specular", "Hopoo Games/Optimized/Switch/FX/CloudRemap", "Hopoo Games/UI/UI Bar Remap" };
    public static Dictionary<Texture2D, Texture2D> defaultTextureToParriableTexture = [];
    public static Dictionary<Material, Material> defaultMaterialToParriableMaterial = [];
    public static Dictionary<Material, Material> parriableMaterialToDefaultMaterial = [];
    public ProjectileGhostController projectileGhostController;
    public Renderer[] renderers;
    public bool init;
    private bool appliedParriable;
    public void Init(Gradient gradient)
    {
        projectileGhostController = GetComponent<ProjectileGhostController>();
        renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (!renderer) continue;
            foreach (Material material in renderer.sharedMaterials)
            {
                if (!material) continue;
                if (!defaultMaterialToParriableMaterial.TryGetValue(material, out Material parriableMaterial))
                {
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
                    else if (material.mainTexture && material.mainTexture is Texture2D texture21)
                    {
                        texture2D = texture21;
                    }
                    else
                    {
                        texture2D = null;
                    }
                    if (!texture2D)
                    {
                        defaultMaterialToParriableMaterial.Add(material, material);
                        parriableMaterialToDefaultMaterial.Add(material, material);
                        continue;
                    }
                    if (!defaultTextureToParriableTexture.TryGetValue(texture2D, out Texture2D texture2D1))
                    {
                        texture2D1 = texture2D.ApplyGradientToTexture(gradient);
                        defaultTextureToParriableTexture.Add(texture2D, texture2D1);
                    }
                    parriableMaterial = GameObject.Instantiate(material);
                    if (isRemap)
                    {
                        parriableMaterial.SetTexture("_RemapTex", texture2D1);
                    }
                    else
                    {
                        parriableMaterial.mainTexture = texture2D1;
                        if (parriableMaterial.HasTexture("_EmTex"))
                        {
                            parriableMaterial.SetTexture("_EmTex", texture2D1);
                            parriableMaterial.SetColor("_EmColor", Color.white);
                        }
                        if (parriableMaterial.HasFloat("_Boost"))
                        {
                            parriableMaterial.SetFloat("_Boost", parriableMaterial.GetFloat("_Boost") * BrightnessMultiplier);
                        }
                    }
                    defaultMaterialToParriableMaterial.Add(material, parriableMaterial);
                    parriableMaterialToDefaultMaterial.Add(parriableMaterial, material);
                }
            }
        }
        init = true;
    }
    public void ApplyDefaultMaterial()
    {
        if (!appliedParriable) return;
        foreach (Renderer renderer in renderers)
        {
            if (!renderer) continue;
            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                Material material = renderer.sharedMaterials[i];
                if (!material) continue;
                if (parriableMaterialToDefaultMaterial.TryGetValue(material, out Material defaultMaterial))
                {
                    materials[i] = defaultMaterial;
                }
                else
                {
                    materials[i] = material;
                }
            }
            renderer.sharedMaterials = materials;
        }
        appliedParriable = false;
    }
    public void ApplyParriableMaterial()
    {
        if (appliedParriable) return;
        foreach (Renderer renderer in renderers)
        {
            if (!renderer) continue;
            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                Material material = renderer.sharedMaterials[i];
                if (!material) continue;
                if (defaultMaterialToParriableMaterial.TryGetValue(material, out Material parriableMaterial))
                {
                    materials[i] = parriableMaterial;
                }
                else
                {
                    materials[i] = material;
                }
            }
            renderer.sharedMaterials = materials;
        }
        appliedParriable = true;
    }
}
