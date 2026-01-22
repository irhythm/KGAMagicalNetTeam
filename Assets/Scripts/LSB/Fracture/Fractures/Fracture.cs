using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메쉬 파괴(Fracture) 작업을 수행하는 클래스
/// NvBlast 라이브러리를 이용해 메쉬를 Voronoi 패턴으로 분할하고, 
/// 분할된 파편들을 GameObject로 변환하여 조립
/// </summary>
public static class Fracture
{
    // Physics.OverlapBoxNonAlloc에서 사용할 버퍼 (메모리 할당 최적화용)
    private static readonly Collider[] OverlapBuffer = new Collider[64];

    /// <summary>
    /// 대상 게임 오브젝트를 파괴하여 파편들의 집합체로 변환합니다.
    /// </summary>
    public static ChunkGraphManager FractureGameObject(GameObject gameObject, Anchor anchor, int seed, int totalChunks, Material insideMaterial, Material outsideMaterial, float jointBreakForce, float density)
    {
        // 원본 오브젝트의 통합 메쉬 데이터를 가져옵니다.
        var mesh = GetWorldMesh(gameObject);

        // NvBlast 시드 설정
        NvBlastExtUnity.setSeed(seed);

        // Unity Mesh를 NvMesh(NvBlast용 데이터) 변환
        var nvMesh = new NvMesh(
            mesh.vertices,
            mesh.normals,
            mesh.uv,
            mesh.vertexCount,
            mesh.GetIndices(0),
            (int)mesh.GetIndexCount(0)
        );

        // 메쉬 분할 수행
        var meshes = FractureMeshesInNvblast(totalChunks, nvMesh);

        // 각 파편의 질량 계산 (전체 부피 * 밀도 / 파편 수)
        var chunkMass = mesh.Volume() * density / totalChunks;

        // 분할된 메쉬 데이터로 실제 GameObject(Chunk) 생성
        var chunks = BuildChunks(insideMaterial, outsideMaterial, meshes, chunkMass);

        // 파편들 간의 이웃 관계(Neighbours) 등록
        int count = chunks.Count;
        for (int i = 0; i < count; i++)
        {
            RegisterNeighbours(chunks[i], 0.005f, jointBreakForce);
        }

        // 설정된 앵커 위치의 파편들 고정 처리
        AnchorChunks(gameObject, anchor);

        // 파편들을 담을 부모 오브젝트 생성
        var fractureGameObject = new GameObject("Fracture");
        Transform fractureTransform = fractureGameObject.transform;

        // 원본과 동일한 Transform 설정
        fractureGameObject.transform.position = gameObject.transform.position;
        fractureGameObject.transform.rotation = gameObject.transform.rotation;
        fractureGameObject.transform.localScale = gameObject.transform.localScale;

        // 파편들을 부모 하위로 이동
        for (int i = 0; i < count; i++)
        {
            chunks[i].transform.SetParent(fractureTransform, true);
        }

        // ChunkGraphManager 추가 및 초기화
        var graphManager = fractureGameObject.AddComponent<ChunkGraphManager>();
        graphManager.Setup(fractureGameObject.GetComponentsInChildren<ChunkNode>());

        return graphManager;
    }

    /// <summary>
    /// 지정된 앵커 면에 위치한 파편들을 찾아 고정시키는 메서드
    /// </summary>
    private static void AnchorChunks(GameObject gameObject, Anchor anchor)
    {
        var transform = gameObject.transform;

        // 베이킹 시 오류 방지를 위해 isSharedMesh=true 옵션 사용
        var bounds = gameObject.GetCompositeMeshBounds(includeInactive: false, isSharedMesh: true);

        // 앵커 영역에 겹치는 콜라이더 검색
        var anchoredColliders = GetAnchoredColliders(anchor, transform, bounds);

        foreach (var collider in anchoredColliders)
        {
            if (collider.TryGetComponent(out ChunkNode node))
            {
                // 해당 노드를 앵커(무적) 상태로 설정
                node.IsIndestructible = true;
            }
        }
    }

    /// <summary>
    /// 메쉬 리스트를 기반으로 실제 GameObject를 생성하는 메서드
    /// </summary>
    private static List<GameObject> BuildChunks(Material insideMaterial, Material outsideMaterial, List<Mesh> meshes, float chunkMass)
    {
        var list = new List<GameObject>(meshes.Count);
        for (int i = 0; i < meshes.Count; i++)
        {
            var chunk = BuildChunk(insideMaterial, outsideMaterial, meshes[i], chunkMass);
            chunk.name += $" [{i}]";
            list.Add(chunk);
        }
        return list;
    }

    /// <summary>
    /// NvBlast를 사용하여 Voronoi 패턴으로 메쉬를 분할하는 메서드
    /// </summary>
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
        // index 0은 원본일 가능성이 있어 1부터 추출
        for (var i = 1; i < meshCount; i++)
        {
            meshes.Add(ExtractChunkMesh(fractureTool, i));
        }

        return meshes;
    }

    /// <summary>
    /// 앵커 방향의 경계면을 Physics.OverlapBox로 검사하는 메서드
    /// </summary>
    private static HashSet<Collider> GetAnchoredColliders(Anchor anchor, Transform meshTransform, Bounds bounds)
    {
        var anchoredChunks = new HashSet<Collider>();
        var frameWidth = .01f; // 감지할 두께
        var meshWorldCenter = meshTransform.TransformPoint(bounds.center);
        var meshWorldExtents = Vector3.Scale(bounds.extents, meshTransform.lossyScale);

        // 특정 방향 검사 헬퍼 함수
        void CheckAndAdd(Vector3 direction, Vector3 halfExtentsMod)
        {
            var center = meshWorldCenter + direction;
            var colliders = Physics.OverlapBox(center, halfExtentsMod, meshTransform.rotation);
            for (int i = 0; i < colliders.Length; i++)
            {
                anchoredChunks.Add(colliders[i]);
            }
        }

        // 각 플래그별로 검사 수행
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

    private static Vector3 AbsVec3(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

    /// <summary>
    /// NvBlast 데이터를 Unity Mesh로 변환
    /// </summary>
    private static Mesh ExtractChunkMesh(NvFractureTool fractureTool, int index)
    {
        var outside = fractureTool.getChunkMesh(index, false); // 겉면
        var inside = fractureTool.getChunkMesh(index, true);   // 절단면

        var chunkMesh = outside.toUnityMesh();
        chunkMesh.subMeshCount = 2;
        chunkMesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);
        return chunkMesh;
    }

    /// <summary>
    /// 원본 오브젝트의 모든 메쉬를 하나로 병합하여 가져옴
    /// </summary>
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

    /// <summary>
    /// 단일 파편 GameObject 생성 및 컴포넌트 부착
    /// </summary>
    private static GameObject BuildChunk(Material insideMaterial, Material outsideMaterial, Mesh mesh, float mass)
    {
        var chunk = new GameObject("Chunk");

        // 렌더러 설정
        var renderer = chunk.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = new[] { outsideMaterial, insideMaterial };

        var meshFilter = chunk.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        // 파편 노드 스크립트
        var chunkNode = chunk.AddComponent<ChunkNode>();

        // 물리 설정 (Kinematic)
        var rigibody = chunk.AddComponent<Rigidbody>();
        rigibody.mass = mass;
        rigibody.isKinematic = true;

        // 콜라이더 설정
        var mc = chunk.AddComponent<MeshCollider>();
        mc.convex = true;
        mc.sharedMesh = mesh;

        return chunk;
    }

    /// <summary>
    /// 파편 주변의 다른 파편들을 감지하여 이웃으로 등록합니다.
    /// </summary>
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
                    // 양방향 연결
                    myNode.AddNeighbour(otherNode);
                    otherNode.AddNeighbour(myNode);
                }
            }
        }
    }
}