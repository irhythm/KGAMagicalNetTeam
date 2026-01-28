using UnityEngine;

public class StateMachine
{
    public IState CurrentState { get; private set; }

    public void InitState(IState initState)
    {
        CurrentState = initState;
        CurrentState.Enter();
    }

    public void ChangeState(IState newState)
    {
        if (CurrentState == newState) return; // 같은 상태로 전환 방지
        //상태가있으면 가져오기, 없으면 None
        string prevStateName = CurrentState != null ? CurrentState.GetType().Name : "None";

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
