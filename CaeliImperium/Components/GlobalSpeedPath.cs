using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components
{
    public class GlobalSpeedPath : MonoBehaviour
    {
        public static float endEffectScale = 1f;
        public static float endLineLengthSmoothTime = 1.5f;
        public ItemBehaviours.DrawSpeedPathBehaviour speedPathDrawerComponent;
        public LineRenderer lineRenderer;
        private float endLineLength;
        private float endLineLengthVelocity;
        private List<Vector3> pathPositions = [];
        private bool disconected;
        private Transform endEffect;
        private Transform startEffect;
        public void AddPath(SpeedPathComponent speedPatchComponent)
        {
            if (pathPositions.Count >= ItemBehaviours.DrawSpeedPathBehaviour.maxPaths)
            {
                pathPositions.RemoveRange(0, 2);
                if (startEffect) startEffect.position = pathPositions[0];
            }
            Vector3 vector3 = speedPatchComponent.transform.position;
            if (pathPositions.Count > 2)
            {
                pathPositions.Add((pathPositions[pathPositions.Count - 1] + vector3) / 2f);
            }
            pathPositions.Add(vector3);
            lineRenderer.positionCount = pathPositions.Count;
            lineRenderer.SetPositions(pathPositions.ToArray());
            lineRenderer.positionCount++;
            lineRenderer.positionCount++;
            endLineLength = 0f;
            if (!startEffect)
            {
                startEffect = Instantiate(CaeliImperiumAssets.SpeedPathEndPrefab, transform).transform;
                startEffect.localScale = new Vector3(endEffectScale, endEffectScale, endEffectScale);
                startEffect.position = pathPositions[0];
            }
            if (!endEffect)
            {
                endEffect = Instantiate(CaeliImperiumAssets.SpeedPathEndPrefab, transform).transform;
                endEffect.localScale = new Vector3(endEffectScale, endEffectScale, endEffectScale);
            }
        }
        public void Disconect()
        {
            if (lineRenderer.positionCount >= 2)
                lineRenderer.positionCount--;
            lineRenderer.positionCount--;
            disconected = true;
            if (endEffect) endEffect.position = pathPositions[pathPositions.Count - 1];
        }
        public void Update()
        {
            if (disconected || !speedPathDrawerComponent || !speedPathDrawerComponent.body || lineRenderer.positionCount <= 2) return;
            endLineLength = Mathf.SmoothDamp(endLineLength, 1f, ref endLineLengthVelocity, endLineLengthSmoothTime / speedPathDrawerComponent.velocity, float.MaxValue, Time.deltaTime);
            Vector3 vector3 = (speedPathDrawerComponent.previousPosition + speedPathDrawerComponent.body.transform.position) / 2f;
            Vector3 vector31 = vector3 - speedPathDrawerComponent.previousPosition;
            vector3 = speedPathDrawerComponent.previousPosition + vector31 * endLineLength;
            if (endEffect) endEffect.position = vector3;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, vector3);
            lineRenderer.SetPosition(lineRenderer.positionCount - 2, (speedPathDrawerComponent.previousPosition + vector3) / 2f);
        }
    }
}
