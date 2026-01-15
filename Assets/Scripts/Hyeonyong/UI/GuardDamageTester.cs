using UnityEngine;
using Photon.Pun;

public class GuardDamageTester : MonoBehaviour
{
    private GuardAI guard;

    void Start()
    {
        guard = GetComponent<GuardAI>();
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (guard.targetPlayer != null)
            {
                Debug.Log($"T키 입력 -> InflictDamage() 강제 호출");
                guard.InflictDamage();
            }
            else
            {
                Debug.Log("타겟 플레이어가 없음");
            }
        }
    }
}