using System.Collections.Generic;
using UnityEngine;

public class ChunkGraphManager : MonoBehaviour
{
    private ChunkNode[] nodes;

    // Flood Fill을 위한 컬렉션
    private Queue<ChunkNode> searchQueue = new Queue<ChunkNode>();
    private HashSet<ChunkNode> safeNodes = new HashSet<ChunkNode>();

    public void Setup(Rigidbody[] bodies)
    {
        nodes = new ChunkNode[bodies.Length];
        for (int i = 0; i < bodies.Length; i++)
        {
            var node = bodies[i].GetComponent<ChunkNode>();
            if (node == null) node = bodies[i].gameObject.AddComponent<ChunkNode>();
            node.Setup();
            nodes[i] = node;
        }
    }

    private void FixedUpdate()
    {
        // 1. 끊어진 링크가 있는지 확인
        bool needsGraphRebuild = false;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null && nodes[i].HasBrokenLinks)
            {
                nodes[i].CleanBrokenLinks();
                needsGraphRebuild = true;
            }
        }

        // 2. 끊어진 곳이 있다면 전체 구조 안전성 검사 실행
        if (needsGraphRebuild)
        {
            RecalculateStructuralIntegrity();
        }
    }

    // [핵심 변경] 구조적 무결성 재계산 (Flood Fill 방식)
    private void RecalculateStructuralIntegrity()
    {
        searchQueue.Clear();
        safeNodes.Clear();

        // 1단계: '진짜 앵커(땅)'들을 찾아서 큐에 넣고 안전하다고 표시
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            // 노드가 살아있고 && 고정 상태이고 && 불파괴(땅) 속성이라면 시작점
            if (node != null && node.IsFrozen && node.IsIndestructible)
            {
                searchQueue.Enqueue(node);
                safeNodes.Add(node);
            }
        }

        // 2단계: 앵커로부터 연결된 모든 친구들을 탐색 (BFS)
        while (searchQueue.Count > 0)
        {
            var current = searchQueue.Dequeue();
            var neighbours = current.NeighboursArray;

            for (int i = 0; i < neighbours.Length; i++)
            {
                var neighbour = neighbours[i];

                // 이웃이 존재하고 && 아직 안전 마크를 안 받았고 && 현재 고정된 상태라면
                if (neighbour != null && !safeNodes.Contains(neighbour) && neighbour.IsFrozen)
                {
                    safeNodes.Add(neighbour); // 살려줌
                    searchQueue.Enqueue(neighbour); // 다음 탐색 예약
                }
            }
        }

        // 3단계: 안전 리스트에 없는 녀석들은 모두 가짜(공중부양)이므로 떨어뜨림
        int droppedCount = 0;
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];

            // 살아있고 && 고정되어 있는데 && 안전 리스트에 없다면? -> 추락!
            if (node != null && node.IsFrozen && !safeNodes.Contains(node))
            {
                node.Unfreeze();
                droppedCount++;
            }
        }

        // (디버그용) 몇 개나 떨어졌는지 로그 확인 가능
        // if(droppedCount > 0) Debug.Log($"Structural Failure! Dropped {droppedCount} floating chunks.");
    }

    [Header("Debug")]
    public bool ShowGraphGizmos = true;

    private void OnDrawGizmos()
    {
        if (!ShowGraphGizmos || nodes == null) return;

        foreach (var node in nodes)
        {
            if (node == null || !node.IsFrozen) continue;

            // 앵커는 빨간색
            if (node.IsIndestructible)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(node.transform.position, 0.1f);
            }
            // 안전한 노드(지지대 있음)는 초록색
            else if (safeNodes.Contains(node))
            {
                Gizmos.color = Color.green;
                // 연결선 그리기는 너무 복잡해지므로 점만 찍음
                Gizmos.DrawWireSphere(node.transform.position, 0.05f);
            }
            // 위험한 노드(공중부양 - 다음 프레임에 떨어질 예정)는 노란색
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(node.transform.position, 0.1f);
            }
        }
    }
}