using CaeliImperium.Items;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace CaeliImperium.Components
{
    public class GlobalSpeedPath : MonoBehaviour
    {
        public static float gradientSmoothTime = 0.2f;
        public static float vectorSmoothTime = 1.5f;
        public static float gradientMaxAlpha = 2f;
        public static float endEffectScale = 1f;
        public static float endLineLengthSmoothTime = 1.5f;
        public ItemBehaviours.DrawSpeedPathBehaviour speedPathDrawerComponent;
        public LineRenderer lineRenderer;
        public Transform endTranform;
        public ParticleSystem endEffect;
        public Transform startTranform;
        public ParticleSystem startEffect;
        public static Dictionary<int, GlobalSpeedPath> instances = [];
        public int id;
        private Vector3 targetVector;
        private Vector3 targetPreviousVector;
        private Vector3 targetVectorVelocity;
        private Vector3 targetPreviousVectorVelocity;
        private float targetGradient;
        private float coefficient;
        private float coefficient2;
        private float gradient;
        private float gradientVelocity;
        private float endLineLength;
        private float endLineLengthVelocity;
        private List<Vector3> pathPositions = [];
        private bool disconected;
        private static event Action<int> OnGlobalSpeedPathDestroyed;
        private int previousIndex;
        private Vector3 oldPosition;
        private Vector3 newPosition;
        private int count;

        public void Awake()
        {
            previousIndex = transform.GetSiblingIndex();
            instances.Add(previousIndex, this);
            OnGlobalSpeedPathDestroyed += GlobalSpeedPath_OnGlobalSpeedPathDestroyed;
            SpeedPathComponent.OnSpeedPathCharged += SpeedPathComponent_OnSpeedPathCharged;
        }

        private void SpeedPathComponent_OnSpeedPathCharged(SpeedPathComponent obj)
        {
            UpdatePath();
        }

        private void GlobalSpeedPath_OnGlobalSpeedPathDestroyed(int index)
        {
            if (index < previousIndex)
            {
                instances.Remove(previousIndex);
                instances.Add(previousIndex - 1, this);
            }
        }

        public void Start()
        {
            if (!speedPathDrawerComponent || !speedPathDrawerComponent.body) return;
            targetVector = (speedPathDrawerComponent.previousPosition + speedPathDrawerComponent.body.transform.position) / 2f;
            targetPreviousVector = targetVector;
            newPosition = speedPathDrawerComponent.body.transform.position;
            oldPosition = newPosition;
        }
        public void OnDestroy()
        {
            instances.Remove(previousIndex);
            OnGlobalSpeedPathDestroyed -= GlobalSpeedPath_OnGlobalSpeedPathDestroyed;
            OnGlobalSpeedPathDestroyed?.Invoke(previousIndex);
            SpeedPathComponent.OnSpeedPathCharged -= SpeedPathComponent_OnSpeedPathCharged;
        }
        public void SetGradientValues(DrawSpeedPathEvents.SpeedPathGradient speedPathGradient, Vector3 position)
        {
            Transform transform = speedPathGradient?.nearestSpeedPath?.transform;
            if (!transform) return;
            targetGradient = speedPathGradient.nearestSpeedPath.transform.GetSiblingIndex() - 3;
            coefficient = Mathf.Lerp(1f, 0f, Mathf.Sqrt((transform.position - position).sqrMagnitude / (DrawSpeedPathEvents.gradientExtraRange * DrawSpeedPathEvents.gradientExtraRange)));
            if (!startTranform) coefficient2 = 0f;
            coefficient2 = Mathf.Lerp(1f, 0f, Mathf.Sqrt((startTranform.position - position).sqrMagnitude / (DrawSpeedPathEvents.gradientExtraRange * DrawSpeedPathEvents.gradientExtraRange)));
        }
        public void UpdateGradient()
        {
            if (!lineRenderer) return;
            float count = transform.childCount - 3;
            if (count <= 0f) return;
            float value = DrawSpeedPathEvents.gradientCoefficient / count / 2f;
            this.gradient = Mathf.SmoothDamp(this.gradient, targetGradient / count, ref gradientVelocity, gradientSmoothTime, float.MaxValue, Time.unscaledDeltaTime);
            GradientAlphaKey[] gradientAlphaKeys = [new GradientAlphaKey(0f, this.gradient - value), new GradientAlphaKey(gradientMaxAlpha * coefficient, this.gradient), new GradientAlphaKey(0f, this.gradient + value)];
            Gradient gradient = lineRenderer.colorGradient;
            gradient.SetKeys(lineRenderer.colorGradient.colorKeys, gradientAlphaKeys);
            Color color;
            if (endEffect)
            {
                color = endEffect.startColor;
                color.a = coefficient;
                endEffect.startColor = color;
            }
            if (startEffect)
            {
                color = startEffect.startColor;
                color.a = coefficient2;
                startEffect.startColor = color;
            }
            lineRenderer.colorGradient = gradient;
        }
        public void UpdateEnd()
        {
            if (disconected || count == 0 || lineRenderer.positionCount <= 2 || !speedPathDrawerComponent || !speedPathDrawerComponent.body) return;
            float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            endLineLength = Mathf.SmoothDamp(endLineLength, 1f, ref endLineLengthVelocity, endLineLengthSmoothTime / speedPathDrawerComponent.velocity, float.MaxValue, Time.unscaledDeltaTime);
            //targetPreviousVector = Vector3.SmoothDamp(targetPreviousVector, speedPathDrawerComponent.previousPosition, ref targetPreviousVectorVelocity, vectorSmoothTime, float.MaxValue, Time.unscaledDeltaTime);
            Vector3 previousPosition = pathPositions[count - 1];
            Vector3 updatePosition = Vector3.Lerp(oldPosition, newPosition, interpolationFactor);
            Vector3 vector3 = (previousPosition + updatePosition) / 2f;
            //Vector3 vector31 = vector3 - previousPosition;
            //vector3 = previousPosition + vector31 * endLineLength;
            targetVector = Vector3.SmoothDamp(targetVector, vector3, ref targetVectorVelocity, vectorSmoothTime / speedPathDrawerComponent.velocity, float.MaxValue, Time.unscaledDeltaTime);
            if (endTranform) endTranform.position = targetVector;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, targetVector);
            lineRenderer.SetPosition(lineRenderer.positionCount - 2, (previousPosition + targetVector) / 2f);
        }
        public void FixedUpdate()
        {
            if (disconected || !speedPathDrawerComponent || !speedPathDrawerComponent.body) return;
            oldPosition = newPosition;
            newPosition = speedPathDrawerComponent.body.transform.position;
        }
        public void Update()
        {
            UpdateGradient();
            UpdateEnd();
        }
        public void UpdatePath()
        {
            pathPositions.Clear();
            for (int i = 3; i < transform.childCount; i++)
            {
                Vector3 vector3 = transform.GetChild(i).position;
                //pathPositions.Add(i == 3 ? vector3 : (transform.GetChild(i - 1).position + vector3) / 2f);
                pathPositions.Add(vector3);
            }
            count = pathPositions.Count;
            //if (pathPositions.Count >= ItemBehaviours.DrawSpeedPathBehaviour.maxPaths)
            //{
            //    pathPositions.RemoveRange(0, 2);
            //    if (startTranform) startTranform.position = pathPositions[0];
            //}
            //Vector3 vector3 = speedPatchComponent.transform.position;
            //Vector3 vector31;
            //if (pathPositions.Count > 2)
            //{
            //    vector31 = (pathPositions[pathPositions.Count - 1] + vector3) / 2f;
            //}
            //else
            //{
            //    vector31 = vector3;
            //}
            //pathPositions.Add(vector31);
            //pathPositions.Add(vector3);
            lineRenderer.positionCount = count;
            lineRenderer.SetPositions(pathPositions.ToArray());
            if (!disconected) lineRenderer.positionCount++;
            //lineRenderer.positionCount++;
            endLineLength = 0f;
            startTranform?.position = pathPositions[0];
        }
        public void Disconect()
        {
            disconected = true;
            UpdatePath();
            if (endTranform) endTranform.position = pathPositions[count - 1];
        }
    }
}
