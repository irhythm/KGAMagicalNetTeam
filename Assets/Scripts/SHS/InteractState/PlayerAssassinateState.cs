using UnityEngine;

public class PlayerAssassinateState : PlayerStateBase, IInteractable
{
    private IInteractable target;   // 상호작용을 할 상대 타겟

    public PlayerAssassinateState(PlayableCharacter player, StateMachine stateMachine, string animationNum, IInteractable target) 
        : base(player, stateMachine, animationNum)
    {
        this.target = target;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Execute()
    {
        base.Execute();
    }

    public override void FixedExecute()
    {
        base.FixedExecute();
    }

    public void OnInteract()
    {

    }
}
