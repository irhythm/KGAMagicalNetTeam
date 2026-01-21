using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전체 파편들의 연결 상태를 관리하는 매니저입니다.
/// BFS(너비 우선 탐색)를 통해 지지대(Anchor)와 연결되지 않은 공중 부양 파편을 감지합니다.
/// </summary>
public class ChunkGraphManager : MonoBehaviour
{
    // 전체 파편 노드 리스트
    [SerializeField] private ChunkNode[] nodes;

    // BFS 탐색을 위한 자료구조 (재사용하여 가비지 컬렉션 줄임)
    private Queue<ChunkNode> searchQueue = new Queue<ChunkNode>();
    private HashSet<ChunkNode> safeNodes = new HashSet<ChunkNode>();

    /// <summary>
    /// 외부에서 노드 리스트를 주입받아 초기화합니다.
    /// </summary>
    public void Setup(ChunkNode[] allNodes)
    {
        nodes = allNodes;

        // 각 노드의 Setup 호출
        foreach (var node in nodes)
        {
            if (node != null) node.Setup();
        }
    }

    private void Awake()
    {
        // 노드 리스트가 비어있으면 런타임에 찾음
        if (nodes == null || nodes.Length == 0)
        {
            nodes = GetComponentsInChildren<ChunkNode>();
        }
    }

    private void FixedUpdate()
    {
        bool needsGraphRebuild = false;

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null) continue;

                // 어떤 파편이 이웃과의 연결이 끊어졌다고 보고하면 그래프 재검사 예약
                if (nodes[i].HasBrokenLinks)
                {
                    nodes[i].CleanBrokenLinks(); // 끊어진 링크 정리
                    needsGraphRebuild = true;
                }
            }
        }

        // 연결 구조가 변경되었으면 재계산 수행
        if (needsGraphRebuild)
        {
            RecalculateStructuralIntegrity();
        }
    }

    /// <summary>
    /// 공중에 떠 있는 파편을 찾아 떨어뜨립니다.
    /// </summary>
    private void RecalculateStructuralIntegrity()
    {
        searchQueue.Clear();
        safeNodes.Clear();

        // 초기 안전 노드 식별: 땅(Anchor)에 붙어있고 고정된 노드들
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node != null && node.IsFrozen && node.IsIndestructible)
            {
                searchQueue.Enqueue(node);
                safeNodes.Add(node);
            }
        }

        // BFS 탐색: 안전한 노드와 연결된 모든 이웃들을 찾아 안전 목록에 추가
        while (searchQueue.Count > 0)
        {
            var current = searchQueue.Dequeue();

            var neighbours = current.neighbours; // 이웃 배열

            if (neighbours != null)
            {
                for (int i = 0; i < neighbours.Length; i++)
                {
                    var neighbour = neighbours[i];

                    // 유효하고, 아직 방문 안 했고, 고정된 상태 = 안전함
                    if (neighbour != null && !safeNodes.Contains(neighbour) && neighbour.IsFrozen)
                    {
                        safeNodes.Add(neighbour);
                        searchQueue.Enqueue(neighbour);
                    }
                }
            }
        }

        // 안전 목록에 없는 고정 노드들은 지지대가 없으므로 물리화 시킴
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node != null && node.IsFrozen && !safeNodes.Contains(node))
            {
                node.Unfreeze();
            }
        }
    }
}