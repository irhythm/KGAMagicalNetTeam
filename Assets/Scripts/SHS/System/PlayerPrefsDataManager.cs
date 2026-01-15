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
            switch (value)
            {
                case "ko":
                //case "ja":    // 아직 일본어는 추가 x → 폰트가 일본어는 지원하지 않기 때문
                case "en":
                    language = value;
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
}
