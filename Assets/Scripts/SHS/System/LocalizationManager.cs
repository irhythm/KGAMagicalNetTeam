using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LocalizationManager : Singleton<LocalizationManager>
{
    private Coroutine localizedCoroutine;

    public void ChangeLanguage(string localeCode)
    {
        // LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        if (localizedCoroutine != null)
            localizedCoroutine = null;

        localizedCoroutine = StartCoroutine(ChangeLanguageCoroutine(localeCode));
    }

    private IEnumerator ChangeLanguageCoroutine(string localeCode)
    {

        var selectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        LocalizationSettings.SelectedLocale = selectedLocale;

        LocalizationSettings.InitializationOperation.WaitForCompletion();

         //var assetHandle = LocalizationSettings.AssetDatabase.GetTableAsync("Font Table", selectedLocale).WaitForCompletion();
         //var stringHandle = LocalizationSettings.AssetDatabase.GetTableAsync("UITable", selectedLocale).WaitForCompletion();


        yield break;
        //// 1.localeCode 키를 가진 언어 받아오기
        //Locale selectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

        //if(selectedLocale == null)
        //{
        //    Debug.LogError($"[LocalizationManager]: {localeCode} 코드를 가진 언어가 존재하지 않습니다.");
        //    yield break;
        //}

        //if(selectedLocale == LocalizationSettings.SelectedLocale)
        //{
        //    Debug.LogError($"[LocalizationManager]: 이미 {localeCode} 코드로 설정되어 있습니다.");
        //    yield break;
        //}

        //// 언어 세팅의 초기화가 끝났는지에 대한 작업 반환
        //yield return LocalizationSettings.InitializationOperation;

        //var tableOp = LocalizationSettings.AssetDatabase.GetTableAsync("Font Asset Table", selectedLocale);
        //var status = tableOp.Status;

        //if(status == AsyncOperationStatus.Succeeded)

        //yield return tableOp;

        //AssetTable table = tableOp.Result;
        //AssetTableEntry entry = table.GetEntry("Font Asset");

    }
}
