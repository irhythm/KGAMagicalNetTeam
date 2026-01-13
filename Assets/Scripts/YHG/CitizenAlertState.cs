using UnityEngine;
//공포상태
public class CitizenAlertState : AIStateBase
{
    private CitizenAI citizen; //시민ㄴ변수 접근

    //alert 전달
    public CitizenAlertState(BaseAI ai, StateMachine stateMachine) 
        : base(ai, stateMachine, BaseAI.AIStateID.Alert)
    {
        citizen = ai as CitizenAI;
    }

    public override void Enter()
    {
        base.Enter(); //네트워크/애니동기화

        //멈추고
        citizen.Agent.isStopped = true;

        //룩앳 추가
        if (citizen.detectedPlayer != null)
        {
            citizen.transform.LookAt(citizen.detectedPlayer);
        }
    }
    public override void Exit()
    {
        //액션 가기 전에 멈춤 해제
        citizen.Agent.isStopped = false;
    }
    public override void Execute()
    {
        //플레이어 나가면 다시 패트롤
        if (citizen.detectedPlayer == null)
        {
            stateMachine.ChangeState(new CitizenPatrolState(citizen, stateMachine));
            return;
        }

        //플레이어 거리 계산
        float distSqr = (citizen.transform.position - citizen.detectedPlayer.position).sqrMagnitude;
        //감지 범위의 제곱
        float safeDistSqr = citizen.detectRadius * citizen.detectRadius;

        if (distSqr > safeDistSqr)
        {
            //역돌격 상태 시작
            stateMachine.ChangeState(new CitizenActionState(citizen, stateMachine));
        }
    }
}
