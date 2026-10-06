using CaeliImperium.Interactables;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace CaeliImperium;
public class CaeliImperiumTitleController : MonoBehaviour
{
    public static Vector3 ResourceWell1Position = new Vector3(-10.3509f, -5.9382f, 50.1817f);
    public static Vector3 ResourceWell1Scale = new Vector3(3f, 3f, 3f);
    public static float ResourceWell1LineWidth = 6f;
    public static float PostProcessWeight = 0.8f;
    public static Vector3 RefineryPosition = new Vector3(61.6778f, - 1.6237f, 83.5794f);
    public static Vector3 RefineryRotation = new Vector3(0f, 198.7585f, 0f);
    public static Vector3 RefineryScale = new Vector3(-3f, 3f, 3f);
    public Image logoImage;
    public Sprite previousLogoSprite;
    public PostProcessVolume postProcessVolume;
    public bool applied;
    public bool init;
    public GameObject resourceWell1;
    public GameObject refinery;
    public void Awake()
    {
        if (init) return;
        GameObject logoObject = GameObject.Find("MainMenu/MENU: Title/TitleMenu/SafeZone/ImagePanel (JUICED)/LogoImage");
        if (logoObject) logoImage = logoObject.GetComponent<Image>();
        GameObject ppObject = GameObject.Find("Main Camera/GlobalPostProcessVolume, Base");
        if (ppObject)
        {
            PostProcessVolume postProcessVolume = ppObject.GetComponent<PostProcessVolume>();
            if (postProcessVolume)
            {
                this.postProcessVolume = Instantiate(postProcessVolume);
                this.postProcessVolume.priority++;
                this.postProcessVolume.weight = PostProcessWeight;
                this.postProcessVolume.profile = CaeliImperiumAssets.NestPP;
                this.postProcessVolume.enabled = false;
            }
        }
        init = true;
    }

    public void Update()
    {
        if (CaeliImperiumUtils.SelectedCaeliImperiumSurvivor())
        {
            Apply();
        }
        else
        {
            Revert();
        }
    }
    public void Apply()
    {
        if (applied) return;
        applied = true;
        if (logoImage)
        {
            previousLogoSprite = logoImage.sprite;
            logoImage.sprite = CaeliImperiumAssets.CaeliImperiumLogo;
        }
        if (postProcessVolume) postProcessVolume.enabled = true;
        if (!PipelineRefineryEvents.init) return;
        if (resourceWell1)
        {
            resourceWell1.SetActive(true);
        }
        else
        {
            resourceWell1 = Instantiate(PipelineRefineryEvents.ResourceWell, ResourceWell1Position, Quaternion.Euler(0f, 0f, 0f));
            resourceWell1.transform.localScale = ResourceWell1Scale;
            Transform lineTransform = resourceWell1.transform.Find("Display/mdlResourceWell/DeathPillar/Line");
            if (lineTransform)
            {
                LineRenderer lineRenderer = lineTransform.GetComponent<LineRenderer>();
                if (lineRenderer)
                {
                    lineRenderer.startWidth = ResourceWell1LineWidth;
                    lineRenderer.endWidth = ResourceWell1LineWidth;
                }
            }
        }
        if (refinery)
        {
            refinery.SetActive(true);
        }
        else
        {
            refinery = Instantiate(PipelineRefineryEvents.PipelineRefinery, RefineryPosition, Quaternion.Euler(RefineryRotation));
            refinery.transform.localScale = RefineryScale;
        }
    }
    public void Revert()
    {
        if (!applied) return;
        applied = false;
        if (logoImage) logoImage.sprite = previousLogoSprite;
        if (postProcessVolume) postProcessVolume.enabled = false;
        if (resourceWell1) resourceWell1.SetActive(false);
        if (refinery) refinery.SetActive(false);
    }
}
