using BrynzaAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CaeliImperium.Components;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PipeMeshGenerator : MonoBehaviour
{
    public float pipeRadius = 0.4f;
    public int radialSegments = 12;
    public int curveSegments = 20;
    public bool generateCaps = true;
    public MeshFilter meshFilter;
    private Mesh mesh;
    public void Awake()
    {
        if (!meshFilter) meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh { name = "PipeMesh" };
        meshFilter.mesh = mesh;
    }
    public void OnDestroy()
    {
        Destroy(mesh);
    }
    public void GeneratePipe(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        if (!mesh) return;
        mesh.Clear();
        int ringVertexCount = radialSegments + 1;
        int numRings = curveSegments + 1;
        int bodyVertexCount = ringVertexCount * numRings;
        int capVertexCount = generateCaps ? (radialSegments + 1) * 2 : 0;
        Vector3[] vertices = new Vector3[bodyVertexCount + capVertexCount];
        Vector3[] normals = new Vector3[bodyVertexCount + capVertexCount];
        Vector2[] uvs = new Vector2[bodyVertexCount + capVertexCount];
        Vector3 lastTangent = CaeliImperiumUtils.BezierGetFirstDerivative(p0, p1, p2, p3, 0f);
        Vector3 normal = Vector3.Cross(lastTangent, Vector3.up);
        if (normal.sqrMagnitude < 0.001f) normal = Vector3.Cross(lastTangent, Vector3.right);
        normal.Normalize();
        int vertIdx = 0;
        for (int i = 0; i < numRings; i++)
        {
            float t = (float)i / curveSegments;
            Vector3 center = CaeliImperiumUtils.BezierGetPoint(p0, p1, p2, p3, t);
            Vector3 tangent = CaeliImperiumUtils.BezierGetFirstDerivative(p0, p1, p2, p3, t);
            Quaternion rotDelta = Quaternion.FromToRotation(lastTangent, tangent);
            normal = rotDelta * normal;
            normal = (normal - Vector3.Project(normal, tangent)).normalized;
            Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
            lastTangent = tangent;
            for (int j = 0; j < ringVertexCount; j++)
            {
                float radAngle = ((float)j / radialSegments) * Mathf.PI * 2f;
                float cos = Mathf.Cos(radAngle);
                float sin = Mathf.Sin(radAngle);
                Vector3 localDir = (cos * normal + sin * binormal).normalized;
                vertices[vertIdx] = center + localDir * pipeRadius;
                normals[vertIdx] = localDir;
                uvs[vertIdx] = new Vector2((float)j / radialSegments, t);
                vertIdx++;
            }
        }
        int bodyTriangleCount = curveSegments * radialSegments * 6;
        int capTriangleCount = generateCaps ? radialSegments * 6 : 0;
        int[] triangles = new int[bodyTriangleCount + capTriangleCount];
        int triIdx = 0;
        for (int ring = 0; ring < curveSegments; ring++)
        {
            for (int seg = 0; seg < radialSegments; seg++)
            {
                int current = ring * ringVertexCount + seg;
                int next = current + ringVertexCount;

                triangles[triIdx++] = current;
                triangles[triIdx++] = next;
                triangles[triIdx++] = current + 1;

                triangles[triIdx++] = current + 1;
                triangles[triIdx++] = next;
                triangles[triIdx++] = next + 1;
            }
        }
        if (generateCaps)
        {
            int startCapOffset = bodyVertexCount;
            Vector3 startTangent = CaeliImperiumUtils.BezierGetPoint(p0, p1, p2, p3, 0f);
            Vector3 startCenter = CaeliImperiumUtils.BezierGetFirstDerivative(p0, p1, p2, p3, 0f);
            for (int j = 0; j < ringVertexCount; j++)
            {
                vertices[startCapOffset + j] = vertices[j];
                normals[startCapOffset + j] = -startTangent;
                uvs[startCapOffset + j] = uvs[j];
            }
            int endCapOffset = bodyVertexCount + ringVertexCount;
            Vector3 endTangent = CaeliImperiumUtils.BezierGetFirstDerivative(p0, p1, p2, p3, 1f);
            int lastRingOffset = (numRings - 1) * ringVertexCount;
            for (int j = 0; j < ringVertexCount; j++)
            {
                vertices[endCapOffset + j] = vertices[lastRingOffset + j];
                normals[endCapOffset + j] = endTangent;
                uvs[endCapOffset + j] = uvs[lastRingOffset + j];
            }
            for (int seg = 0; seg < radialSegments; seg++)
            {
                triangles[triIdx++] = startCapOffset;
                triangles[triIdx++] = startCapOffset + seg + 1;
                triangles[triIdx++] = startCapOffset + seg;
                triangles[triIdx++] = endCapOffset;
                triangles[triIdx++] = endCapOffset + seg;
                triangles[triIdx++] = endCapOffset + seg + 1;
            }
        }
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }
}
