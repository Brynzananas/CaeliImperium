using CaeliImperium.Items;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using static CaeliImperium.Items.DrawSpeedPathEvents;

namespace CaeliImperium.ItemBehaviours
{
    public class DrawSpeedPath2Behaviour : CharacterBody.ItemBehavior
    {
        private static List<DrawSpeedPath2Behaviour> _instances = [];
        public static ReadOnlyCollection<DrawSpeedPath2Behaviour> readOnlyInstances => _instances.AsReadOnly();

        public List<Vector3> pathPoints = new List<Vector3>();
        public class PointsCluster
        {
            public Vector3 center;
            public int startIndex;
            public int endIndex;
        }
        public List<PointsCluster> pointsClusters = new List<PointsCluster>();
        private LineRenderer _lineRenderer;
        public LineRenderer lineRenderer
        {
            get
            {
                if (!_lineRenderer) _lineRenderer = Instantiate(SpeedPathLine).GetComponent<LineRenderer>();
                return _lineRenderer;
            }
        }
        public Transform startMarker => lineRenderer.transform.Find("ChalkStart");
        public Transform endMarker => lineRenderer.transform.Find("ChalkEnd");
        public TeamComponent teamComponent;

        public void OnEnable()
        {
            _instances.Add(this);
        }
        public void OnDisable()
        {
            _instances.Remove(this);
        }
        public void OnDestroy()
        {
            if (_lineRenderer) Destroy(_lineRenderer.gameObject);
        }
        public void Start()
        {
            teamComponent = GetComponent<TeamComponent>();
        }
        public void FixedUpdate()
        {
            SetPoint(transform.position);
            TrimExcessPathLength();
        }
        public void SetPoint(Vector3 position) => SetPoint(position, true);
        public void SetPoint(Vector3 position, bool smooth)
        {
            if (pathPoints.Count == 0)
            {
                pathPoints.Add(position);
                UpdateStartMarker(position);
                UpdateEndMarker(position);
                CreateNewCluster(position, 0);
            }
            else
            {
                Vector3 endPosition = pathPoints[pathPoints.Count - 1];
                if (Vector3.Distance(endPosition, position) >= SpeedPathMinDistanceBetweenPoints)
                {
                    if (smooth)
                    {
                        position = (endPosition + position) / 2f;
                    }
                    pathPoints.Add(position);
                    int newIndex = pathPoints.Count - 1;
                    PointsCluster lastCluster = pointsClusters[pointsClusters.Count - 1];
                    if (Vector3.Distance(lastCluster.center, position) >= SpeedPathClusterRadius)
                    {
                        CreateNewCluster(position, newIndex);
                    }
                    else
                    {
                        lastCluster.endIndex = newIndex;
                    }

                    UpdateEndMarker(position);
                }
            }
            RefreshLineRendererPositions();
        }
        public void CreateNewCluster(Vector3 position, int index)
        {
            pointsClusters.Add(new PointsCluster
            {
                center = position,
                startIndex = index,
                endIndex = index
            });
        }
        public void TrimExcessPathLength()
        {
            if (pathPoints.Count < 2) return;
            float currentLength = GetTotalPathLength();
            bool pointsRemoved = false;
            float maxLength;
            if (body)
            {
                maxLength = stack.Stack(SpeedPathMaxLength, SpeedPathMaxLengthStack);
            }
            else
            {
                maxLength = SpeedPathMaxLength;
            }
            while (currentLength > maxLength && pathPoints.Count > 2)
            {
                float segmentLength = Vector3.Distance(pathPoints[0], pathPoints[1]);
                pathPoints.RemoveAt(0);
                currentLength -= segmentLength;
                ShiftClusterIndices();
                pointsRemoved = true;
            }
            if (pointsRemoved)
            {
                UpdateStartMarker(pathPoints[0]);
                RefreshLineRendererPositions();
            }
        }
        public void ShiftClusterIndices()
        {
            for (int i = pointsClusters.Count - 1; i >= 0; i--)
            {
                pointsClusters[i].startIndex--;
                pointsClusters[i].endIndex--;
                if (pointsClusters[i].endIndex < 0)
                {
                    pointsClusters.RemoveAt(i);
                }
                else if (pointsClusters[i].startIndex < 0)
                {
                    pointsClusters[i].startIndex = 0;
                }
            }
        }
        public bool IsNearPath(Vector3 position, float distance)
        {
            if (pathPoints.Count < 2) return false;
            float searchRadius = SpeedPathClusterRadius + distance;
            float searchRadiusSqr = searchRadius * searchRadius;
            for (int c = 0; c < pointsClusters.Count; c++)
            {
                PointsCluster pointsCluster = pointsClusters[c];
                if ((position - pointsCluster.center).sqrMagnitude <= searchRadiusSqr)
                {
                    int end = Mathf.Min(pointsCluster.endIndex, pathPoints.Count - 2);
                    for (int i = pointsCluster.startIndex; i <= end; i++)
                    {
                        if (i < 0 || i >= pathPoints.Count - 1) continue;
                        if (DistanceToSegment(position, pathPoints[i], pathPoints[i + 1]) <= distance) return true;
                    }
                }
            }
            return false;
        }
        public bool IsNearPathExcludingEnd(Vector3 position, float distance, float excludeFromStartDistance, float excludeFromEndDistance)
        {
            if (pathPoints.Count < 2) return false;
            float totalPathLength = GetTotalPathLength();
            if (excludeFromStartDistance + excludeFromEndDistance >= totalPathLength) return false;
            float coarseSearchRadiusSqr = Mathf.Pow(SpeedPathClusterRadius + distance, 2);
            float endCutoff = totalPathLength - excludeFromEndDistance;
            float[] pointCumulativeLengths = GetPointCumulativeLengths();
            for (int c = 0; c < pointsClusters.Count; c++)
            {
                PointsCluster pointsCluster = pointsClusters[c];
                if ((position - pointsCluster.center).sqrMagnitude > coarseSearchRadiusSqr) continue;
                int end = Mathf.Min(pointsCluster.endIndex, pathPoints.Count - 2);
                for (int i = pointsCluster.startIndex; i <= end; i++)
                {
                    if (i < 0 || i >= pathPoints.Count - 1) continue;
                    float segmentStartDist = pointCumulativeLengths[i];
                    float segmentEndDist = pointCumulativeLengths[i + 1];
                    float segmentLen = segmentEndDist - segmentStartDist;
                    if (segmentEndDist <= excludeFromStartDistance) continue;
                    if (segmentStartDist >= endCutoff) continue;
                    Vector3 validStart = pathPoints[i];
                    Vector3 validEnd = pathPoints[i + 1];
                    if (segmentStartDist < excludeFromStartDistance && segmentLen > 0f)
                    {
                        float t = (excludeFromStartDistance - segmentStartDist) / segmentLen;
                        validStart = Vector3.Lerp(pathPoints[i], pathPoints[i + 1], t);
                    }
                    if (segmentEndDist > endCutoff && segmentLen > 0f)
                    {
                        float t = (endCutoff - segmentStartDist) / segmentLen;
                        validEnd = Vector3.Lerp(pathPoints[i], pathPoints[i + 1], t);
                    }
                    if (DistanceToSegment(position, validStart, validEnd) <= distance) return true;
                }
            }
            return false;
        }
        public float[] GetPointCumulativeLengths()
        {
            float[] lengths = new float[pathPoints.Count];
            lengths[0] = 0f;
            for (int i = 0; i < pathPoints.Count - 1; i++) lengths[i + 1] = lengths[i] + Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
            return lengths;
        }
        public float GetTotalPathLength()
        {
            float total = 0f;
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                total += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
            }
            return total;
        }
        public void UpdateStartMarker(Vector3 position)
        {
            startMarker.transform.position = position;
        }

        public void UpdateEndMarker(Vector3 position)
        {
            endMarker.transform.position = position;
        }

        public void RefreshLineRendererPositions()
        {
            lineRenderer.positionCount = pathPoints.Count;
            lineRenderer.SetPositions(pathPoints.ToArray());
        }
        public void UpdateLineGradient(Vector3 position)
        {
            int pointCount = lineRenderer.positionCount;
            if (pointCount < 2) return;
            int keyCount = Mathf.Min(pointCount, 8);
            GradientColorKey[] colorKeys = new GradientColorKey[keyCount];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[keyCount];
            float totalLength = 0f;
            float[] cumulativeLengths = new float[pointCount];
            cumulativeLengths[0] = 0f;
            for (int i = 1; i < pointCount; i++)
            {
                totalLength += Vector3.Distance(lineRenderer.GetPosition(i - 1), lineRenderer.GetPosition(i));
                cumulativeLengths[i] = totalLength;
            }
            for (int i = 0; i < keyCount; i++)
            {
                int pointIndex = Mathf.RoundToInt((float)i / (keyCount - 1) * (pointCount - 1));
                Vector3 pointPos = lineRenderer.GetPosition(pointIndex);
                float time = totalLength > 0 ? cumulativeLengths[pointIndex] / totalLength : (float)i / (keyCount - 1);
                float dist = Vector3.Distance(pointPos, position);
                float t = Mathf.Clamp01((dist - SpeedPathRenderDistance) / Mathf.Max(SpeedPathFadeDistance, 0.001f));
                Color currentColor = Color.Lerp(new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 0f), t);
                colorKeys[i] = new GradientColorKey(currentColor, time);
                alphaKeys[i] = new GradientAlphaKey(currentColor.a, time);
            }
            Gradient gradient = new Gradient();
            gradient.SetKeys(colorKeys, alphaKeys);
            lineRenderer.colorGradient = gradient;
        }
        public float DistanceToSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            Vector3 ap = p - a;
            float sqrLen = ab.sqrMagnitude;
            if (sqrLen == 0f) return Vector3.Distance(p, a);
            float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / sqrLen);
            Vector3 projection = a + t * ab;
            return Vector3.Distance(p, projection);
        }
        public bool TeamCheck(CharacterBody characterBody)
        {
            if (characterBody.teamComponent && teamComponent && TeamManager.IsTeamEnemy(characterBody.teamComponent.teamIndex, teamComponent.teamIndex)) return false;
            return true;
        }
    }
}
