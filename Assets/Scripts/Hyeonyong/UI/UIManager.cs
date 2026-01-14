using Photon.Voice.PUN;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    /// <summary>
    /// 저장해야할 값
    /// 1. ID만 자동 로그인으로 불러오기 (string) Key : Email
    /// 2. 배경음 볼륨(float) Key : BGMVolume
    /// 3. 효과음 볼륨(float) Key : EffectVolume
    /// 4. 음성대화 볼륨(float) Key : VoiceChatVolume
    /// 5. 그래픽 품질(int)(enum) Key : GraphicQuality
    /// 6. 마우스 감도(float) Key : MouseSensivity
    /// 7. XY 반전 여무(bool) Key : XYInversion
    /// 8. 언어 변경(int)(enum) : Language
    /// </summary>

    [SerializeField] TMP_InputField emailInput;
    [SerializeField] AudioSource bgmSound;
    [SerializeField] AudioSource effectSound;
    [SerializeField] AudioSource voiceChatSound;
    [SerializeField] TMP_Dropdown graphicQualityDropDown;
    [SerializeField] Slider mouseSensivitySlider;
    [SerializeField] Toggle checkXYInversion;
    [SerializeField] TMP_Dropdown languageDropDown;

    private void Awake()
    {
        Instance = this;
        LoadData();
    }

    private void OnDisable()
    {
        SaveData();
    }

    public void SetVoiceChatSound(AudioSource playerVoiceSound)
    {
        voiceChatSound = playerVoiceSound;
        LoadVoiceChatSound();
    }

    public void LoadVoiceChatSound()
    {
        if (voiceChatSound != null && PlayerPrefs.HasKey("VoiceChatVolume"))
        {
            voiceChatSound.volume = PlayerPrefs.GetFloat("VoiceChatVolume");
        }
    }
    private void LoadData()
    {
        if (emailInput != null && PlayerPrefs.HasKey("Email"))
        {
            emailInput.text = PlayerPrefs.GetString("Email");
        }

        if (bgmSound != null && PlayerPrefs.HasKey("BGMVolume"))
        {
            bgmSound.volume = PlayerPrefs.GetFloat("BGMVolume");
        }

        if (effectSound != null && PlayerPrefs.HasKey("EffectVolume"))
        {
            effectSound.volume = PlayerPrefs.GetFloat("EffectVolume");
        }

        if (voiceChatSound != null && PlayerPrefs.HasKey("VoiceChatVolume"))
        {
            voiceChatSound.volume = PlayerPrefs.GetFloat("VoiceChatVolume");
        }

        if (graphicQualityDropDown != null && PlayerPrefs.HasKey("GraphicQuality"))
        {
            graphicQualityDropDown.value = PlayerPrefs.GetInt("GraphicQulity");
        }

        if (mouseSensivitySlider != null && PlayerPrefs.HasKey("MouseSensivity"))
        {
            mouseSensivitySlider.value = PlayerPrefs.GetFloat("MouseSensivity");
        }
        if (checkXYInversion != null && PlayerPrefs.HasKey("XYInversion"))
        {
            checkXYInversion.isOn = PlayerPrefs.GetInt("XYInversion") == 1 ? true : false;
        }
        if (languageDropDown != null && PlayerPrefs.HasKey("language"))
        {
            languageDropDown.value = PlayerPrefs.GetInt("language");
        }
    }

    public void SaveData()
    {
        if (emailInput != null)
        {
            PlayerPrefs.SetString("Email", emailInput.text);
        }

        if (bgmSound != null)
        {
            PlayerPrefs.SetFloat("BGMVolume", bgmSound.volume);
        }

        if (effectSound != null)
        {
            PlayerPrefs.SetFloat("EffectVolume", effectSound.volume);
        }

        if (voiceChatSound != null)
        {
            PlayerPrefs.SetFloat("VoiceChatVolume", voiceChatSound.volume);
        }

        if (graphicQualityDropDown != null)
        {
            PlayerPrefs.SetInt("GraphicQulity", graphicQualityDropDown.value);
        }

        if (mouseSensivitySlider != null)
        {
            PlayerPrefs.SetFloat("MouseSensivity", mouseSensivitySlider.value);
        }
        if (checkXYInversion != null)
        {
            int num = checkXYInversion.isOn == true ? 1 : 0;
            PlayerPrefs.SetInt("XYInversion", num);
        }
        if (languageDropDown != null)
        {
            PlayerPrefs.SetInt("language", languageDropDown.value);
        }
    }
}
