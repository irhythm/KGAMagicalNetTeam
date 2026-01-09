using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimatorManager : MonoBehaviourPun
{
    Animator animator;
    float directionDampTime = 0.25f;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {

        if (!photonView.IsMine)
        {
            return;
        }

        float h = 0f;
        float v= 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            h -= 1f;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            h += 1f;
        }

        if (Keyboard.current.wKey.isPressed)
        {
            v += 1f;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            v -= 1f;
        }

        //후진 막는 코드
        if (v < 0)
            v = 0;
        

        animator.SetFloat("Speed", h * h + v * v);
        animator.SetFloat("Direction",h,directionDampTime,Time.deltaTime);
    }
}
