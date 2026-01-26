using UnityEngine;
using UnityEngine.AI;

public class GarrisonPatrolState : AIStateBase
{
    private GarrisonGuardAI garrisonAI;

    //대기/이동 타이머
    private float waitTimer = 0f;
    private float moveTimer = 0f;
    private bool isWaiting = false;

    //최적화_ 1초에 5번 정도?
    private float detectTimer = 0f;
    private float detectInterval = 0.2f; 

    //랜덤성 부여용
    private float waitDuration = 2.0f; //도착 시 n초 대기
    private float moveDuration = 4.0f; //n초 동안 못 가면 다른 곳 찾기 + 끼임 방지

    public GarrisonPatrolState(BaseAI ai, StateMachine stateMachine) : base(ai, stateMachine, BaseAI.AIStateID.Patrol)
    {
        garrisonAI = ai as GarrisonGuardAI;
    }

    public override void Enter()
    {
        base.Enter();

        if (garrisonAI == null) return;

        //이속세팅
        if (garrisonAI.Agent != null && garrisonAI.Agent.isOnNavMesh)
        {
            garrisonAI.Agent.isStopped = false;
            garrisonAI.Agent.speed = garrisonAI.patrolSpeed;
        }

        //첫 번째 목적지 찾기
        SetRandomDestination();
    }

    //랜덤 목적지 설정
    private void SetRandomDestination()
    {
        //중심 지정
        Vector3 center = garrisonAI.garrisonCenter.position;
        float radius = garrisonAI.maxChaseDist; //30?

        Vector3 randomPoint = GetRandomPoint(center, radius);

        if (garrisonAI.Agent.isOnNavMesh)
        {
            garrisonAI.Agent.isStopped = false;
            garrisonAI.Agent.SetDestination(randomPoint);
        }
    }

    //네비 위 랜덤좌표 구하기
    private Vector3 GetRandomPoint(Vector3 center, float radius) //벡터반환 인자값은 센터에서 범위
    {
        Vector3 randomDir = Random.insideUnitSphere * radius; //구 안의 랜덤 좌표
        randomDir += center;

        NavMeshHit hit;

        //베이크땅확인
        if (NavMesh.SamplePosition(randomDir, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }

    public override void Execute()
    {
        if (garrisonAI == null) return;

        //최적화_연산량 감소용 타이머
        detectTimer += Time.deltaTime;
        if (detectTimer >= detectInterval)
        {
            detectTimer = 0f;
            if (garrisonAI.CheckEnemyNearby())
            {
                //시야각 발견 시 추적 상태로 전환
                ai.ChangeState(new GarrisonChaseState(ai, stateMachine));
                return;
            }
        }

        //소음 감지 시, 거기로 이동. 속도?
        if (garrisonAI.hasNoiseDetected)
        {
            MoveToNoiseLocation(garrisonAI.lastNoisePos);
            return;
        }

        //평상시 순찰
        PatrolBehavior();
    }
    
    //대기 시작
    private void StartWait()
    {
        isWaiting = true;
        waitTimer = 0f;
        if (garrisonAI.Agent.isOnNavMesh)
        {
            garrisonAI.Agent.isStopped = true;
        }
    }

    //소음 위치로 이동하는 함수
    private void MoveToNoiseLocation(Vector3 noisePos)
    {
        garrisonAI.hasNoiseDetected = false;

        if (garrisonAI.Agent.isOnNavMesh)
        {
            garrisonAI.Agent.isStopped = false;
            garrisonAI.Agent.SetDestination(noisePos);

            //이동 로직 초기화
            isWaiting = false;
            moveTimer = 0f;
        }
    }

    //순찰 행동 패턴
    private void PatrolBehavior()
    {
        if (garrisonAI.Agent == null || !garrisonAI.Agent.isOnNavMesh) return;

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitDuration)
            {
                isWaiting = false;
                SetRandomDestination();
                moveTimer = 0f;
            }
        }
        else
        {   
            //남은 거리가 멈출 거리보다 적으면 대기
            if (!garrisonAI.Agent.pathPending && garrisonAI.Agent.remainingDistance <= garrisonAI.Agent.stoppingDistance)
            {
                StartWait();
            }

            moveTimer += Time.deltaTime;
            if (moveTimer > moveDuration)
            {
                StartWait();
            }
        }
    }


}
