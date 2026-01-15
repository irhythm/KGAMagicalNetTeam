using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("인풋 액션")]
    [SerializeField] private InputActionReference lookAction;

    [Header("추적 대상")]
    [SerializeField] private Transform target; // 추적할 플레이어

    [Header("위치 설정")]
    [SerializeField] private float distance = 5.0f;
    [SerializeField] private Vector2 pitchLimits = new Vector2(-20, 80);

    [Header("오프셋 설정")]
    [SerializeField] private Vector3 Offset = new Vector3(1f, 1.5f, 0f);

    [Header("감도 설정")]
    [SerializeField] private float sensitivity = 50.0f;
    public float Sensitivity
    {
        get => sensitivity;
        set => sensitivity = value;
    }

    [SerializeField] private bool invertX = false;
    public bool InvertX
    {
        get => invertX;
        set => invertX = value;
    }

    [SerializeField] private bool invertY = false;
    public bool InvertY
    {
        get => invertY;
        set => invertY = value;
    }

    private float currentX = 0.0f;
    private float currentY = 0.0f;

    public Transform CameraTransform { get; private set; }

    private void Awake()
    {
        CameraTransform = this.transform;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(CheckGameManager());
    }

    private void OnEnable()
    {
        if (lookAction != null) lookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (lookAction != null) lookAction.action.Disable();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onOpenUI -= CheckDisable;
            GameManager.Instance.onCloseUI -= CheckEnable;
        }
    }

    IEnumerator CheckGameManager()
    {
        yield return new WaitUntil(() => FindAnyObjectByType(typeof(GameManager)));
        
        GameManager.Instance.onOpenUI += CheckDisable;
        GameManager.Instance.onCloseUI += CheckEnable;
    }
    private void CheckEnable() => SetControl(true);
    private void CheckDisable() => SetControl(false);

    /// <summary>
    /// 플레이어 시점제어 인풋 활성 및 비활성화 커서 상태도 변화
    /// </summary>
    /// <remarks>
    /// 시점 제어 기능이 활성화되면 커서가 잠기고 숨겨져 카메라 이동이 중단되지 않습니다.
    /// 시점 제어 기능을 비활성화하면 커서가 잠금 해제되고 표시되어 사용자가 UI 요소와 상호 작용할 수 있습니다.
    /// </remarks>
    /// <param name="isActive">true로 설정하면 시점 제어 기능이 활성화되고 커서가 잠기며, false로 설정하면 시점 제어 기능이 비활성화되고 커서가 잠금 해제됩니다.</param>
    public void SetControl(bool isActive)
    {
        if (lookAction == null || lookAction.action == null) return;

        if (isActive)
        {
            lookAction.action.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            lookAction.action.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// 카메라가 볼 타겟 설정
    /// </summary>
    /// <param name="newTarget">새로운 플레이어</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        currentX = target.eulerAngles.y;
        currentY = 10f;
    }

    private void LateUpdate()
    {
        if (!target) return;
        if (lookAction == null) return;

        // 마우스 움직임 값
        Vector2 inputDelta = lookAction.action.ReadValue<Vector2>();

        // 마우스 감도 값 반영
        float deltaX = inputDelta.x * sensitivity * Time.deltaTime;
        float deltaY = inputDelta.y * sensitivity * Time.deltaTime;

        // 반전 옵션 체크
        InvertMouse(deltaX, deltaY);

        // 최소, 최대 y값 설정
        currentY = Mathf.Clamp(currentY, pitchLimits.x, pitchLimits.y);

        // 쿼터니언 회전값 계산
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // 오프셋을 적용한 회전값 계산
        Vector3 rotatedOffset = rotation * Offset;

        // 포지션 설정 거리랑 각도 위치 오프셋 반영
        Vector3 position = target.position + rotatedOffset - (rotation * Vector3.forward * distance);

        // 각도 포지션 반영
        transform.rotation = rotation;
        transform.position = position;
    }

    /// <summary>
    /// 마우스 x, y 반전옵션 체크용
    /// </summary>
    /// <param name="deltaX"></param>
    /// <param name="deltaY"></param>
    private void InvertMouse(float deltaX, float deltaY)
    {
        if (invertX) currentX -= deltaX;
        else currentX += deltaX;

        if (invertY) currentY += deltaY;
        else currentY -= deltaY;
    }
}