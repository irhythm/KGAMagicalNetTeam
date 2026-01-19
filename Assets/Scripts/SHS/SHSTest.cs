using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SHSTest : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string animParam;

    private void Update()
    {
        if (animator == null) return;

        if(Keyboard.current.gKey.wasPressedThisFrame)
        {
            animator.SetTrigger(animParam);
        }
    }
}
