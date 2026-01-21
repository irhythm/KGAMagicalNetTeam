using UnityEngine;

public enum DirectionType
{
    Forward,
    Backward,
    Right,
    Left
}

public static class SHS_Extensions
{
    /// <summary>
    /// myTrans가 target 기준 원하는 방향, 각도에 있는지 반환
    /// </summary>
    /// <param name="myTrans"> 결과를 구할 트랜스폼 </param>
    /// <param name="target"> 방향의 기준이 되는 타겟 트랜스폼 </param>
    /// <param name="type"> 어디 방향인지 (앞, 뒤) </param>
    /// <param name="angle"> 각도 </param>
    /// <returns> 설정한 방향, 각도에 플레이어가 있으면 true </returns>
    public static bool IsTargetInDirection(this Transform myTrans, Transform target, DirectionType type, float angle)
    {
        if (myTrans == null || target == null) return false;

        Vector3 playerDir = (myTrans.position - target.position);
        playerDir.y = 0;
        playerDir.Normalize();

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

        pivotDir.y = 0;
        pivotDir.Normalize();

        #region 방법 1
        float dot = Vector3.Dot(pivotDir, playerDir);
        dot = Mathf.Clamp(dot, -1f, 1f);

        return dot >= Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);
        #endregion

        #region 방법 2
        //float angleDot = Vector3.Angle(pivotDir, playerDir);

        //return angleDot <= angle * 0.5f || Mathf.Approximately(angleDot, angle * 0.5f);
        #endregion
    }
}
