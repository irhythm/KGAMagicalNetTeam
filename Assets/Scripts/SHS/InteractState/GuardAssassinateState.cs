using UnityEngine;

public class GuardAssassinateState : AIStateBase, IInteractable
{
    private IInteractable target;       // 상호작용을 할 상대 타겟

    private readonly int? animHashNum;

    public bool isInteracted { get; private set; }

    public Transform ActorTrans => ai.transform;

    public GuardAssassinateState(BaseAI ai, StateMachine stateMachine, BaseAI.AIStateID stateID, IInteractable target, string animName)
        : base(ai, stateMachine, stateID)
    {
        this.target = target;
        
        if (!string.IsNullOrEmpty(animName))
        {
            animHashNum = Animator.StringToHash(animName);
        }
        else
        {
            animHashNum = null;
        }
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

    }

    public override void FixedExecute()
    {
        base.FixedExecute();
    }

    public void OnExecuterInteraction()
    {
        
    }

    public void OnReceiverInteraction()
    {

    }

    public void OnStopped()
    {

    }
}
