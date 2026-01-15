using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class GuardDamageTester : MonoBehaviour
{
    private GuardAI guard;
    private InputAction testAction;

    private void Awake()
    {
        guard = GetComponent<GuardAI>();

        //t키 바인딩
        testAction = new InputAction(binding: "<Keyboard>/t");
    }

    void Start()
    {
        guard = GetComponent<GuardAI>();
    }

    private void OnEnable()
    {
        testAction.Enable(); 
        testAction.performed += OnTestInput;
    }
    private void OnTestInput(InputAction.CallbackContext ctx)
    {
        if (guard != null && guard.targetPlayer != null)
        {
            Debug.Log($"T 입력 -> InflictDamage() 강제 호출");
            guard.InflictDamage();
        }
        else
        {
            Debug.Log("GuardAI를 찾을 수 없음");
        }
    }
}