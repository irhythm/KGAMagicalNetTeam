using UnityEngine;

public interface IPhysicsObject
{
    // 상태 이상 적용/해제
    void OnStatusChange(bool isControlled);

    // 외부 힘 적용
    void OnApplyExternalForce(Vector3 forceDirection, float forcePower, ForceMode mode);
}