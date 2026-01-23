using UnityEngine;

public class CitizenActionState : AIStateBase
{
    private CitizenAI citizen;

    public CitizenActionState(BaseAI ai, StateMachine machine)
        : base(ai, machine, BaseAI.AIStateID.Action)
    {
        citizen = ai as CitizenAI;
    }

    public override void Enter()
    {
        base.Enter(); //동기화

        //도망칠 준비
        citizen.Agent.isStopped = false;        //혹시모르니 한번 더
        citizen.Agent.speed = citizen.runSpeed; //속도 변경

        //목적지 설정
        Vector3 exitPos = citizen.GetNearestExit();
        citizen.Agent.SetDestination(exitPos);

        Debug.Log($"시민 {citizen.name} 도주 시작");
    }

    public override void Execute()
    {
        if (citizen.Agent == null || !citizen.Agent.isActiveAndEnabled || !citizen.Agent.isOnNavMesh)
        {
            return;
        }
        //경로계산 끝났고, 남은거리 1 이하면 탈출성공
        if (!citizen.Agent.pathPending && citizen.Agent.remainingDistance < 1.0f)
        {
            citizen.OnEscapeSuccess();
        }
    }
}
