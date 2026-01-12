using UnityEngine;

public class PlayerCarryState : PlayerStateBase
{
    public PlayerCarryState(PlayableCharacter player, StateMachine stateMachine) : base(player, stateMachine, "IsCarry") { }

    public override void FixedExecute()
    {
        base.FixedExecute();
    }

    public override void Execute()
    {
        base.Execute();
    }
}
