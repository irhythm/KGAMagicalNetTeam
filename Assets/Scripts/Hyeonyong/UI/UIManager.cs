using Photon.Voice.PUN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
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
    [SerializeField] Toggle bgmSoundMute;
    [SerializeField] Slider sfxSound;
    [SerializeField] Toggle sfxSoundMute;
    [SerializeField] Slider voiceChatSound;
    public Slider VoiceChatSound => voiceChatSound;
    [SerializeField] Toggle voiceChatSoundMute;
    public Toggle VoiceChatSoundMute => voiceChatSoundMute;
    [SerializeField] Slider micSound;
    public Slider MicSound => micSound;

    [SerializeField] Toggle micSoundMute;
    public Toggle MicSoundMute => micSoundMute;

    [SerializeField] TMP_Dropdown graphicQualityDropDown;
    [SerializeField] Slider mouseSensivitySlider;
    [SerializeField] Toggle checkMouseXInvert;
    [SerializeField] Toggle checkMouseYInvert;
    [SerializeField] TMP_Dropdown languageDropDown;
    [SerializeField] GameObject[] Panels;
    [SerializeField] private InputActionReference escInput;

    [SerializeField] TMP_Dropdown resolutionDropDown;


    public Transform playerInfoPanel;
    public Transform myInfoPanel;
    public Transform IconPanel;
    public TextMeshProUGUI moneyCount;

    [SerializeField] GameObject gameSettingUI;
    bool onGameSettingUI = false;

    public Action onOpenUI;
    public Action onCloseUI;

    [SerializeField] bool onGame=false;
    [SerializeField] GameObject[] settingUIGameObject;

    Dictionary<string, bool> checkUI = new Dictionary<string, bool>();


    [SerializeField] string uiName = "GameMenu";

    private void Awake()
    {
        Instance = this;
        StartCoroutine(LoadData());
        if (escInput == null)
            return;
        escInput.action.Enable();
        escInput.action.performed += ClosePanels;
        escInput.action.performed += OpenUI;
    }
    private void Start()
    {
        AddUI(uiName);

        //if (escInput != null)
        //{
        //    escInput.action.Enable();
        //    escInput.action.performed += OpenUI;
        //}
    }
    private void OnDisable()
    {
        if (escInput != null)
        {
            escInput.action.performed -= ClosePanels;
            escInput.action.performed -= OpenUI;
        }
    }

    public void ClosePanels(InputAction.CallbackContext context)
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

    public void SetPlayerMic()
    {
        
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

    private IEnumerator LoadData()
    {
        yield return new WaitUntil(() => SoundManager.Instance != null);
        if (emailInput != null)
        {
            emailInput.onValueChanged.AddListener((value) => 
            { PlayerPrefsDataManager.LoginId = value; });
            emailInput.text = PlayerPrefsDataManager.LoginId;
            emailInput.ActivateInputField();
        }

        if (bgmSound != null)
        {
            bgmSound.onValueChanged.AddListener((value) => 
            { SoundManager.Instance.SetSoundVolume(Soundtype.BGM, bgmSound.value); });
            bgmSound.value = PlayerPrefsDataManager.BgmVolume;
        }
        if (bgmSoundMute != null)
        {
            bgmSoundMute.onValueChanged.AddListener((isOn) =>
            {
                PlayerPrefsDataManager.BgmVolumeMute = isOn;
            });
            bgmSoundMute.isOn = PlayerPrefsDataManager.BgmVolumeMute;
        }

        if (sfxSound != null)
        {
            sfxSound.onValueChanged.AddListener((value) =>
            { SoundManager.Instance.SetSoundVolume(Soundtype.SFX, sfxSound.value); });
            sfxSound.value = PlayerPrefsDataManager.SFXVolume;
        }
        if (sfxSoundMute != null)
        {
            sfxSoundMute.onValueChanged.AddListener((isOn) =>
            {
                PlayerPrefsDataManager.SFXVolumeMute = isOn;
            });
            sfxSoundMute.isOn = PlayerPrefsDataManager.SFXVolumeMute;
        }

        if (voiceChatSound != null)
        {
            voiceChatSound.onValueChanged.AddListener((value) => { PlayerPrefsDataManager.PlayerVoice = value; });
            voiceChatSound.value = PlayerPrefsDataManager.PlayerVoice;
        }
        if (voiceChatSoundMute != null)
        {
            voiceChatSoundMute.onValueChanged.AddListener((isOn) =>
            {
                PlayerPrefsDataManager.PlayerVoiceMute = isOn;
            });
            voiceChatSoundMute.isOn = PlayerPrefsDataManager.PlayerVoiceMute;
        }

        if (micSound != null)
        {
           
            micSound.onValueChanged.AddListener((value) => { PlayerPrefsDataManager.PlayerMic = value; });
            micSound.value = PlayerPrefsDataManager.PlayerMic;
        }
        if (micSoundMute != null)
        {
            micSoundMute.onValueChanged.AddListener((isOn) =>
            {
                PlayerPrefsDataManager.PlayerMicMute = isOn;
            });
            micSoundMute.isOn = PlayerPrefsDataManager.PlayerMicMute;
        }

        if (graphicQualityDropDown != null)
        {
            graphicQualityDropDown.onValueChanged.AddListener((value) => 
            { PlayerPrefsDataManager.GraphicQuality = value; });
            graphicQualityDropDown.value = PlayerPrefsDataManager.GraphicQuality;
        }

        if (mouseSensivitySlider != null)
        {
            mouseSensivitySlider.onValueChanged.AddListener((value) => 
            { PlayerPrefsDataManager.MouseSensitivity = value; });
            mouseSensivitySlider.value = PlayerPrefsDataManager.MouseSensitivity;
        }

        if (checkMouseXInvert != null)
        {
            checkMouseXInvert.onValueChanged.AddListener((isOn) =>
            {
                PlayerPrefsDataManager.MouseXInvert = isOn;
            });
            checkMouseXInvert.isOn = PlayerPrefsDataManager.MouseXInvert;
        }
        if (checkMouseYInvert != null)
        {
            checkMouseYInvert.onValueChanged.AddListener((isOn) =>
            {
                PlayerPrefsDataManager.MouseYInvert = isOn;
            });
            checkMouseYInvert.isOn = PlayerPrefsDataManager.MouseYInvert;
        }


        if (languageDropDown != null)
        {
            languageDropDown.onValueChanged.AddListener((value) =>
            {
                switch (value)
                {
                    case 0:
                        PlayerPrefsDataManager.Language = "ko";
                        break;
                    case 1:
                        PlayerPrefsDataManager.Language = "en";
                        break;
                    case 2:
                        PlayerPrefsDataManager.Language = "ja";
                        break;
                }
            });
            switch (PlayerPrefsDataManager.Language)
            {
                case "ko":
                    languageDropDown.value = 0;
                    break;
                case "en":
                    languageDropDown.value = 1;
                    break;
                case "ja":
                    languageDropDown.value = 2;
                    break;
            }

        }

        if (resolutionDropDown != null)
        {
            resolutionDropDown.onValueChanged.AddListener((value) => { PlayerPrefs.SetInt("resolution", value); });
            if (PlayerPrefs.HasKey("resolution"))
            {
                resolutionDropDown.value = PlayerPrefs.GetInt("resolution");
                //PlayerPrefs.SetInt("language", languageDropDown.value);
            }
        }
    }

    private void OpenUI(InputAction.CallbackContext context)
    {
        if (gameSettingUI != null)
        {
            if (onGame)
            {
                foreach (GameObject ui in settingUIGameObject)
                {
                    if (ui.activeSelf)
                    {
                        ui.gameObject.SetActive(false);
                        return;
                    }
                }
            }

            Debug.Log("esc 입력");
            onGameSettingUI = !onGameSettingUI;
            gameSettingUI.SetActive(onGameSettingUI);

            if (onGameSettingUI)
            {
                OpenUI(uiName);
            }
            else
            {
                CloseUI(uiName);
            }
        }
    }
    public void OpenUIForUI()
    {
        if (gameSettingUI != null)
        {
            onGameSettingUI = !onGameSettingUI;
            gameSettingUI.SetActive(onGameSettingUI);

            if (onGameSettingUI)
            {
                OpenUI(uiName);
            }
            else
            {
                CloseUI(uiName);
            }
        }
    }
    public void OpenUI(string uiName)
    {
        Debug.Log("UI 열림");
        checkUI[uiName] = true;
        onOpenUI.Invoke();
    }

    public void CloseUI(string name)
    {
        checkUI[name] = false;
        if (!CheckUiClose())
            return;
        Debug.Log("UI 닫힘");
        onCloseUI.Invoke();
    }

    public bool CheckUiClose()
    {
        foreach (var c in checkUI)
        {
            if (c.Value)
            {
                return false;
            }
        }
        return true;
    }
    public void AddUI(string name)
    {
        if (checkUI.ContainsKey(name))
            return;
        checkUI[name] = false;
    }

    public void ExitGame()
    {
        GameManager.Instance.ExitGame();
    }
}
