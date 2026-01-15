using UnityEngine;

public abstract class PlayerStateBase : IState
{
    protected PlayableCharacter player;
    protected StateMachine stateMachine;
    protected int animationNum;

    protected PlayerStateBase(PlayableCharacter player, StateMachine stateMachine, string animationNum = null)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        if(animationNum != null )
            this.animationNum = Animator.StringToHash(animationNum);
        else
            this.animationNum = 0;
    }

    public virtual void Enter()
    {
        if(animationNum != 0)
            player.Animator.SetBool(animationNum, true);
    }

    public virtual void Exit()
    {
        if (animationNum != 0)
            player.Animator.SetBool(animationNum, false);
    }

    public virtual void Execute() { }
    public virtual void FixedExecute(){}
}
