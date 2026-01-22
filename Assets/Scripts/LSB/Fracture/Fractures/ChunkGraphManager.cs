using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전체 파편들의 연결 상태를 관리하는 매니저 클래스
/// 파편간의 연결 상태를 추적하고, 구조적 무결성을 재계산합니다.
/// </summary>
public class ChunkGraphManager : MonoBehaviour
{
    [SerializeField] private ChunkNode[] nodes;

    // BFS 탐색을 위한 자료구조 (필드로 선언하여 가비지 컬렉션 줄임)
    private Queue<ChunkNode> searchQueue = new Queue<ChunkNode>();
    private HashSet<ChunkNode> safeNodes = new HashSet<ChunkNode>();

    /// <summary>
    /// 외부에서 노드 리스트를 주입받아 초기화
    /// </summary>
    public void Setup(ChunkNode[] allNodes)
    {
        nodes = allNodes;
        foreach (var node in nodes)
        {
            if (node != null) node.Setup();
        }
    }

    private void Awake()
    {
        // 노드 리스트가 비어있으면 런타임에 찾기
        if (nodes == null || nodes.Length == 0)
        {
            nodes = GetComponentsInChildren<ChunkNode>();
        }

        // 게임 시작 시, 저장된 배열 데이터를 HashSet으로 복구
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                if (node != null) node.Setup();
            }
        }
    }

    private void FixedUpdate()
    {
        bool needsGraphRebuild = false;

        // 파편 중 하나라도 연결이 끊어졌다고 보고하면 그래프 재계산 트리거
        if (nodes != null)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null) continue;

                if (nodes[i].HasBrokenLinks)
                {
                    nodes[i].CleanBrokenLinks();
                    needsGraphRebuild = true;
                }
            }
        }

        if (needsGraphRebuild)
        {
            RecalculateStructuralIntegrity();
        }
    }

    /// <summary>
    /// 구조적 무결성 재계산 (지지대가 없는 파편 떨어뜨리기)
    /// </summary>
    private void RecalculateStructuralIntegrity()
    {
        searchQueue.Clear();
        safeNodes.Clear();

        // 앵커 찾기
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            // 고정되어 있고(Frozen) + 파괴불가(Indestructible)인 녀석이 진짜 지지대
            if (node != null && node.IsFrozen && node.IsIndestructible)
            {
                searchQueue.Enqueue(node);
                safeNodes.Add(node);
            }
        }

        // 지지대와 연결된 모든 파편 탐색 (BFS 알고리즘)
        while (searchQueue.Count > 0)
        {
            var current = searchQueue.Dequeue();
            var neighbours = current.neighbours; // Setup()이 호출되어야 이 배열이 유효함

            if (neighbours != null)
            {
                for (int i = 0; i < neighbours.Length; i++)
                {
                    var neighbour = neighbours[i];
                    // 유효하고, 아직 방문 안 했고, 고정된 상태라면 -> 안전함
                    if (neighbour != null && !safeNodes.Contains(neighbour) && neighbour.IsFrozen)
                    {
                        safeNodes.Add(neighbour);
                        searchQueue.Enqueue(neighbour);
                    }
                }
            }
        }

        // 지지대와 연결이 끊긴(안전하지 않은) 파편들 추락 시키기
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node != null && node.IsFrozen && !safeNodes.Contains(node))
            {
                node.Unfreeze(); // 물리 켜기 (추락)
            }
        }
    }
}