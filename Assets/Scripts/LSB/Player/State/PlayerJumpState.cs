using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    public PlayerJumpState(PlayableCharacter player, StateMachine stateMachine) : base(player, stateMachine, "IsJump") { }

    public override void FixedExecute()
    {
        base.FixedExecute();
    }

    public override void Execute()
    {
        base.Execute();
    }
}
