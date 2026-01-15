using UnityEngine;

public class PlayerActionState : PlayerStateBase
{
    public PlayerActionState(PlayableCharacter player, StateMachine stateMachine)
        : base(player, stateMachine) { }

    public override void Enter()
    {
        base.Enter();

        if(player.InputHandler.AttackLeftTriggered)
        {

        }
        else if(player.InputHandler.AttackRightTriggered)
        {

        }
    }

    public override void FixedExecute()
    {
        base.FixedExecute();
    }

    public override void Execute()
    {
        base.Execute();
    }

    public void AttackStateChange()
    {
        stateMachine.ChangeState(player.MoveState);
    }
}
