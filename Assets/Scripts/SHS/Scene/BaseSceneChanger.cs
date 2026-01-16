using UnityEngine;

/// <summary>
/// 각 씬에 배치할 씬 관리 클래스
/// 해당 씬에 진입했을 때, 해당 씬에서 벗어날 때 등을 관리
/// 해당 씬에 필요한 데이터 등을 가지고 있기도 함
/// </summary>
public class BaseSceneChanger : MonoBehaviour
{
    [Header("씬 배경음")]
    [SerializeField] protected AudioClip bgmAudioClip;
    [SerializeField] private bool isBgmPlaying;

    [Header("로딩창 유무")]
    [SerializeField] private bool hasLoading;

    protected virtual void Start()
    {
        if(isBgmPlaying)
        {
            SoundManager.Instance.PlayBGM(bgmAudioClip);
        }
    }

    /// <summary>
    /// 해당 씬에 진입했을 때 실행할 것들
    /// </summary>
    protected virtual void EnterScene()
    {
        if(isBgmPlaying && bgmAudioClip != null)
            SoundManager.Instance.PlayBGM(bgmAudioClip);


    }

    /// <summary>
    /// 해당 씬을 벗어날 때 실행할 것들
    /// </summary>
    protected virtual void ExitScene()
    {
        if(hasLoading)
        {

        }
    }

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void OnDisable()
    {
        
    }
}
