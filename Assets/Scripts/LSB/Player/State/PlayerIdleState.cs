using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public PlayerIdleState(PlayableCharacter player, StateMachine stateMachine) : base(player, stateMachine, "IsIdle") { }

    public override void FixedExecute()
    {
        base.FixedExecute();

        player.Rigidbody.linearVelocity = Vector3.zero;
    }

    public override void Execute()
    {
        base.Execute();

        if (player.InputHandler.MoveInput.sqrMagnitude > 0.01f)
        {
            stateMachine.ChangeState(player.MoveState);
        }
    }
}
