using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class UI_Settings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropDown_Language;    // 언어 설정 드랍다운

    private void Awake()
    {
        InitSettings();
    }

    private void InitSettings()
    {
        if (dropDown_Language != null)
        {
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(PlayerPrefsDataManager.Language);

            if (locale != null)
                dropDown_Language.value = LocalizationSettings.AvailableLocales.Locales.IndexOf(locale);
        }
    }

    // 언어 변경
    public void OnValueChanged_SelectLangugae()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[dropDown_Language.value];
        PlayerPrefsDataManager.Language = LocalizationSettings.SelectedLocale.Identifier.Code;
    }
}
