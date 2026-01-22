using UnityEngine;

public class PlayerAssassinateState : PlayerStateBase, IInteractable
{
    private IInteractable target;   // 상호작용을 할 상대 타겟
    private AssassinateInteract assassinateInteract;

    public PlayerAssassinateState(PlayableCharacter player, StateMachine stateMachine, string animationNum, IInteractable target) 
        : base(player, stateMachine, animationNum)
    {
        this.target = target;
    }

    public override void Enter()
    {
        base.Enter();

        // 암살 상호작용 클래스 생성
        if (assassinateInteract == null)
            assassinateInteract = new AssassinateInteract(this, target);
        else
            assassinateInteract.Init(this, target);
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
