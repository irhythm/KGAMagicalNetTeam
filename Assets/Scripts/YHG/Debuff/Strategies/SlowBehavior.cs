using UnityEngine;

//둔화
public class SlowBehavior : IDebuffBehavior
{
    private float originalSpeed;

    public void OnEnter(IDebuffable target, DebuffInfo info)
    {
        originalSpeed = target.GetOriginalSpeed();

        //0~1f
        float slowRatio = Mathf.Clamp01(1.0f - info.Value);
        target.SetSpeed(originalSpeed * slowRatio);
    }

    public void OnExecute(IDebuffable target) { }

    public void OnExit(IDebuffable target)
    {
        //속도 복구
        target.SetSpeed(originalSpeed);
    }
}