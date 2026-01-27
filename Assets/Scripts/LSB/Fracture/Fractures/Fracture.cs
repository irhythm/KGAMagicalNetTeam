using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메쉬 파괴(Fracture) 작업을 수행하는 클래스
/// </summary>
public static class Fracture
{
    private static readonly Collider[] OverlapBuffer = new Collider[64];

    public static ChunkGraphManager FractureGameObject(GameObject gameObject, Anchor anchor, int seed, int totalChunks, Material insideMaterial, Material outsideMaterial, float jointBreakForce, float density, float debrisLifetime)
    {
        var mesh = GetWorldMesh(gameObject);

        NvBlastExtUnity.setSeed(seed);

        var nvMesh = new NvMesh(
            mesh.vertices,
            mesh.normals,
            mesh.uv,
            mesh.vertexCount,
            mesh.GetIndices(0),
            (int)mesh.GetIndexCount(0)
        );

        var meshes = FractureMeshesInNvblast(totalChunks, nvMesh);
        var chunkMass = mesh.Volume() * density / totalChunks;

        var chunks = BuildChunks(insideMaterial, outsideMaterial, meshes, chunkMass, debrisLifetime);

        int count = chunks.Count;
        for (int i = 0; i < count; i++)
        {
            RegisterNeighbours(chunks[i], 0.005f, jointBreakForce);
        }

        AnchorChunks(gameObject, anchor);

        var fractureGameObject = new GameObject("Fracture");
        Transform fractureTransform = fractureGameObject.transform;

        fractureGameObject.transform.position = gameObject.transform.position;
        fractureGameObject.transform.rotation = gameObject.transform.rotation;
        fractureGameObject.transform.localScale = gameObject.transform.localScale;

        for (int i = 0; i < count; i++)
        {
            chunks[i].transform.SetParent(fractureTransform, true);
        }

        var graphManager = fractureGameObject.AddComponent<ChunkGraphManager>();
        graphManager.Setup(fractureGameObject.GetComponentsInChildren<ChunkNode>());

        return graphManager;
    }

    private static void AnchorChunks(GameObject gameObject, Anchor anchor)
    {
        var transform = gameObject.transform;
        var bounds = gameObject.GetCompositeMeshBounds(includeInactive: false, isSharedMesh: true);
        var anchoredColliders = GetAnchoredColliders(anchor, transform, bounds);

        foreach (var collider in anchoredColliders)
        {
            if (collider.TryGetComponent(out ChunkNode node))
            {
                node.IsIndestructible = true;
            }
        }
    }

    private static List<GameObject> BuildChunks(Material insideMaterial, Material outsideMaterial, List<Mesh> meshes, float chunkMass, float debrisLifetime)
    {
        var list = new List<GameObject>(meshes.Count);
        for (int i = 0; i < meshes.Count; i++)
        {
            var chunk = BuildChunk(insideMaterial, outsideMaterial, meshes[i], chunkMass, debrisLifetime);
            chunk.name += $" [{i}]";
            list.Add(chunk);
        }
        return list;
    }

    private static List<Mesh> FractureMeshesInNvblast(int totalChunks, NvMesh nvMesh)
    {
        var fractureTool = new NvFractureTool();
        fractureTool.setRemoveIslands(false);
        fractureTool.setSourceMesh(nvMesh);

        var sites = new NvVoronoiSitesGenerator(nvMesh);
        sites.uniformlyGenerateSitesInMesh(totalChunks);

        fractureTool.voronoiFracturing(0, sites);
        fractureTool.finalizeFracturing();

        var meshCount = fractureTool.getChunkCount();
        var meshes = new List<Mesh>(meshCount);
        for (var i = 1; i < meshCount; i++)
        {
            meshes.Add(ExtractChunkMesh(fractureTool, i));
        }

        return meshes;
    }

    private static HashSet<Collider> GetAnchoredColliders(Anchor anchor, Transform meshTransform, Bounds bounds)
    {
        var anchoredChunks = new HashSet<Collider>();
        var frameWidth = .01f;
        var meshWorldCenter = meshTransform.TransformPoint(bounds.center);
        var meshWorldExtents = Vector3.Scale(bounds.extents, meshTransform.lossyScale);

        void CheckAndAdd(Vector3 direction, Vector3 halfExtentsMod)
        {
            var center = meshWorldCenter + direction;
            var colliders = Physics.OverlapBox(center, halfExtentsMod, meshTransform.rotation);
            for (int i = 0; i < colliders.Length; i++)
            {
                anchoredChunks.Add(colliders[i]);
            }
        }

        if (anchor.HasFlag(Anchor.Left)) { var h = AbsVec3(meshWorldExtents); h.x = frameWidth; CheckAndAdd(-meshTransform.right * meshWorldExtents.x, h); }
        if (anchor.HasFlag(Anchor.Right)) { var h = AbsVec3(meshWorldExtents); h.x = frameWidth; CheckAndAdd(meshTransform.right * meshWorldExtents.x, h); }
        if (anchor.HasFlag(Anchor.Bottom)) { var h = AbsVec3(meshWorldExtents); h.y = frameWidth; CheckAndAdd(-meshTransform.up * meshWorldExtents.y, h); }
        if (anchor.HasFlag(Anchor.Top)) { var h = AbsVec3(meshWorldExtents); h.y = frameWidth; CheckAndAdd(meshTransform.up * meshWorldExtents.y, h); }
        if (anchor.HasFlag(Anchor.Front)) { var h = AbsVec3(meshWorldExtents); h.z = frameWidth; CheckAndAdd(-meshTransform.forward * meshWorldExtents.z, h); }
        if (anchor.HasFlag(Anchor.Back)) { var h = AbsVec3(meshWorldExtents); h.z = frameWidth; CheckAndAdd(meshTransform.forward * meshWorldExtents.z, h); }

        return anchoredChunks;
    }

    private static Vector3 AbsVec3(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

    private static Mesh ExtractChunkMesh(NvFractureTool fractureTool, int index)
    {
        var outside = fractureTool.getChunkMesh(index, false);
        var inside = fractureTool.getChunkMesh(index, true);

        var chunkMesh = outside.toUnityMesh();
        chunkMesh.subMeshCount = 2;
        chunkMesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);
        return chunkMesh;
    }

    private static Mesh GetWorldMesh(GameObject gameObject)
    {
        var meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        var combineList = new List<CombineInstance>();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (ValidateMesh(meshFilters[i].sharedMesh))
            {
                combineList.Add(new CombineInstance()
                {
                    mesh = meshFilters[i].sharedMesh,
                    transform = meshFilters[i].transform.localToWorldMatrix
                });
            }
        }

        var totalMesh = new Mesh();
        totalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        totalMesh.CombineMeshes(combineList.ToArray(), true);
        return totalMesh;
    }

    private static bool ValidateMesh(Mesh mesh)
    {
        if (mesh == null) return false;
        if (!mesh.isReadable) { Debug.LogError($"Mesh [{mesh.name}] is not readable."); return false; }
        if (mesh.vertexCount == 0) { Debug.LogError($"Mesh [{mesh.name}] has 0 vertices."); return false; }
        if (mesh.uv.Length == 0) { Debug.LogError($"Mesh [{mesh.name}] has no UVs."); return false; }
        return true;
    }

    private static GameObject BuildChunk(Material insideMaterial, Material outsideMaterial, Mesh mesh, float mass, float debrisLifetime)
    {
        mesh.RecalculateBounds();
        Vector3 meshCenter = mesh.bounds.center;

        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= meshCenter;
        }
        mesh.vertices = vertices;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        var chunk = new GameObject("Chunk");

        chunk.transform.position = meshCenter;
        chunk.transform.rotation = Quaternion.identity;

        var renderer = chunk.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = new[] { outsideMaterial, insideMaterial };

        var meshFilter = chunk.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        var chunkNode = chunk.AddComponent<ChunkNode>();
        chunkNode.DebrisLifetime = debrisLifetime;

        var rigibody = chunk.AddComponent<Rigidbody>();
        rigibody.mass = mass;
        rigibody.isKinematic = true;

        var mc = chunk.AddComponent<MeshCollider>();
        mc.convex = true;
        mc.sharedMesh = mesh;

        return chunk;
    }

    private static void RegisterNeighbours(GameObject chunk, float touchRadius, float breakForce)
    {
        if (!chunk.TryGetComponent(out ChunkNode myNode)) return;

        myNode.BreakForce = breakForce;

        if (!chunk.TryGetComponent(out MeshCollider mc)) return;

        var bounds = mc.bounds;
        Vector3 center = bounds.center;
        Vector3 halfExtents = bounds.extents + new Vector3(touchRadius, touchRadius, touchRadius);

        int hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, OverlapBuffer, chunk.transform.rotation, -1, QueryTriggerInteraction.Ignore);

        for (int j = 0; j < hitCount; j++)
        {
            var col = OverlapBuffer[j];
            if (col.attachedRigidbody != null && col.gameObject != chunk)
            {
                if (col.TryGetComponent(out ChunkNode otherNode))
                {
                    myNode.AddNeighbour(otherNode);
                    otherNode.AddNeighbour(myNode);
                }
            }
        }
    }
}