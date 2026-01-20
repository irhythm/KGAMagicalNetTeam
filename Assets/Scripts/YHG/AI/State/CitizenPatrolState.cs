using UnityEngine;

public class CitizenPatrolState : AIStateBase
{
    private CitizenAI citizen;      
    private bool movingForward = true; //앞뒤전환

    public override void Execute()
    {
        //플레이어가 근처에 있는지 체크
        if (citizen.CheckPlayerNearby())
        {
            //공포상태로 전환
            stateMachine.ChangeState(new CitizenAlertState(citizen, stateMachine));
            return;
        }

        //목적지에 도착했는지 체크
        //네비매쉬 pathPending: 경로 계산 중인가? 트루면 계싼중인거
        //remainingDistance: 남은 거리
        if (!citizen.Agent.pathPending && citizen.Agent.remainingDistance < 0.5f)
        {
            //도착했으면 반대로 뒤집고 다시 이동
            movingForward = !movingForward;
            MoveToNextPoint();
        }
    }
    //BaseAi로 받아서 CitizenAI로 변경하기(citizenAI 변수쓰려고)
    public CitizenPatrolState(BaseAI ai, StateMachine machine): base(ai, machine, BaseAI.AIStateID.Patrol)
    {
        citizen = ai as CitizenAI;
    }

    public override void Enter()
    {
        //ChangeNetworkState 용 필수
        base.Enter(); 

        citizen.Agent.speed = citizen.moveSpeed; //걷는 속도로 설정 
        citizen.Agent.isStopped = false;         //이동 시작 

        MoveToNextPoint(); //전진 후진 시작
    }
    //다음 이동 지점 설정
    private void MoveToNextPoint()
    {
        //CitizenAI 본체에게 순찰 지점 좌표 요청
        Vector3 target = citizen.GetPatrolPoint(movingForward);

        //네비게이션 에이전트에게 이동 명령
        citizen.Agent.SetDestination(target);
    }
}
