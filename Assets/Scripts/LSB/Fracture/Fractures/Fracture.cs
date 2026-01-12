using System.Collections.Generic;
using UnityEngine;

public static class Fracture
{
    private static readonly Collider[] OverlapBuffer = new Collider[64];

    public static ChunkGraphManager FractureGameObject(GameObject gameObject, Anchor anchor, int seed, int totalChunks, Material insideMaterial, Material outsideMaterial, float jointBreakForce, float density)
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

        // 1. NvBlast로 메쉬 분할
        var meshes = FractureMeshesInNvblast(totalChunks, nvMesh);

        // 2. 청크 GameObject 생성
        var chunkMass = mesh.Volume() * density / totalChunks;
        var chunks = BuildChunks(insideMaterial, outsideMaterial, meshes, chunkMass);

        // 3. [최적화 핵심] Joint 대신 이웃 관계만 등록 (O(N) 근사)
        int count = chunks.Count;
        for (int i = 0; i < count; i++)
        {
            RegisterNeighbours(chunks[i], 0.005f, jointBreakForce);
        }

        // 4. 앵커 설정 (Kinematic 고정)
        AnchorChunks(gameObject, anchor);

        // 5. 부모 설정 및 정리
        var fractureGameObject = new GameObject("Fracture");
        Transform fractureTransform = fractureGameObject.transform;

        // 원본 위치/회전 복사 중요
        fractureGameObject.transform.position = gameObject.transform.position;
        fractureGameObject.transform.rotation = gameObject.transform.rotation;
        fractureGameObject.transform.localScale = gameObject.transform.localScale;

        for (int i = 0; i < count; i++)
        {
            chunks[i].transform.SetParent(fractureTransform, true); // worldPositionStays = true
        }

        var graphManager = fractureGameObject.AddComponent<ChunkGraphManager>();
        graphManager.Setup(fractureGameObject.GetComponentsInChildren<Rigidbody>());

        return graphManager;
    }

    private static void AnchorChunks(GameObject gameObject, Anchor anchor)
    {
        var transform = gameObject.transform;
        var bounds = gameObject.GetCompositeMeshBounds();
        var anchoredColliders = GetAnchoredColliders(anchor, transform, bounds);

        foreach (var collider in anchoredColliders)
        {
            if (collider.TryGetComponent(out ChunkNode node))
            {
                // [수정] RigidBody를 직접 건드리는 대신 플래그 설정
                node.IsIndestructible = true;
            }
        }
    }

    private static List<GameObject> BuildChunks(Material insideMaterial, Material outsideMaterial, List<Mesh> meshes, float chunkMass)
    {
        // LINQ Select 제거 -> List 직접 할당
        var list = new List<GameObject>(meshes.Count);
        for (int i = 0; i < meshes.Count; i++)
        {
            var chunk = BuildChunk(insideMaterial, outsideMaterial, meshes[i], chunkMass);
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

        // 로컬 함수로 중복 코드 제거 및 최적화
        void CheckAndAdd(Vector3 direction, Vector3 halfExtentsMod)
        {
            var center = meshWorldCenter + direction;
            // OverlapBoxNonAlloc을 쓰면 더 좋지만, HashSet에 넣어야 하므로 반환값 사용
            // 하지만 여기서는 횟수가 적으므로 일반 OverlapBox 사용 (단, 결과 병합 방식 개선)
            var colliders = Physics.OverlapBox(center, halfExtentsMod, meshTransform.rotation);
            for (int i = 0; i < colliders.Length; i++)
            {
                anchoredChunks.Add(colliders[i]);
            }
        }

        if (anchor.HasFlag(Anchor.Left))
        {
            var halfExtents = AbsVec3(meshWorldExtents);
            halfExtents.x = frameWidth;
            CheckAndAdd(-meshTransform.right * meshWorldExtents.x, halfExtents);
        }
        if (anchor.HasFlag(Anchor.Right))
        {
            var halfExtents = AbsVec3(meshWorldExtents);
            halfExtents.x = frameWidth;
            CheckAndAdd(meshTransform.right * meshWorldExtents.x, halfExtents);
        }
        if (anchor.HasFlag(Anchor.Bottom))
        {
            var halfExtents = AbsVec3(meshWorldExtents);
            halfExtents.y = frameWidth;
            CheckAndAdd(-meshTransform.up * meshWorldExtents.y, halfExtents);
        }
        if (anchor.HasFlag(Anchor.Top))
        {
            var halfExtents = AbsVec3(meshWorldExtents);
            halfExtents.y = frameWidth;
            CheckAndAdd(meshTransform.up * meshWorldExtents.y, halfExtents);
        }
        if (anchor.HasFlag(Anchor.Front))
        {
            var halfExtents = AbsVec3(meshWorldExtents);
            halfExtents.z = frameWidth;
            CheckAndAdd(-meshTransform.forward * meshWorldExtents.z, halfExtents);
        }
        if (anchor.HasFlag(Anchor.Back))
        {
            var halfExtents = AbsVec3(meshWorldExtents);
            halfExtents.z = frameWidth;
            CheckAndAdd(meshTransform.forward * meshWorldExtents.z, halfExtents);
        }

        return anchoredChunks;
    }

    // Vector3 Abs 확장 메서드 대체용 (유틸성)
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
        // LINQ 제거 및 CombineInstance 배열 최적화
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
        // 32비트 인덱스 포맷 활성화 (버텍스 많은 경우 대비)
        totalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        totalMesh.CombineMeshes(combineList.ToArray(), true);
        return totalMesh;
    }

    private static bool ValidateMesh(Mesh mesh)
    {
        if (mesh == null) return false;
        if (!mesh.isReadable)
        {
            Debug.LogError($"Mesh [{mesh.name}] has to be readable.");
            return false;
        }
        if (mesh.vertexCount == 0)
        {
            Debug.LogError($"Mesh [{mesh.name}] does not have any vertices.");
            return false;
        }
        if (mesh.uv.Length == 0)
        {
            Debug.LogError($"Mesh [{mesh.name}] does not have any uvs.");
            return false;
        }
        return true;
    }

    private static GameObject BuildChunk(Material insideMaterial, Material outsideMaterial, Mesh mesh, float mass)
    {
        var chunk = new GameObject("Chunk");

        var renderer = chunk.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = new[] { outsideMaterial, insideMaterial };

        var meshFilter = chunk.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        var chunkNode = chunk.AddComponent<ChunkNode>(); // ChunkNode 미리 추가

        var rigibody = chunk.AddComponent<Rigidbody>();
        rigibody.mass = mass;
        rigibody.isKinematic = true; // [중요] 생성 시점에는 일단 모두 멈춰둠

        var mc = chunk.AddComponent<MeshCollider>();
        mc.convex = true;
        mc.sharedMesh = mesh;

        return chunk;
    }

    private static void RegisterNeighbours(GameObject chunk, float touchRadius, float breakForce)
    {
        if (!chunk.TryGetComponent(out ChunkNode myNode)) return;

        // 파괴 힘 설정
        myNode.BreakForce = breakForce;

        if (!chunk.TryGetComponent(out MeshCollider mc)) return;

        var bounds = mc.bounds;
        Vector3 center = bounds.center;
        Vector3 halfExtents = bounds.extents + new Vector3(touchRadius, touchRadius, touchRadius);

        // OverlapBoxNonAlloc으로 인접한 청크 검색
        int hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, OverlapBuffer, chunk.transform.rotation, -1, QueryTriggerInteraction.Ignore);

        for (int j = 0; j < hitCount; j++)
        {
            var col = OverlapBuffer[j];
            // 나 자신이 아니고, 리지드바디가 있다면
            if (col.attachedRigidbody != null && col.gameObject != chunk)
            {
                if (col.TryGetComponent(out ChunkNode otherNode))
                {
                    // 서로 이웃으로 등록 (양방향)
                    myNode.AddNeighbour(otherNode);
                    otherNode.AddNeighbour(myNode);
                }
            }
        }
    }
}