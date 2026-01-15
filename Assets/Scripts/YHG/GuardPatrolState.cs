using UnityEngine;
using UnityEngine.AI;
//경비 순찰상태, 매니저통신 지원요청 오면 뛰어가고 없으면 배회
public class GuardPatrolState : AIStateBase
{
    private GuardAI guard;

    //동시 생각 방지
    private float decisionTimer = 0f;
    private float decisionInterval;

    //마지막 목적지 기억용
    private Vector3 lastDestination = Vector3.zero;

    public GuardPatrolState(BaseAI ai, StateMachine stateMachine) : base(ai, stateMachine, BaseAI.AIStateID.Patrol)
    {
        guard = ai as GuardAI;
        //랜덤 설정
        decisionInterval = Random.Range(0.4f, 0.6f);
    }
    public override void Enter()
    {
        base.Enter(); //네트워크 상태 동기화

        if (guard.Agent != null && guard.Agent.isOnNavMesh)
        {
            guard.Agent.isStopped = false;
            guard.Agent.speed = guard.patrolSpeed;
        }

        //판단 1번
        DecideDestination();
    }


    //내앞에있는가, 갈 곳이 있는가(가드매니저) 체크 시간은 일단 2초
    public override void Execute()
    {
        if (guard.CheckEnemyNearby())
        {
            //발견 즉시 추격 상태로 전환
            stateMachine.ChangeState(new GuardChaseState(guard, stateMachine));
            return;
        }

        //랜덤 간격으로만 목적지 고민
        decisionTimer += Time.deltaTime;
        if (decisionTimer > decisionInterval)
        {
            DecideDestination();
            decisionTimer = 0f;
            decisionInterval = Random.Range(0.4f, 0.6f); //다음도 랜덤, 최적화 필요시 리팩토링(삭제)
        }
    }

    private void DecideDestination()
    {
        //GuardManager가 없으면 랜덤 이동
        if (GuardManager.instance == null)
        {
            if (!guard.Agent.pathPending && guard.Agent.remainingDistance < 1.0f)
            {
                RandomMove();
            }
            return;
        }

        //매니저 타겟포스 받기(소음, 지원요청)
        Vector3? targetPos = GuardManager.instance.GetBestTargetPosition(guard.transform.position);

        if (targetPos.HasValue)
        {
            //변경 감지, 목적지가 2m 이상 차이날때만 네비 갱신
            if (Vector3.Distance(targetPos.Value, lastDestination) > 2.0f)
            {
                guard.Agent.speed = guard.patrolSpeed;

                guard.Agent.SetDestination(targetPos.Value);
                lastDestination = targetPos.Value;
            }
        }
        else
        {
            guard.Agent.speed = guard.patrolSpeed;
            if (!guard.Agent.pathPending && guard.Agent.remainingDistance < 1.0f)
            {
                RandomMove();
            }
        }
    }
    private void RandomMove()
    {
        Vector3 randomDir = Random.insideUnitSphere * 10f;
        randomDir += guard.transform.position;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            guard.Agent.SetDestination(hit.position);
            lastDestination = hit.position; //기억 갱신
        }
    }
}
