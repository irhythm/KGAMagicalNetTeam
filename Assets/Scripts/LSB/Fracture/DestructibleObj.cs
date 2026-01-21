using UnityEngine;

/// <summary>
/// 런타임에 파괴 효과를 제어하는 스크립트입니다.
/// 평소에는 SolidModel을 보여주다가, 충격을 받으면 FracturedRoot를 활성화하고 물리력을 가합니다.
/// </summary>
public class DestructibleWall : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject solidModel;    // 멀쩡한 상태의 오브젝트
    [SerializeField] private GameObject fracturedRoot; // 깨진 파편들의 부모 오브젝트

    [Header("Settings")]
    [SerializeField] private float despawnDelay = 10f; // 파편 제거 대기 시간

    private bool _isBroken = false;    // 이미 깨졌는지 체크 플래그
    private Rigidbody[] _chunkRBs;     // 파편들의 리지드바디 캐싱용 배열

    private void Awake()
    {
        // Solid는 켜고, Fractured는 끔
        if (solidModel != null) solidModel.SetActive(true);
        if (fracturedRoot != null)
        {
            // 성능 최적화를 위해 미리 리지드바디들을 찾아놓음
            _chunkRBs = fracturedRoot.GetComponentsInChildren<Rigidbody>(true);
            fracturedRoot.SetActive(false);
        }
    }

    /// <summary>
    /// 외부에서 호출하는 파괴 함수입니다.
    /// </summary>
    public void OnHit(Vector3 hitPoint, float force, float radius, float upward)
    {
        if (_isBroken) return; // 이미 깨졌으면 무시
        _isBroken = true;

        // 멀쩡한 모델 숨기기
        if (solidModel != null) solidModel.SetActive(false);

        // 파편들 활성화 및 물리력 적용
        if (fracturedRoot != null)
        {
            fracturedRoot.SetActive(true);

            foreach (var rb in _chunkRBs)
            {
                if (rb != null)
                {
                    rb.isKinematic = false; // 물리 연산 시작
                    rb.WakeUp();            // 잠자던 물리 객체 깨우기
                    // 폭발력 적용
                    rb.AddExplosionForce(force, hitPoint, radius, upward, ForceMode.Impulse);
                }
            }
        }
    }
}