using Photon.Pun;
using UnityEngine;

public class PlayerCameraSetup : MonoBehaviourPunCallbacks
{
    public ThirdPersonCamera cameraScript;

    private void Start()
    {
        // 내 캐릭터만 찾아서 연결함
        if (photonView.IsMine)
        {
            cameraScript = FindAnyObjectByType<ThirdPersonCamera>();

            if (cameraScript != null)
            {
                cameraScript.SetTarget(transform);
            }
            else
            {
                Debug.LogError("카메라 없음");
            }
        }
    }
}
