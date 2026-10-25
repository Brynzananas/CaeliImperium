using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaeliImperium.Components.Refinery;
public class PipelineBuildInfoHUDController : MonoBehaviour
{
    public PipelineBuilder pipelineBuilder;
    public string errorToken;
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    public Image angleBar;
    public TextMeshProUGUI angleText;
    public Image distanceBar;
    public TextMeshProUGUI distanceText;
    public string collidesValidToken;
    public string collidesInvalidToken;
    public TextMeshProUGUI collisionText;
    public void Update()
    {
        PipelineBuilder.BuildInfo? buildInfo = pipelineBuilder ? pipelineBuilder.buildInfo : null;
        PipelineBuilder.BuildInfo buildInfo2 = default;
        if (buildInfo.HasValue) buildInfo2 = buildInfo.Value;
        if (angleBar)
        {
            if (buildInfo.HasValue)
            {
                angleBar.fillAmount = buildInfo2.angle / buildInfo2.maxAngle;
                if (buildInfo2.angleInvalid)
                {
                    angleBar.color = invalidColor;
                }
                else
                {
                    angleBar.color = validColor;
                }
            }
            else
            {
                angleBar.color = invalidColor;
                angleBar.fillAmount = 1f;
            }
        }
        if (angleText)
        {
            if (buildInfo.HasValue)
            {
                angleText.text = (int)buildInfo2.angle + "/" + (int)buildInfo2.maxAngle;
                if (buildInfo2.angleInvalid)
                {
                    angleText.color = invalidColor;
                }
                else
                {
                    angleText.color = validColor;
                }
            }
            else
            {
                angleText.text = Language.GetString(errorToken);
                angleText.color = invalidColor;
            }
        }
        if (distanceBar)
        {
            if (buildInfo.HasValue)
            {
                distanceBar.fillAmount = buildInfo2.distance / buildInfo2.maxDistance;
                if (buildInfo2.distanceInvalid)
                {
                    distanceBar.color = invalidColor;
                }
                else
                {
                    distanceBar.color = validColor;
                }
            }
            else
            {
                distanceBar.color = invalidColor;
                distanceBar.fillAmount = 1f;
            }
        }
        if (distanceText)
        {
            if (buildInfo.HasValue)
            {
                distanceText.text = (int)buildInfo2.minDistance + "/" + (int)buildInfo2.distance + "/" + (int)buildInfo2.maxDistance;
                if (buildInfo2.distanceInvalid)
                {
                    distanceText.color = invalidColor;
                }
                else
                {
                    distanceText.color = validColor;
                }
            }
            else
            {
                distanceText.text = Language.GetString(errorToken);
                distanceText.color = invalidColor;
            }
        }
        if (collisionText)
        {
            if (buildInfo.HasValue)
            {
                if (buildInfo2.collides)
                {
                    collisionText.text = Language.GetString(collidesInvalidToken);
                    collisionText.color = invalidColor;
                }
                else
                {
                    collisionText.text = Language.GetString(collidesValidToken);
                    collisionText.color = validColor;
                }
            }
            else
            {
                collisionText.text = Language.GetString(errorToken);
                collisionText.color = invalidColor;
            }
        }
    }
}
