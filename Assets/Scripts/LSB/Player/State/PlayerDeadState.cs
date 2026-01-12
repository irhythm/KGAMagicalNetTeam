using UnityEngine;

public class PlayerDeadState : PlayerStateBase
{
    public PlayerDeadState(PlayableCharacter player, StateMachine stateMachine) : base(player, stateMachine, "IsDead") { }

    public override void FixedExecute()
    {
        base.FixedExecute();
    }

    public override void Execute()
    {
        base.Execute();
    }
}
