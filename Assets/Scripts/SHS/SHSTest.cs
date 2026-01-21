using UnityEngine;
using UnityEngine.InputSystem;

public class SHSTest : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string animParam;

    [SerializeField] private Transform target;

    private void Update()
    {
        // if (animator == null) return;

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            // animator.SetTrigger(animParam);
            if (IsTargetInDirection(transform, target, DirectionType.Right, 120f))
                Debug.Log("오른쪽에 있음");
            else
                Debug.Log("false");
        }
    }
    public static bool IsTargetInDirection(Transform myTrans, Transform target, DirectionType type, float angle)
    {
        if (target == null) return false;

        Vector3 playerDir = (myTrans.position - target.position).normalized;
        Vector3 pivotDir = target.forward;

        switch (type)
        {
            case DirectionType.Forward:
                pivotDir = target.forward;
                break;
            case DirectionType.Backward:
                pivotDir = -target.forward;
                break;
            case DirectionType.Right:
                pivotDir = target.right;
                break;
            case DirectionType.Left:
                pivotDir = -target.right;
                break;
        }

        float angleDot = Vector3.Angle(pivotDir, playerDir);

        return angleDot <= angle * 0.5f || Mathf.Approximately(angleDot, angle * 0.5f);
    }
}
