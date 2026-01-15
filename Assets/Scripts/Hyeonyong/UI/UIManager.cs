using Photon.Voice.PUN;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] Slider bgmSound;
    [SerializeField] Slider effectSound;
    [SerializeField] Slider voiceChatSound;
    [SerializeField] TMP_Dropdown graphicQualityDropDown;
    [SerializeField] Slider mouseSensivitySlider;
    [SerializeField] Toggle checkXYInversion;
    [SerializeField] TMP_Dropdown languageDropDown;
    [SerializeField] GameObject[] Panels;
    [SerializeField] private InputActionReference escInput;

    [SerializeField] TMP_Dropdown resolutionDropDown;


    public Transform playerInfoPanel;
    public Transform myInfoPanel;
    private void Awake()
    {
        Instance = this;
        LoadData();
        if(escInput==null)
            return;
        escInput.action.Enable();
        escInput.action.performed += ClosePanels;
    }
    private void OnDisable()
    {
        escInput.action.performed -= ClosePanels;
    }

    private void ClosePanels(InputAction.CallbackContext context)
    {
        if (Panels.Length <= 0)
        {
            return;
        }

        foreach (var panel in Panels)
        {
            if (panel.activeSelf)
            {
                panel.SetActive(false);
            }
        }
    }
    


    //public void SetVoiceChatSound(AudioSource playerVoiceSound)
    //{
    //    voiceChatSound = playerVoiceSound;
    //    LoadVoiceChatSound();
    //}

    //public void LoadVoiceChatSound()
    //{
    //    if (voiceChatSound != null && PlayerPrefs.HasKey("VoiceChatVolume"))
    //    {
    //        voiceChatSound.volume = PlayerPrefs.GetFloat("VoiceChatVolume");
    //    }
    //}
    private void LoadData()
    {
        if (emailInput != null)
        {
            if (PlayerPrefs.HasKey("Email"))
            {
                emailInput.text = PlayerPrefs.GetString("Email");
            }
            emailInput.onValueChanged.AddListener((value) => { PlayerPrefs.SetString("Email", value); });
        }

        if (bgmSound != null)
        {
            if (PlayerPrefs.HasKey("BGMVolume"))
            {
                bgmSound.value = PlayerPrefs.GetFloat("BGMVolume");
            }
            bgmSound.onValueChanged.AddListener((value) => { PlayerPrefs.SetFloat("BGMVolume", value); });
        }

        if (effectSound != null)
        {
            if (PlayerPrefs.HasKey("EffectVolume"))
            {
                effectSound.value = PlayerPrefs.GetFloat("EffectVolume");
                //PlayerPrefs.SetFloat("EffectVolume", effectSound.value);
            }
            effectSound.onValueChanged.AddListener((value) => { PlayerPrefs.SetFloat("EffectVolume", value); });
        }

        if (voiceChatSound != null)
        {
            if (PlayerPrefs.HasKey("VoiceChatVolume"))
            {
                voiceChatSound.value = PlayerPrefs.GetFloat("VoiceChatVolume");
                //PlayerPrefs.SetFloat("VoiceChatVolume", voiceChatSound.value);
            }
            voiceChatSound.onValueChanged.AddListener((value) => { PlayerPrefs.SetFloat("VoiceChatVolume", value); });
        }

        if (graphicQualityDropDown != null)
        {
            if (PlayerPrefs.HasKey("GraphicQuality"))
            {
                graphicQualityDropDown.value = PlayerPrefs.GetInt("GraphicQuality");
                //PlayerPrefs.SetInt("GraphicQuality", graphicQualityDropDown.value);
            }
            graphicQualityDropDown.onValueChanged.AddListener((value) => { PlayerPrefs.SetInt("GraphicQuality", value); });
        }

        if (mouseSensivitySlider != null)
        {
            if (PlayerPrefs.HasKey("MouseSensivity"))
            {
                mouseSensivitySlider.value = PlayerPrefs.GetFloat("MouseSensivity");
                mouseSensivitySlider.onValueChanged.AddListener((value) => { PlayerPrefs.SetFloat("MouseSensivity", value); });
                //PlayerPrefs.SetFloat("MouseSensivity", mouseSensivitySlider.value);
            }
        }
        if (checkXYInversion != null)
        {
            if (PlayerPrefs.HasKey("XYInversion"))
            {
                checkXYInversion.isOn = PlayerPrefs.GetInt("XYInversion") == 1 ? true : false;

                //int num = checkXYInversion.isOn == true ? 1 : 0;
                //PlayerPrefs.SetInt("XYInversion", num);
            }
            checkXYInversion.onValueChanged.AddListener((isOn) =>
            {
                int num = checkXYInversion.isOn == true ? 1 : 0;
                PlayerPrefs.SetInt("XYInversion", num);
            });
        }


        if (languageDropDown != null)
        {
            if (PlayerPrefs.HasKey("language"))
            {
                languageDropDown.value = PlayerPrefs.GetInt("language");
                //PlayerPrefs.SetInt("language", languageDropDown.value);
            }
            languageDropDown.onValueChanged.AddListener((value) => { PlayerPrefs.SetInt("language", value); });
        }

        if (resolutionDropDown != null)
        {
            if (PlayerPrefs.HasKey("resolution"))
            {
                resolutionDropDown.value = PlayerPrefs.GetInt("resolution");
                //PlayerPrefs.SetInt("language", languageDropDown.value);
            }
            resolutionDropDown.onValueChanged.AddListener((value) => { PlayerPrefs.SetInt("resolution", value); });
        }
    }

    public void SaveData()
    {
        if (emailInput != null)
        {
            PlayerPrefs.SetString("Email", emailInput.text);
        }
        Debug.Log("email");

        if (bgmSound != null)
        {
            PlayerPrefs.SetFloat("BGMVolume", bgmSound.value);
        }
        Debug.Log("BGM");
        if (effectSound != null)
        {
            PlayerPrefs.SetFloat("EffectVolume", effectSound.value);
        }
        Debug.Log("이펙트");
        Debug.Log("Voice");
        if (voiceChatSound != null)
        {
            PlayerPrefs.SetFloat("VoiceChatVolume", voiceChatSound.value);
        }
        Debug.Log("퀄리티");
        if (graphicQualityDropDown != null)
        {
            Debug.Log("저장하는 값 : " + graphicQualityDropDown.value);
            PlayerPrefs.SetInt("GraphicQuality", graphicQualityDropDown.value);
        }
        Debug.Log("감도");
        if (mouseSensivitySlider != null)
        {
            PlayerPrefs.SetFloat("MouseSensivity", mouseSensivitySlider.value);
        }

        Debug.Log("좌우 반전");
        if (checkXYInversion != null)
        {
            int num = checkXYInversion.isOn == true ? 1 : 0;
            PlayerPrefs.SetInt("XYInversion", num);
        }
        Debug.Log("언어");
        if (languageDropDown != null)
        {
            PlayerPrefs.SetInt("language", languageDropDown.value);
        }

        //다른 값들이 먼저 지워져서 문제인 것으로 추정
    }
}
