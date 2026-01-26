using UnityEngine;

//주둔지 경비병이 구역을 벗어날 경우의 상태, 무적 + 고속 복귀
public class GarrisonReturnState : AIStateBase
{
    private GarrisonGuardAI garrison;

    //복귀 속도 배율
    private float returnSpeedMultiplier = 1.5f;

    private float detectTimer = 0f;

    public GarrisonReturnState(BaseAI ai, StateMachine machine) 
        : base(ai, machine, BaseAI.AIStateID.Chase)
    {
        garrison = ai as GarrisonGuardAI;
    }

    public override void Enter()
    {
        base.Enter();
        if (garrison == null) return;

        //이동 설정, 속도 높이고 목적지 지정
        if (garrison.Agent != null && garrison.Agent.isOnNavMesh)
        {
            garrison.Agent.isStopped = false;
            garrison.Agent.updateRotation = true;
            garrison.Agent.updatePosition = true;

            //배속
            garrison.Agent.speed = garrison.runSpeed * returnSpeedMultiplier;

            //스폰포인트로
            garrison.Agent.SetDestination(garrison.InitialSpawnPos);
        }
    }

    public override void Execute()
    {
        if (garrison == null) return;

        if (!garrison.Agent.pathPending && garrison.Agent.remainingDistance <= 1.0f)
        {
            stateMachine.ChangeState(new GarrisonPatrolState(garrison, stateMachine));
            return;
        }

        detectTimer += Time.deltaTime;
        if (detectTimer >= 0.5f)
        {
            detectTimer = 0f;
            
            //돌아가다가 시야 내 적 발견하면???
            if (garrison.CheckEnemyNearby())
            {
                //주둔지 내부인지 체크, 아니면 추적 실패(탈출한놈) 계속 봄.,,
                float distToCenter = Vector3.Distance(garrison.garrisonCenter.position, garrison.targetPlayer.position);
                if (distToCenter <= garrison.maxChaseDist)
                {
                    //걔 추격
                    stateMachine.ChangeState(new GarrisonChaseState(garrison, stateMachine));
                    return;
                }
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (garrison != null)
        {
            if (garrison.Agent != null)
            {
                garrison.Agent.speed = garrison.patrolSpeed;
            }
        }
    }


}
