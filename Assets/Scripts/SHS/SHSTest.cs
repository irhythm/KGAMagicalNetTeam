using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SHSTest : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string animParam;

    [SerializeField] private Slider slider;

    private void Update()
    {
        if (animator == null) return;

        if(Keyboard.current.gKey.wasPressedThisFrame)
        {
            animator.SetTrigger(animParam);
        }
    }

    public void OnValueChanged_VolumeSlider()
    {
        SoundManager.Instance.SetSoundVolume(Soundtype.BGM, slider.value);
    }
}
