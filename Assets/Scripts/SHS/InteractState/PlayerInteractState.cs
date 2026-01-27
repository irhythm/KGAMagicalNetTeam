using UnityEngine;

public class PlayerInteractState : PlayerStateBase
{
    public IInteractable target { get; private set; }   // 상호작용을 할 상대 타겟

    private InteractionDataSO interactionData;

    public PlayerInteractState(PlayableCharacter player, StateMachine stateMachine, IInteractable target = null) 
        : base(player, stateMachine)
    {
        this.target = target;
    }

    public void SetTarget(IInteractable target) => this.target = target;

    public override void Enter()
    {
        base.Enter();

        player.InputHandler.CanInteractMotion = false;
    }

    public override void Exit()
    {
        base.Exit();

        // 다시 인풋 시스템 가능하게
    }

    public override void Execute()
    {
        base.Execute();
    }

    public override void FixedExecute()
    {
        base.FixedExecute();
    }

    public void Init(InteractionDataSO data)
    {
        interactionData = data;
    }
}
