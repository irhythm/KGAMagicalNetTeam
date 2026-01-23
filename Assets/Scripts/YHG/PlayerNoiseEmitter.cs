using Photon.Pun;
using UnityEngine;

//플레이어블 캐릭터 프리팹에 부착할 예정
//소음 발생기
public class PlayerNoiseEmitter : MonoBehaviourPun
{
    [Header("소음 반경 설정")]
    public float runNoiseRadius = 10f; 
    public float sprintNoiseRadius = 20f;

    [Header("탐지 설정")]
    public LayerMask guardLayer;
    public float noiseInterval = 0.4f;

    //참조용
    private PlayerInputHandler inputHandler;

    [Header("디버그 설정")]
    public Color gizmoColor = new Color(0, 1, 1, 0.4f);

    //GC 방지용 버퍼
    private Collider[] noiseBuffer = new Collider[10];
    private float noiseTimer = 0f;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        //내 캐릭터만 소음 발생
        if (!photonView.IsMine) return;
        HandleMovementNoise();
    }
    private void HandleMovementNoise()
    {
        //움직이는 중인가용??
        bool isMoving = inputHandler.MoveInput.sqrMagnitude > 0.01f;

        //걷고있나용?, 타이머 리셋
        if (!isMoving || inputHandler.IsWalkInput)
        {
            noiseTimer = 0f;
            return;
        }

        //달리기, 질주 판별
        bool isSprinting = inputHandler.IsSprintInput;

        float currentRadius = isSprinting ? sprintNoiseRadius : runNoiseRadius;
        //질주면 0.3초체크
        float currentInterval = isSprinting ? 0.3f : noiseInterval;

        noiseTimer += Time.deltaTime;
        
        if (noiseTimer >= currentInterval)
        {
            noiseTimer = 0f;
            EmitNoise(currentRadius);
        }
    }

    //소음 발생
    public void EmitNoise(float radius)
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, noiseBuffer, guardLayer);
        for (int i = 0; i < count; i++)
        {
            GuardAI guard = noiseBuffer[i].GetComponent<GuardAI>();
            if (guard != null)
            {
                guard.OnHearNoise(transform.position);
            }
        }
    }

    //내부 마법 소음용(외부 호출), 나중에 기존 마법소음과 통합할수도?
    public void EmitMagicNoise(float radius)
    {
        EmitNoise(radius);
    }

    //에디터 범위 눈으로 확인
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, runNoiseRadius);
    }
}
