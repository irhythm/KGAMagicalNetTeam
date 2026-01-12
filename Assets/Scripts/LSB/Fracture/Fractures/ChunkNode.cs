using System.Collections.Generic;
using UnityEngine;

public class ChunkNode : MonoBehaviour
{
    private HashSet<ChunkNode> _neighbours = new HashSet<ChunkNode>();
    public ChunkNode[] NeighboursArray { get; private set; } = new ChunkNode[0];

    private Rigidbody rb;

    // [수정] 단순히 Kinematic인 것과 '진짜 고정된 땅'을 구분합니다.
    public bool IsIndestructible { get; set; } = false;

    // [추가] 외부 충격에 견디는 힘 (기본값 설정, 필요시 Fracture.cs에서 주입)
    public float BreakForce { get; set; } = 50f;

    public bool HasBrokenLinks { get; private set; }
    public bool IsFrozen => rb != null && rb.isKinematic;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Setup()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        Freeze();
        RefreshNeighboursArray();
    }

    // [추가] 물리 조인트가 없으므로 직접 충돌을 감지해야 합니다.
    private void OnCollisionEnter(Collision collision)
    {
        // 이미 떨어져 나온 상태라면 무시
        if (!rb.isKinematic) return;

        // 충격량이 한계치를 넘으면 연결 해제
        if (collision.impulse.magnitude > BreakForce)
        {
            Unfreeze(); // 나 자신을 떨어뜨림 -> 이웃들에게 전파됨
        }
    }

    // 폭발 등으로 직접 데미지를 줄 때 호출할 함수
    public void ApplyImpact(float force)
    {
        if (force > BreakForce) Unfreeze();
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
            HasBrokenLinks = true; // 그래프 매니저에게 "재검사 필요" 신호 보냄
        }
    }

    private void RefreshNeighboursArray()
    {
        NeighboursArray = new ChunkNode[_neighbours.Count];
        _neighbours.CopyTo(NeighboursArray);
    }

    public void CleanBrokenLinks() => HasBrokenLinks = false;

    public void Unfreeze()
    {
        // 진짜 앵커(벽/바닥)는 절대 떨어지지 않음
        if (IsIndestructible) return;

        if (rb != null && rb.isKinematic)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.gameObject.layer = LayerMask.NameToLayer("Default");

            // 내가 떨어지면 이웃들과의 관계를 끊음 -> 이웃들이 "어? 내 옆이 사라졌네?" 하고 GraphManager가 작동함
            foreach (var neighbor in _neighbours)
            {
                if (neighbor != null) neighbor.RemoveNeighbour(this);
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
            // "FrozenChunks" 레이어가 없다면 Default나 다른 레이어 사용
            int layer = LayerMask.NameToLayer("FrozenChunks");
            if (layer != -1) rb.gameObject.layer = layer;
        }
    }
}