using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메쉬 파괴(Fracture) 작업을 수행하는 핵심 정적 클래스입니다.
/// NvBlast 라이브러리를 이용해 메쉬를 Voronoi 패턴으로 분할하고, 
/// 분할된 파편(Chunk)들을 GameObject로 변환하여 조립합니다.
/// </summary>
public static class Fracture
{
    // Physics.OverlapBoxNonAlloc에서 사용할 버퍼 (메모리 할당 최적화용)
    private static readonly Collider[] OverlapBuffer = new Collider[64];

    /// <summary>
    /// 대상 게임 오브젝트를 파괴하여 파편들의 집합체로 변환합니다.
    /// </summary>
    /// <param name="gameObject">파괴할 원본 오브젝트</param>
    /// <param name="anchor">고정시킬 면</param>
    /// <param name="seed">파괴 패턴의 랜덤 시드값</param>
    /// <param name="totalChunks">생성할 파편의 개수</param>
    /// <param name="insideMaterial">절단면(안쪽)에 적용할 머티리얼</param>
    /// <param name="outsideMaterial">겉면(바깥쪽)에 적용할 머티리얼</param>
    /// <param name="jointBreakForce">파편 간 연결이 끊어지는 힘의 임계값</param>
    /// <param name="density">파편의 밀도 (질량 계산용)</param>
    /// <returns>생성된 파편들을 관리하는 그래프 매니저</returns>
    public static ChunkGraphManager FractureGameObject(GameObject gameObject, Anchor anchor, int seed, int totalChunks, Material insideMaterial, Material outsideMaterial, float jointBreakForce, float density)
    {
        // 원본 오브젝트의 통합 메쉬 데이터를 가져옵니다.
        var mesh = GetWorldMesh(gameObject);

        // NvBlast에 시드 설정 (동일한 시드면 동일하게 깨짐)
        NvBlastExtUnity.setSeed(seed);

        // Unity Mesh 데이터를 NvMesh(NvBlast용 데이터)로 변환
        var nvMesh = new NvMesh(
            mesh.vertices,
            mesh.normals,
            mesh.uv,
            mesh.vertexCount,
            mesh.GetIndices(0),
            (int)mesh.GetIndexCount(0)
        );

        // 메쉬를 개수에 맞게 분할
        var meshes = FractureMeshesInNvblast(totalChunks, nvMesh);

        // 각 파편의 질량 계산 (전체 부피 * 밀도 / 파편 수)
        var chunkMass = mesh.Volume() * density / totalChunks;

        // 분할된 메쉬 데이터를 실제 GameObject(Chunk) 생성
        var chunks = BuildChunks(insideMaterial, outsideMaterial, meshes, chunkMass);

        // 파편들 간의 인접 청크를 계산하여 등록
        int count = chunks.Count;
        for (int i = 0; i < count; i++)
        {
            RegisterNeighbours(chunks[i], 0.005f, jointBreakForce);
        }

        // 설정된 앵커에 붙어있는 파편들을 고정 처리
        AnchorChunks(gameObject, anchor);

        // 파편들을 담을 부모 오브젝트 생성 및 트랜스폼 설정
        var fractureGameObject = new GameObject("Fracture");
        Transform fractureTransform = fractureGameObject.transform;

        fractureGameObject.transform.position = gameObject.transform.position;
        fractureGameObject.transform.rotation = gameObject.transform.rotation;
        fractureGameObject.transform.localScale = gameObject.transform.localScale;

        // 생성된 파편들을 부모 오브젝트 하위로 이동
        for (int i = 0; i < count; i++)
        {
            chunks[i].transform.SetParent(fractureTransform, true);
        }

        // ChunkGraphManager 컴포넌트 추가 및 초기화
        var graphManager = fractureGameObject.AddComponent<ChunkGraphManager>();

        // 모든 청크 노드들을 그래프 매니저에 등록
        graphManager.Setup(fractureGameObject.GetComponentsInChildren<ChunkNode>());

        return graphManager;
    }

    /// <summary>
    /// 지정된 앵커에 위치한 파편들을 찾아 고정시킵니다.
    /// </summary>
    private static void AnchorChunks(GameObject gameObject, Anchor anchor)
    {
        var transform = gameObject.transform;

        // 프리팹 에셋 등의 경우를 대비해 isSharedMesh=true로 바운드 계산
        var bounds = gameObject.GetCompositeMeshBounds(includeInactive: false, isSharedMesh: true);

        // 해당 범위에 겹치는 콜라이더들을 찾음
        var anchoredColliders = GetAnchoredColliders(anchor, transform, bounds);

        foreach (var collider in anchoredColliders)
        {
            if (collider.TryGetComponent(out ChunkNode node))
            {
                // 해당 노드를 파괴 불가능 상태로 설정
                node.IsIndestructible = true;
            }
        }
    }

    /// <summary>
    /// 분할된 메쉬 리스트를 받아 실제 GameObject들을 생성합니다.
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
    /// NvBlast 라이브러리를 사용하여 Voronoi 기반으로 메쉬를 분할합니다.
    /// </summary>
    private static List<Mesh> FractureMeshesInNvblast(int totalChunks, NvMesh nvMesh)
    {
        var fractureTool = new NvFractureTool();
        // 독립된 섬(Island) 제거 옵션 끄기
        fractureTool.setRemoveIslands(false); 
        fractureTool.setSourceMesh(nvMesh);

        // Voronoi 사이트(점) 생성
        var sites = new NvVoronoiSitesGenerator(nvMesh);
        sites.uniformlyGenerateSitesInMesh(totalChunks);

        // 분할 실행
        fractureTool.voronoiFracturing(0, sites);
        fractureTool.finalizeFracturing();

        // 결과 메쉬 추출
        var meshCount = fractureTool.getChunkCount();
        var meshes = new List<Mesh>(meshCount);
        // 0번은 원본일 수 있으므로 1번부터 추출
        for (var i = 1; i < meshCount; i++) 
        {
            meshes.Add(ExtractChunkMesh(fractureTool, i));
        }

        return meshes;
    }

    /// <summary>
    /// 앵커 방향에 있는 콜라이더들을 Physics.OverlapBox로 찾아냅니다.
    /// </summary>
    private static HashSet<Collider> GetAnchoredColliders(Anchor anchor, Transform meshTransform, Bounds bounds)
    {
        var anchoredChunks = new HashSet<Collider>();
        var frameWidth = .01f; // 감지 범위 두께
        var meshWorldCenter = meshTransform.TransformPoint(bounds.center);
        var meshWorldExtents = Vector3.Scale(bounds.extents, meshTransform.lossyScale);

        // 특정 방향의 경계면을 검사하여 콜라이더 수집하는 내부 함수
        void CheckAndAdd(Vector3 direction, Vector3 halfExtentsMod)
        {
            var center = meshWorldCenter + direction;
            var colliders = Physics.OverlapBox(center, halfExtentsMod, meshTransform.rotation);
            for (int i = 0; i < colliders.Length; i++)
            {
                anchoredChunks.Add(colliders[i]);
            }
        }

        // 각 앵커 플래그에 따라 검사 수행
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
    /// NvBlast 툴에서 Unity Mesh 형태로 데이터를 추출합니다.
    /// 안쪽 면과 바깥쪽 면을 구분하여 SubMesh로 설정합니다.
    /// </summary>
    private static Mesh ExtractChunkMesh(NvFractureTool fractureTool, int index)
    {
        var outside = fractureTool.getChunkMesh(index, false); // 바깥면
        var inside = fractureTool.getChunkMesh(index, true);   // 안쪽면 (절단면)

        var chunkMesh = outside.toUnityMesh();
        chunkMesh.subMeshCount = 2; // 머티리얼 2개 사용
        chunkMesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);
        return chunkMesh;
    }

    /// <summary>
    /// 대상 오브젝트의 자식들에 있는 모든 메쉬를 하나로 합쳐서 가져옵니다.
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
        totalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // 많은 버텍스 지원
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
    /// 단일 파편에 대한 GameObject를 생성하고 컴포넌트를 부착합니다.
    /// </summary>
    private static GameObject BuildChunk(Material insideMaterial, Material outsideMaterial, Mesh mesh, float mass)
    {
        var chunk = new GameObject("Chunk");

        // 렌더러 설정 (겉면, 절단면 머티리얼)
        var renderer = chunk.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = new[] { outsideMaterial, insideMaterial };

        var meshFilter = chunk.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        // 파편 노드 스크립트 추가
        var chunkNode = chunk.AddComponent<ChunkNode>();

        // 물리 설정 처음은 Kinematic으로 고정
        var rigibody = chunk.AddComponent<Rigidbody>();
        rigibody.mass = mass;
        rigibody.isKinematic = true;

        // 콜라이더 설정 (Convex 필수)
        var mc = chunk.AddComponent<MeshCollider>();
        mc.convex = true;
        mc.sharedMesh = mesh;

        return chunk;
    }

    /// <summary>
    /// 파편 주변의 다른 파편들을 감지하여 이웃(Neighbour)으로 등록합니다.
    /// 그래프 연결성을 구축하는 중요한 단계입니다.
    /// </summary>
    private static void RegisterNeighbours(GameObject chunk, float touchRadius, float breakForce)
    {
        if (!chunk.TryGetComponent(out ChunkNode myNode)) return;

        myNode.BreakForce = breakForce;

        if (!chunk.TryGetComponent(out MeshCollider mc)) return;

        // 자신의 콜라이더보다 약간 더 큰 범위로 겹치는 물체 검색
        var bounds = mc.bounds;
        Vector3 center = bounds.center;
        Vector3 halfExtents = bounds.extents + new Vector3(touchRadius, touchRadius, touchRadius);

        int hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, OverlapBuffer, chunk.transform.rotation, -1, QueryTriggerInteraction.Ignore);

        for (int j = 0; j < hitCount; j++)
        {
            var col = OverlapBuffer[j];
            // 자기 자신이 아니고, 리지드바디가 있는 경우
            if (col.attachedRigidbody != null && col.gameObject != chunk)
            {
                if (col.TryGetComponent(out ChunkNode otherNode))
                {
                    // 서로를 이웃으로 추가 (양방향 연결)
                    myNode.AddNeighbour(otherNode);
                    otherNode.AddNeighbour(myNode);
                }
            }
        }
    }
}