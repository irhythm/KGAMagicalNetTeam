using UnityEngine;

public class DebuffState : AIStateBase
{
    private DebuffInfo info;          //디버프정보
    private IDebuffBehavior behavior; //전략
    private float timer;

    public DebuffInfo CurrentInfo => info;

    public DebuffState(BaseAI ai, StateMachine stateMachine, DebuffInfo info)
        : base(ai, stateMachine, BaseAI.AIStateID.Debuff)
    {
        this.info = info;
    }

    public override void Enter()
    {
        //네트워크 상태 Debuff
        base.Enter();

        //전략 가져오기
        behavior = DebuffFactory.GetBehavior(info.Type);

        if (behavior != null)
        {
            behavior.OnEnter(ai, info);
        }
        timer = 0f;
    }

    public override void Execute()
    {
        if (behavior != null)
        {
            behavior.OnExecute(ai);
        }

        timer += Time.deltaTime;

        if (timer >= info.Duration)
        {
            //BaseAI가 초기 상태로 복구
            ai.RemoveDebuff(info.Type);
        }
    }
    public override void Exit()
    {
        if (behavior != null)
        {
            behavior.OnExit(ai);
        }
        base.Exit();
    }
}