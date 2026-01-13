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

        else
            return;

        GameManager.Instance.onOpenUI -= CheckDisable;
        GameManager.Instance.onCloseUI -= CheckEnable;
    }

    IEnumerator CheckGameManager()
    {
        yield return new WaitUntil(() => FindAnyObjectByType(typeof(GameManager)));
        
        GameManager.Instance.onOpenUI += CheckDisable;
        GameManager.Instance.onCloseUI += CheckEnable;
    }
    private void CheckEnable()
    {
        if (lookAction != null) lookAction.action.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("촬영 속행");
    }
    private void CheckDisable()
    {
        if (lookAction != null) lookAction.action.Disable();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Debug.Log("촬영 중지");
    }
    

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        currentX = target.eulerAngles.y;
        currentY = 10f; // 약간 위에서 시작
    }

    private void LateUpdate()
    {
        if (!target) return;
        if (lookAction == null) return;

        Vector2 inputDelta = lookAction.action.ReadValue<Vector2>();

        float deltaX = inputDelta.x * sensitivity * Time.deltaTime;
        float deltaY = inputDelta.y * sensitivity * Time.deltaTime;

        InvertMouse(deltaX, deltaY);

        currentY = Mathf.Clamp(currentY, pitchLimits.x, pitchLimits.y);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        Vector3 rotatedOffset = rotation * Offset;

        Vector3 position = target.position + rotatedOffset - (rotation * Vector3.forward * distance);

        transform.rotation = rotation;
        transform.position = position;
    }

    private void InvertMouse(float deltaX, float deltaY)
    {
        if (invertX) currentX -= deltaX;
        else currentX += deltaX;

        if (invertY) currentY += deltaY;
        else currentY -= deltaY;
    }

    public Quaternion GetFlatRotation()
    {
        Vector3 euler = transform.eulerAngles;
        return Quaternion.Euler(0, euler.y, 0);
    }
}