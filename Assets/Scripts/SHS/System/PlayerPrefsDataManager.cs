using UnityEngine;

/// <summary>
/// PlayerPrefs로 데이터 저장
/// 유저 아이디, 언어 설정, 음악 볼륨 설정, 기타 볼륨 설정, 품질 설정, 해상도 수치
/// </summary>
public static class PlayerPrefsDataManager
{
    #region PlayerPrefs Key
    public const string KEY_LOGINID = "LoginId";                    // 로그인 아이디 키
    public const string KEY_LANGUAGE = "Language";                  // 언어 설정 키
    public const string KEY_BGMVOLUME = "BgmVolume";                // BGM 볼륨 키
    public const string KEY_SFXVOLUME = "SfxVolume";                // SFX 볼륨 키
    public const string KEY_GRAPHICQUALITY = "GraphicQuality";      // 그래픽 품질 설정 키
    public const string KEY_RESOLUTIONWIDTH = "ResolutionWidth";    // 가로 해상도 수치 키
    public const string KEY_RESOLUTIONHEIGHT = "ResolutionHeight";  // 세로 해상도 수치 키
    public const string KEY_RESOLUTIONWINDOW = "ResolutionWindow";  // 세로 해상도 수치 키
    public const string KEY_RESOLUTIONHZ = "ResolutionHz";  // 주사율
    public const string KEY_MouseSensivity = "MouseSensitivity";  // 마우스 감도 키
    public const string KEY_MouseXInvert= "MouseXInvert";  // 마우스 X 반전
    public const string KEY_MouseYInvert= "MouseYInvert";  // 마우스 Y 반전
    public const string KEY_PlayerVoice= "PlayerVoice";  // 음성 채팅 사운드
    public const string KEY_PlayerMic= "PlayerMic";  // 음성 채팅 사운드

    public const string KEY_BGMVOLUME_MUTE = "BGMVolumeMute";
    public const string KEY_SFXVOLUME_MUTE = "SFXVolumeMute";
    public const string KEY_PlayerVoice_MUTE = "PlayerVoiceMute";
    public const string KEY_PlayerMic_MUTE = "PlayerMicMute";
    
    #endregion

    public static string LoginId
    {
        get { return PlayerPrefs.GetString(KEY_LOGINID, string.Empty); }
        set
        {
            if (string.IsNullOrEmpty(value))
                return;

            PlayerPrefs.SetString(KEY_LOGINID, value);
        }
    }
    public static string Language   // 국제 표준 언어 코드를 사용 (ex. "ko", "en", "ja")
    {
        get{ return PlayerPrefs.GetString(KEY_LANGUAGE, "ko"); }

        set
        {
            string language;

            // 오타 위험 등을 방지하기 위해 잘못된 값이거나 지정되지 않은 경우일 경우 기본값으로 한국어 지정
            switch (value.ToLower())
            {
                case "ko":
                //case "ja":    // 아직 일본어는 추가 x → 폰트가 일본어는 지원하지 않기 때문
                case "en":
                    language = value.ToLower();
                    break;
                default:
                    language = "ko";
                    break;
            }

            PlayerPrefs.SetString(KEY_LANGUAGE, language);
        }
    }

    public static float BgmVolume
    {
        get { return PlayerPrefs.GetFloat(KEY_BGMVOLUME, 1f); }

        set
        {
            float vol = Mathf.Clamp(value, 0f, 1f);
            PlayerPrefs.SetFloat(KEY_BGMVOLUME, vol);
        }
    }

    public static float SFXVolume
    {
        get { return PlayerPrefs.GetFloat(KEY_SFXVOLUME, 1f); }

        set
        {
            float vol = Mathf.Clamp(value, 0f, 1f);
            PlayerPrefs.SetFloat(KEY_SFXVOLUME, vol);
        }
    }

    public static int GraphicQuality
    {
        get { return PlayerPrefs.GetInt(KEY_GRAPHICQUALITY, QualitySettings.GetQualityLevel()); }

        set
        {
            int index = Mathf.Clamp(value, 0, QualitySettings.names.Length - 1);
            PlayerPrefs.SetInt(KEY_GRAPHICQUALITY, index);
        }
    }

    public static float MouseSensitivity
    {
        get { return PlayerPrefs.GetFloat(KEY_MouseSensivity, 50f); }

        set
        {
            float sensitive = Mathf.Clamp(value, 0f, 100f);
            PlayerPrefs.SetFloat (KEY_MouseSensivity, sensitive);
        }
    }

    public static bool MouseXInvert
    {
        get 
        {      
            return PlayerPrefs.GetInt(KEY_MouseXInvert, 0) == 1 ? true : false;
        }
        set
        {
            int num = value == true ? 1 : 0;
            PlayerPrefs.SetInt(KEY_MouseXInvert, num);
        }
    }
    public static bool MouseYInvert
    {
        get 
        {      
            return PlayerPrefs.GetInt(KEY_MouseYInvert, 0) == 1 ? true : false;
        }
        set
        {
            int num = value == true ? 1 : 0;
            PlayerPrefs.SetInt(KEY_MouseYInvert, num);
        }
    }

    public static float PlayerVoice
    {
        get { return PlayerPrefs.GetFloat(KEY_PlayerVoice, 1f); }

        set
        {
            float voice = Mathf.Clamp(value, 0f, 1f);
            PlayerPrefs.SetFloat(KEY_PlayerVoice, voice);
        }
    }
    public static float PlayerMic
    {
        get { return PlayerPrefs.GetFloat(KEY_PlayerMic, 1f); }

        set
        {
            float mic = Mathf.Clamp(value, 0f, 3f);
            PlayerPrefs.SetFloat(KEY_PlayerMic, mic);
        }
    }

    public static bool BgmVolumeMute
    {
        get
        {
            return PlayerPrefs.GetInt(KEY_BGMVOLUME_MUTE, 0) == 1 ? true : false;
        }
        set
        {
            int num = value == true ? 1 : 0;
            PlayerPrefs.SetInt(KEY_BGMVOLUME_MUTE, num);
        }
    }

    public static bool SFXVolumeMute
    {
        get
        {
            return PlayerPrefs.GetInt(KEY_SFXVOLUME_MUTE, 0) == 1 ? true : false;
        }
        set
        {
            int num = value == true ? 1 : 0;
            PlayerPrefs.SetInt(KEY_SFXVOLUME_MUTE, num);
        }
    }


    public static bool PlayerVoiceMute
    {
        get
        {
            return PlayerPrefs.GetInt(KEY_PlayerVoice_MUTE, 0) == 1 ? true : false;
        }
        set
        {
            int num = value == true ? 1 : 0;
            PlayerPrefs.SetInt(KEY_PlayerVoice_MUTE, num);
        }
    }

    public static bool PlayerMicMute
    {
        get
        {
            return PlayerPrefs.GetInt(KEY_PlayerMic_MUTE, 0) == 1 ? true : false;
        }
        set
        {
            int num = value == true ? 1 : 0;
            PlayerPrefs.SetInt(KEY_PlayerMic_MUTE, num);
        }
    }

    public static int ResolutionWidth
    {
        get
        {
            return PlayerPrefs.GetInt(KEY_RESOLUTIONWIDTH);
        }
        set
        {
            PlayerPrefs.SetInt(KEY_RESOLUTIONWIDTH, value);
        }
    }
    public static int ResolutionHeight
    {
        get
        {
            return PlayerPrefs.GetInt(KEY_RESOLUTIONHEIGHT);
        }
        set
        {
            PlayerPrefs.SetInt(KEY_RESOLUTIONHEIGHT, value);
        }
    }
    public static float ResolutionHz
    {
        get
        {
            return PlayerPrefs.GetFloat (KEY_RESOLUTIONHZ);
        }
        set
        {
            PlayerPrefs.SetFloat(KEY_RESOLUTIONHZ, value);
        }
    }

    public static int ResolutionWindow
    {
        get
        {
            return PlayerPrefs.GetInt(KEY_RESOLUTIONWINDOW);
        }
        set
        {
            PlayerPrefs.SetInt(KEY_RESOLUTIONWINDOW, value);
        }
    }
}
