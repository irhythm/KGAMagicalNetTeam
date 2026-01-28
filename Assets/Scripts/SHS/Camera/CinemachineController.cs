using Unity.Cinemachine;
using UnityEngine;

public enum Cinemachinetype
{
    CutScene,       // 컷씬, 연출
    InGame          // 인게임, 3인칭
}

/// <summary>
/// 시네머신 카메라를 관리하기 위한 클래스
/// 카메라 전환, 용도에 맞는 카메라 우선순위 변경 등을 담당
/// 연출용 시네머신 카메라는 오직 하나만 사용
/// </summary>
public class CinemachineController : MonoBehaviour
{
    private const int UsedCameraPriority = 5;
    private const int UnusedCameraPriority = 0;

    [field: SerializeField] public CinemachineCamera cutSceneCamera { get; private set; }      // 연출용 시네머신 카메라
    [field: SerializeField] public CinemachineCamera playerCamera { get; private set; }        // 평소에 플레이어를 따라다니는 3인칭 카메라
    private void Start()
    {
        ProjectManager.Instance.CinemachineControl = this;
    }
    
    public void SetCameraType(Cinemachinetype type)
    {
        if (cutSceneCamera == null || playerCamera == null) return;

        switch(type)
        {
            case Cinemachinetype.CutScene:
                cutSceneCamera.Priority = UsedCameraPriority;
                playerCamera.Priority = UnusedCameraPriority;
                break;
            case Cinemachinetype.InGame:
                cutSceneCamera.Priority = UnusedCameraPriority;
                playerCamera.Priority = UsedCameraPriority;
                break;
        }
    }

    private void OnDisable()
    {
        ProjectManager.Instance.CinemachineControl = null;
    }
}
