using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개별 파편(Chunk)의 상태와 이웃 관계를 관리하는 클래스입니다.
/// </summary>
public class ChunkNode : MonoBehaviour
{
    // 프리팹 저장용 이웃 배열
    [SerializeField] public ChunkNode[] neighbours;

    // 런타임 로직용 이웃 집합 (빠른 검색용 HashSet)
    private HashSet<ChunkNode> _neighbours = new HashSet<ChunkNode>();

    // 외부 공개용 프로퍼티
    public ChunkNode[] NeighboursArray { get; private set; } = new ChunkNode[0];

    private Rigidbody rb;
    public bool IsIndestructible { get; set; } = false; // 땅에 붙은 고정 노드 여부
    public float BreakForce { get; set; } = 50f;        // 견딜 수 있는 힘

    // 연결이 끊어졌음을 매니저에게 알리는 플래그
    public bool HasBrokenLinks { get; private set; } = false;

    // 현재 고정되어 있는가? (Kinematic 상태)
    public bool IsFrozen => rb != null && rb.isKinematic;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 초기화 함수. 저장된 이웃 데이터를 HashSet으로 복구합니다.
    /// </summary>
    public void Setup()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        // 저장된 배열 데이터가 있는데 HashSet이 비어있다면 복구
        if (neighbours != null && neighbours.Length > 0 && _neighbours.Count == 0)
        {
            _neighbours = new HashSet<ChunkNode>(neighbours);
            _neighbours.RemoveWhere(n => n == null); // null 정리
        }

        Freeze(); // 초기엔 고정
        RefreshNeighboursArray();
    }

    /// <summary>
    /// 현재의 HashSet 이웃 정보를 배열로 변환해 저장합니다.
    /// </summary>
    public void SyncNeighboursToArray()
    {
        _neighbours.RemoveWhere(n => n == null);
        neighbours = new ChunkNode[_neighbours.Count];
        _neighbours.CopyTo(neighbours);
    }

    /// <summary>
    /// 끊어진 링크 정보를 정리하고 플래그를 초기화합니다.
    /// </summary>
    public void CleanBrokenLinks()
    {
        // 사라지거나 떨어진(Unfrozen) 이웃을 제거
        int removedCount = _neighbours.RemoveWhere(n => n == null || !n.IsFrozen);

        if (removedCount > 0)
        {
            RefreshNeighboursArray();
            SyncNeighboursToArray();
        }

        HasBrokenLinks = false;
    }

    public void AddNeighbour(ChunkNode node)
    {
        if (node == null || node == this) return;
        if (!_neighbours.Contains(node))
        {
            _neighbours.Add(node);
            RefreshNeighboursArray();
        }
    }

    public void RemoveNeighbour(ChunkNode chunkNode)
    {
        if (_neighbours.Contains(chunkNode))
        {
            _neighbours.Remove(chunkNode);
            RefreshNeighboursArray();
            HasBrokenLinks = true; // 링크 끊어짐 알림
        }
    }

    private void RefreshNeighboursArray()
    {
        NeighboursArray = new ChunkNode[_neighbours.Count];
        _neighbours.CopyTo(NeighboursArray);
    }

    /// <summary>
    /// 파편을 물리화하여 떨어뜨립니다. (Kinematic 해제)
    /// </summary>
    public void Unfreeze()
    {
        if (IsIndestructible) return; // 고정된 앵커라면 떨어지지 않음

        if (rb != null && rb.isKinematic)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.gameObject.layer = LayerMask.NameToLayer("Default");

            // 자신이 떨어지면 이웃들도 재검사 대상이 됨
            foreach (var neighbor in _neighbours)
            {
                if (neighbor != null)
                {
                    neighbor.RemoveNeighbour(this);
                }
            }

            _neighbours.Clear();
            RefreshNeighboursArray();
        }
    }

    private void Freeze()
    {
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}