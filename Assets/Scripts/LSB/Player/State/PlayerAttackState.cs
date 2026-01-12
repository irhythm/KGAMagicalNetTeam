using UnityEngine;

public class PlayerAttackState : PlayerStateBase
{
    public PlayerAttackState(PlayableCharacter player, StateMachine stateMachine) : base(player, stateMachine, "IsAttack") { }

    public override void FixedExecute()
    {
        base.FixedExecute();
    }

    public override void Execute()
    {
        base.Execute();
    }
}
