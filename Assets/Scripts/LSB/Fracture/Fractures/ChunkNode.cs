using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// 파편의 상태와 이웃의 정보를 관리하는 노드
/// </summary>
public class ChunkNode : MonoBehaviour, IExplosion
{
    // 프리팹 직렬화용 이웃 배열
    [SerializeField] public ChunkNode[] neighbours;

    // 빠른 검색/삭제를 위한 HashSet (게임 시작 시 배열에서 복구됨)
    private HashSet<ChunkNode> _neighbours = new HashSet<ChunkNode>();

    public ChunkNode[] NeighboursArray { get; private set; } = new ChunkNode[0];

    private Rigidbody rb;

    // 앵커 여부를 필드로 선언하여 Unity가 프리팹에 저장(직렬화)할 수 있게 함
    [SerializeField] public bool IsIndestructible = false;
    [SerializeField] public float BreakForce = 50f;

    // 연결이 끊어졌음을 매니저에게 알리는 플래그
    public bool HasBrokenLinks { get; private set; } = false;

    // 현재 고정되어 있는가?
    public bool IsFrozen => rb != null && rb.isKinematic;

    // 파편이 부서진 후 사라질 때까지의 시간
    [SerializeField] public float DebrisLifetime = 5f;

    // UI 및 외부 시스템 연결을 위한 이벤트
    public static event Action<ChunkNode> OnAnyChunkBroken;

    // 개별 파편이 사라질 때 발생하는 이벤트
    public event Action OnDebrisDisable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 초기화 함수. 저장된 이웃 배열 데이터를 HashSet으로 복구합니다.
    /// </summary>
    public void Setup()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        // 저장된 배열 -> HashSet 복구 (중복 방지 및 빠른 연산)
        if (neighbours != null && neighbours.Length > 0 && _neighbours.Count == 0)
        {
            _neighbours = new HashSet<ChunkNode>(neighbours);
            _neighbours.RemoveWhere(n => n == null);
        }

        Freeze(); // 초기엔 고정 상태로 시작
        RefreshNeighboursArray();
    }

    /// <summary>
    /// HashSet의 데이터를 배열로 동기화합니다. (베이킹 시 사용)
    /// </summary>
    public void SyncNeighboursToArray()
    {
        _neighbours.RemoveWhere(n => n == null);
        neighbours = new ChunkNode[_neighbours.Count];
        _neighbours.CopyTo(neighbours);
    }

    /// <summary>
    /// 끊어지거나 사라진 이웃을 리스트에서 정리합니다.
    /// </summary>
    public void CleanBrokenLinks()
    {
        // null이거나 이미 떨어진(Unfrozen) 이웃 제거
        int c = _neighbours.RemoveWhere(n => n == null || !n.IsFrozen);
        if (c > 0)
        {
            RefreshNeighboursArray();
            SyncNeighboursToArray();
        }
        HasBrokenLinks = false;
    }

    // 이웃 추가
    public void AddNeighbour(ChunkNode n) 
    { 
        if (!_neighbours.Contains(n)) 
        { 
            _neighbours.Add(n); RefreshNeighboursArray(); 
        } 
    }

    // 이웃 제거 (제거 시 플래그를 세워 매니저에게 알림)
    public void RemoveNeighbour(ChunkNode n) 
    { 
        if (_neighbours.Contains(n)) 
        { 
            _neighbours.Remove(n); 
            RefreshNeighboursArray(); 
            HasBrokenLinks = true; 
        } 
    }

    // 외부 공개용 배열 갱신
    private void RefreshNeighboursArray() 
    { 
        NeighboursArray = new ChunkNode[_neighbours.Count]; 
        _neighbours.CopyTo(NeighboursArray); 
    }

    /// <summary>
    /// 외부에서 호출하여 파편에 폭발력을 가합니다.
    /// </summary>
    public void ApplyExplosionForce(float explosionForce, Vector3 explosionPos, float explosionRadius, float upwardModifier)
    {
        // 앵커인 경우 힘을 무시
        if (IsIndestructible) return;

        // Kinematic 상태라면 해제
        if (IsFrozen)
        {
            Unfreeze();
        }

        // 실제 물리력 적용
        if (rb != null)
        {
            rb.AddExplosionForce(explosionForce, explosionPos, explosionRadius, upwardModifier, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// 파편을 고정 해제하여 중력의 영향을 받게 합니다.
    /// </summary>
    public void Unfreeze()
    {
        if (IsIndestructible) return;

        if (rb != null && rb.isKinematic)
        {
            rb.isKinematic = false; // 물리 켜기
            rb.useGravity = true;   // 중력 켜기
            rb.gameObject.layer = LayerMask.NameToLayer("Default"); // 레이어 변경

            // 내가 떨어지면 나와 연결된 이웃들의 목록에서도 나를 지워야 함
            foreach (var neighbor in _neighbours)
            {
                if (neighbor != null) neighbor.RemoveNeighbour(this);
            }
            _neighbours.Clear(); // 내 이웃 목록 초기화
            RefreshNeighboursArray();

            // 부서짐 이벤트 발생
            OnAnyChunkBroken?.Invoke(this);

            // 일정 시간 뒤 비활성화 코루틴 시작
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(DisableDebrisRoutine());
            }
        }
    }

    // 사라짐 처리 코루틴
    private IEnumerator DisableDebrisRoutine()
    {
        yield return CoroutineManager.waitForSeconds(DebrisLifetime);

        OnDebrisDisable?.Invoke();
        gameObject.SetActive(false);
    }

    private void Freeze()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    // 에디터에서 앵커 상태를 시각적으로 확인하기 위한 기즈모
    private void OnDrawGizmos()
    {
        if (IsIndestructible)
        {
            Gizmos.color = Color.red;
            Collider col = GetComponent<Collider>();
            // 파편의 중심점에 빨간 구체 표시
            Vector3 pos = (col != null) ? col.bounds.center : transform.position;
            Gizmos.DrawSphere(pos, 0.2f);
        }
    }

    public void OnExplosion(Vector3 explosionPos, MagicDataSO data, int attackerActorNr)
    {

        ApplyExplosionForce(
            data.knockbackForce,
            explosionPos,
            data.radius,
            data.forceUpward
        );
    }
}