using UnityEngine;
using UnityEngine.InputSystem;
public class SimpleRagdollTester : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private HumanoidRagdollController targetController;

    [SerializeField] private float pushPower = 50f;

    private void Start()
    {
        if (targetController == null)
        {
            targetController = GetComponent<HumanoidRagdollController>();
        }
    }

    private void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (targetController != null)
            {
                Debug.Log("T키 감지");
                TriggerFakeHit();
            }
            else
            {
                Debug.LogError("HumanoidRagdollController가 없읆,,");
            }
        }
    }
    private void TriggerFakeHit()
    {
        //방향 뒤, 위
        Vector3 backwardDir = -transform.forward + Vector3.up * 0.5f;

        //힘 * 방향
        Vector3 finalForce = backwardDir * pushPower;

        //전달
        targetController.ApplyRagdoll(finalForce);
    }


}
