using UnityEngine;

public class PlayerInteractState : PlayerStateBase
{
    public IInteractable target { get; private set; }   // 상호작용을 할 상대 타겟

    public PlayerInteractState(PlayableCharacter player, StateMachine stateMachine, IInteractable target = null) 
        : base(player, stateMachine)
    {
        this.target = target;
    }

    public void SetTarget(IInteractable target) => this.target = target;

    public override void Enter()
    {
        base.Enter();

        // 상호작용을 시작했으므로 더 이상 못하게 막음
        player.InputHandler.CanInteractMotion = false;
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
}
