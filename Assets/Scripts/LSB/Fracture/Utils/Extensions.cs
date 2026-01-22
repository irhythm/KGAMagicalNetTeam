using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 벡터 연산, 메쉬 부피 계산, 바운드 박스 계산 등 수학적 유틸리티를 제공하는 확장 클래스
/// </summary>
public static class Extensions
{
    // --- 기본 확장 메서드 ---

    public static Color SetAlpha(this Color color, float value)
    {
        return new Color(color.r, color.g, color.b, value);
    }

    public static ISet<T> ToSet<T>(this IEnumerable<T> iEnumerable)
    {
        return new HashSet<T>(iEnumerable);
    }

    public static T GetOrAddComponent<T>(this Component c) where T : Component
    {
        return c.gameObject.GetOrAddComponent<T>();
    }

    public static Component GetOrAddComponent(this GameObject go, Type componentType)
    {
        var result = go.GetComponent(componentType);
        return result == null ? go.AddComponent(componentType) : result;
    }

    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        return GetOrAddComponent(go, typeof(T)) as T;
    }

    // --- Vector3 확장 메서드 ---

    public static Vector3 SetX(this Vector3 vector3, float x) => new Vector3(x, vector3.y, vector3.z);
    public static Vector3 SetY(this Vector3 vector3, float y) => new Vector3(vector3.x, y, vector3.z);
    public static Vector3 SetZ(this Vector3 vector3, float z) => new Vector3(vector3.x, vector3.y, z);

    public static Vector3 Multiply(this Vector3 vectorA, Vector3 vectorB)
    {
        return Vector3.Scale(vectorA, vectorB);
    }

    public static Vector3 Abs(this Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }

    // --- Mesh 부피(Volume) 계산 ---

    private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    public static float Volume(this Mesh mesh)
    {
        float volume = 0;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            var p1 = vertices[triangles[i + 0]];
            var p2 = vertices[triangles[i + 1]];
            var p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        return Mathf.Abs(volume);
    }

    // --- Bounds(경계) 계산 및 변환 ---

    public static Vector3[] GetVertices(this Bounds bounds) => new[]
    {
        bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z),
        bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z),
        bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z),
        bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z),
        bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z),
        bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z),
        bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z),
        bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z),
    };

    public static Vector3 Min(this Vector3 vectorA, Vector3 vectorB) => Vector3.Min(vectorA, vectorB);
    public static Vector3 Max(this Vector3 vectorA, Vector3 vectorB) => Vector3.Max(vectorA, vectorB);

    public static Bounds ToBounds(this Vector3[] vertices)
    {
        var min = Vector3.one * float.MaxValue;
        var max = Vector3.one * float.MinValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            min = vertices[i].Min(min);
            max = vertices[i].Max(max);
        }

        return new Bounds((max - min) / 2 + min, max - min);
    }

    public static Bounds ToBounds(this IEnumerable<Vector3> vertices)
    {
        return vertices.ToArray().ToBounds();
    }

    public static Vector3 TransformPoint(this Transform t, Vector3 position, Transform dest)
    {
        var world = t.TransformPoint(position);
        return dest.InverseTransformPoint(world);
    }

    public static Bounds TransformBounds(this Transform from, Transform to, Bounds bounds)
    {
        return bounds.GetVertices()
            .Select(bv => from.transform.TransformPoint(bv, to.transform))
            .ToBounds();
    }

    // 프리팹 베이킹 오류 해결을 위해 isSharedMesh 파라미터가 추가된 바운드 계산
    public static Bounds GetCompositeMeshBounds(this GameObject go, bool includeInactive = false, bool isSharedMesh = false)
    {
        var meshFilters = go.GetComponentsInChildren<MeshFilter>(includeInactive);
        if (meshFilters.Length == 0) return new Bounds();

        var boundsList = new List<Bounds>();

        foreach (var mf in meshFilters)
        {
            var mesh = isSharedMesh ? mf.sharedMesh : mf.mesh;

            if (mesh == null) continue;

            var localBound = mf.transform.TransformBounds(go.transform, mesh.bounds);
            if (localBound.size != Vector3.zero)
            {
                boundsList.Add(localBound);
            }
        }

        if (boundsList.Count == 0) return new Bounds();

        var compositeBounds = boundsList[0];
        for (int i = 1; i < boundsList.Count; i++)
        {
            compositeBounds.Encapsulate(boundsList[i]);
        }

        return compositeBounds;
    }
}