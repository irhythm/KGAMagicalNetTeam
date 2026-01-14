using UnityEngine;

//AI 상태 뼈대 베이스
public abstract class AIStateBase : IState
{
    //자식 상태들이 공통으로 쓸 변수들
    protected BaseAI ai;                 
    protected StateMachine stateMachine; //상태 관리자
    protected BaseAI.AIStateID stateID;  //이 상태의 네트워크ID

    protected AIStateBase(BaseAI ai, StateMachine stateMachine, BaseAI.AIStateID stateID)
    {
        this.ai = ai;
        this.stateMachine = stateMachine;
        this.stateID = stateID;
    }

    public virtual void Enter()
    {
        //본체에 알리고 애니메이션 변경 및 네트워크 전송 수행 = currentNetworkState 갱신
        ai.ChangeNetworkState(stateID);
    }

    public abstract void Execute();
    public virtual void FixedExecute() { }
    public virtual void Exit() { }
}