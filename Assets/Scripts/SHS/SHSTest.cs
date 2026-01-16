using UnityEngine;
using UnityEngine.InputSystem;

public class SHSTest : MonoBehaviour
{
    private void Update()
    {
        if(Keyboard.current.fKey.wasPressedThisFrame)
            Loading.Instance.LoadScene("Title");
    }
}
