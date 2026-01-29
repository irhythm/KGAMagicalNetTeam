using UnityEngine;

//처형 로직을 정확히 몰라 초안만
//이동정지 + 런타임 애니메이터 교체
public class ExecutionBehavior : IDebuffBehavior
{
    private float originalSpeed;
    //필요할지모르겠음 어차피 처형인데
    //private RuntimeAnimatorController originalController; // 원래 쓰던 애니메이터 저장용

    public void OnEnter(IDebuffable target, DebuffInfo info)
    {
        //이동 정지
        originalSpeed = target.GetOriginalSpeed();
        target.SetSpeed(0);
        target.SetCanMove(false);

        //애니메이터 교체
        Animator anim = target.GetAnimator();
        if (anim != null && info.ExecutionAnimator != null)
        {
            //originalController = anim.runtimeAnimatorController;
            anim.runtimeAnimatorController = info.ExecutionAnimator;
        }
    }
    public void OnExecute(IDebuffable target)
    {
        //안전장치
        target.SetSpeed(0);
    }

    public void OnExit(IDebuffable target)
    {
        target.SetSpeed(originalSpeed);
        target.SetCanMove(true);
    }
}

