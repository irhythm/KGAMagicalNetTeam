using UnityEngine;
using UnityEngine.InputSystem;

public class TestRagdollKiller : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            KillNearbyGuards();
        }
    }

    void KillNearbyGuards()
    {
        // 씬에 있는 모든 경비병 찾기
        GuardAI guard = FindAnyObjectByType<GuardAI>();

        guard.TakeDamage(100);

    }
}