using UnityEngine;
using Photon.Pun;

//주둔지 경비병 AI
public class GarrisonGuardAI : GuardAI
{
    [Header("주둔지 설정")]
    public float fovAngle = 110f;       //시야각
    public float maxChaseDist = 30f;    //주둔지 중심부터 최대 거리
    public float heightThreshold = 2.5f;//시야높이임계점
    public float callColleagueRadius = 15f; //무전 반경


    [Header("주둔지 중심")]
    public Transform garrisonCenter; //여기부터 30~50미터?

    private Vector3 initialSpawnPos;
    public Vector3 InitialSpawnPos => initialSpawnPos;


    [Header("시야 체크용")]
    public Transform eyeTransform; //눈위치 정도

    //소음/신고 감지 상태 저장용 (State에서 갖다 씀)
    public Vector3 lastNoisePos;
    public bool hasNoiseDetected = false;


    [Header("끼즈모")]
    public Color gizmoColor = new Color(0, 0, 1, 0.5f);

    protected override void Awake()
    {
        base.Awake();

        initialSpawnPos = transform.position;
        if (garrisonCenter == null)
        {
            garrisonCenter = transform;
        }

        if (eyeTransform == null)
        {
            eyeTransform = transform;
        }
    }

    //오버라이드
    protected override void SetInitialState()
    {
        ChangeState(new GarrisonPatrolState(this, stateMachine));
    }

    //4단계 시야 필터링
    //거리  / 높이 / 각도 / 레이
    //오버랩/ 임계 / 앵글 / 레이
    public override bool CheckEnemyNearby()
    {
        //거리 체크
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectRadius, connectionBuffer, targetMask);

        if (count == 0)
        {
            targetPlayer = null;
            return false;
        }

        Transform bestTarget = null;
        float minSqrDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Transform target = connectionBuffer[i].transform;

            //높이 체크
            float yDiff = Mathf.Abs(target.position.y - transform.position.y);
            if (yDiff > heightThreshold) continue;

            Vector3 dirToTarget = (target.position - transform.position).normalized;

            //시야각 체크, 정면과 타겟 사이 각도가 시야각의 절반보다 크면 시야 밖
            if(Vector3.Angle(transform.forward, dirToTarget) > fovAngle * 0.5f) continue;

            if (Vector3.Distance(garrisonCenter.position, target.position) > maxChaseDist) continue;

            //레이캐스트, 일단 4발
            if (CanSeeTarget(target))
            {
                //가장 가까운 대상 타겟팅
                float sqrDist = (target.position - transform.position).sqrMagnitude;
                minSqrDist = sqrDist;
                bestTarget = target;
            }
        }

        //타겟 확정
        if (bestTarget != null)
        {
            if (targetPlayer != bestTarget)
            {
                targetPlayer = bestTarget;
                targetPv = targetPlayer.GetComponent<PhotonView>();
            }
            return true;
        }

        targetPlayer = null;
        return false;

    }


    //멀티 레이캐스트 로직
    private bool CanSeeTarget(Transform target)
    {
        Vector3 targetPos = target.position;
        Vector3 up = Vector3.up;
        Vector3 right = target.right;

        //타겟기준 4곳 쏘기
        //얘 자체 콜라이더 범위값을 기준으로 쓰도록 리팩토링 = 모델마다 다른 범위를 체크 가능
        //bounds.extents min Max, or Center값의 익스텐스의 비례한 값을 쏘는 식으로

        Vector3[] checkPoints = new Vector3[]
        {
            targetPos + up * 1.0f,
            targetPos + up * 1.6f,
            targetPos + up * 1.0f + right * 0.3f,
            targetPos + up * 1.0f - right * 0.3f
        };

        foreach (var point in checkPoints)
        {
            Vector3 dir = (point - eyeTransform.position).normalized;
            float dist = Vector3.Distance(eyeTransform.position, point);

            RaycastHit hit;

            if (Physics.Raycast(eyeTransform.position, dir, out hit, dist))
            {
                if (hit.transform == target)
                {
                    //임시 에디터 체크용
#if UNITY_EDITOR
                    Debug.DrawLine(eyeTransform.position, point, Color.red, 0.1f);
#endif
                    return true; //하나라도 보이면 끝
                }
            }
        }
        //4발 전부 오발
        return false;
    }

    //소음 감지
    public override void OnHearNoise(Vector3 noisePos)
    {
        //주둔지 체크
        if (Vector3.Distance(garrisonCenter.position, noisePos) > maxChaseDist) return;
        if (currentNetworkState == AIStateID.Chase || currentNetworkState == AIStateID.Attack) return;

        lastNoisePos = noisePos;
        hasNoiseDetected = true;
    }

    //내부망 신고(가드매니저 안 씀 애는, 오버랩스피어로 주변 동료 호출)
    public override void ReportEnemy(Vector3 pos)
    {
        Collider[] colleagues = Physics.OverlapSphere(transform.position, callColleagueRadius);

        foreach (var col in colleagues)
        {
            GarrisonGuardAI nakama = col.GetComponent<GarrisonGuardAI>();
            if (nakama != null&& nakama != this)
            {
                nakama.ReceiveAlert(pos);
            }
        }
    }

    //동료의 신고를 받는 함수(호출되면 받기)
    public void ReceiveAlert(Vector3 enemyPos)
    {
        if (targetPlayer != null) return;

        //소리 듣고 가는 로직 그대로 사용 무전을 들었다 개념으로다가
        lastNoisePos = enemyPos;
        hasNoiseDetected = true;
    }

    //기즈모
    protected override void OnDrawGizmosSelected()
    {
        //부모의 detectedRadius감지범위, 빨강
        base.OnDrawGizmosSelected();
        
        //기지 범위, 파랑
        Gizmos.color = gizmoColor;
        Vector3 centerPos = (garrisonCenter != null) ? garrisonCenter.position : transform.position;
        Gizmos.DrawWireSphere(centerPos, maxChaseDist);

        //fovAngle시야각, 초록
        Gizmos.color = Color.green;
        Vector3 leftRay = Quaternion.Euler(0, -fovAngle * 0.5f, 0) * transform.forward;
        Vector3 rightRay = Quaternion.Euler(0, fovAngle * 0.5f, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, leftRay * detectRadius);
        Gizmos.DrawRay(transform.position, rightRay * detectRadius);
    }

    protected override void ProcessDamage(float damage)
    {
        //stateMachine 체크해서 리턴중이면 데미지 무시
        if (stateMachine != null && stateMachine.CurrentState is GarrisonReturnState) return;
        base.TakeDamage(damage);
    }
}
