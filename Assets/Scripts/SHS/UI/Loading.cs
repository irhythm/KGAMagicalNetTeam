using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class Loading : Singleton<Loading>
{
    #region 필드
    [Header("로딩창 오브젝트")]
    [SerializeField] private GameObject loadingObj;

    [Header("배경 이미지")]
    [SerializeField] private Image imgBackground;

    [Header("로딩창 배경 이미지")]
    [SerializeField] private Sprite[] backgrounds;

    [Header("로딩 바")]
    [SerializeField] private Slider loadingBar;

    [Header("페이크로딩 시간")]
    [SerializeField] private float fakeLoadingTime;

    [Header("팁 텍스트")]
    [SerializeField] private TMP_Text tipText;

    [Header("팁 문구")]
    [Multiline]
    [SerializeField] private string[] tips;

    [Header("페이크 로딩을 시작할 퍼센트 (0,5 ~ 0.9f)")]
    [Range(0.5f, 0.9f)]
    [SerializeField] private float fakeStartPer;

    private string loadSceneName;           // 다음 씬 이름을 받을 변수
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    public void LoadScene(string sceneName)
    {
        loadingObj.SetActive(true);
        SetBackground();
        SetTip();

        SceneManager.sceneLoaded += OnSceneLoaded;
        loadSceneName = sceneName;
        StartCoroutine(LoadSceneProcess());
    }

    private IEnumerator LoadSceneProcess()
    {
        loadingBar.value = 0f;

        AsyncOperation op = SceneManager.LoadSceneAsync(loadSceneName);
        op.allowSceneActivation = false;

        float timer = 0f;

        while (!op.isDone)
        {
            yield return null;

            if(op.progress < fakeStartPer)
            {
                loadingBar.value = op.progress;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                loadingBar.value = Mathf.Lerp(fakeStartPer, 1f, timer / fakeLoadingTime);
                if(loadingBar.value >= 1f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        if (scene.name == loadSceneName)
        {
            loadingObj.SetActive(false);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void SetBackground()
    {
        if (backgrounds == null || backgrounds.Length == 0)
            return;
        else
            imgBackground.sprite = backgrounds[Random.Range(0, backgrounds.Length)];
    }

    private void SetTip()
    {
        if (tips == null || tips.Length == 0)
            tipText.text = string.Empty;
        else
            tipText.text = tips[Random.Range(0, tips.Length)];
    }
}
