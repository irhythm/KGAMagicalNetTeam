using UnityEngine;

//스턴, 빙결?
public class StunBehavior : IDebuffBehavior
{
    private float originalSpeed;
    private GameObject stunEffect; //필요 시

    public void OnEnter(IDebuffable target, DebuffInfo info)
    {
        originalSpeed = target.GetOriginalSpeed();
        target.SetSpeed(0);
        target.SetCanMove(false);

        if (target.GetAnimator() != null)
        {
            target.GetAnimator().speed = 0; // 멈춤
        }

        if (info.VisualPrefab != null)
        {
            stunEffect = Object.Instantiate(info.VisualPrefab, target.GetCenterPosition());
        }

    }

    public void OnExecute(IDebuffable target)
    {
        //딱히 없을 듯
    }

    public void OnExit(IDebuffable target)
    {
        //원복
        target.SetSpeed(originalSpeed);
        target.SetCanMove(true);

        if (target.GetAnimator() != null)
        {
            target.GetAnimator().speed = 1;
        }

        if (stunEffect != null)
        {
            Object.Destroy(stunEffect);
        }
    }
}
