using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public enum Soundtype
{
    BGM,        // 배경음
    SFX,        // 효과음
}

public class SoundManager : Singleton<SoundManager>
{
    [Header("오디오 믹서")]
    [field: SerializeField] public AudioMixer MasterAudioMixer { get; private set; }

    [Header("브금을 재생할 오디오 소스")]
    [field: SerializeField] public AudioSource bgmSource { get; private set; }

    [Header("효과음을 재생할 오디오 소스 프리팹")]
    [SerializeField] private AudioSource sfxSource;

    [Header("사운드 풀")]
    [SerializeField] private SoundPool soundPool;

    [Header("오디오 클립 모음 SO 파일")]
    [SerializeField] private AudioClipSO audioClipSO;

    // 배경음, 효과음 오디오 클립을 저장할 딕셔너리
    private Dictionary<string, AudioClip> bgmDic = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDic = new Dictionary<string, AudioClip>();

    private List<AudioSource> sfxList = new List<AudioSource>();    // 현재 재생중인 효과음 오디오 소스를 가지고 있을 리스트
    private Coroutine sfxPlayCoroutine;                             // 현재 재생중인 효과음 오디오 소스들을 체크하는 코루틴 변수

    protected override void Awake()
    {
        base.Awake();
        InitData();
    }

    #region Init
    // 데이터 초기화
    private void InitData()
    {
        if (audioClipSO.bgmClips == null || audioClipSO.sfxClips == null)
        {
            return;
        }


        // 배경음 딕셔너리에 데이터 추가
        if (audioClipSO.bgmClips.Length > 0)
        {
            foreach(var so in audioClipSO.bgmClips)
            {
                if(so != null)
                    bgmDic.Add(so.name, so);
            }
        }

        // 효과음 딕셔너리에 데이터 추가
        if(audioClipSO.sfxClips.Length > 0)
        {
            foreach(var so in audioClipSO.sfxClips)
            {
                if(so != null)
                    sfxDic.Add(so.name, so);
            }
        }
    }
    #endregion

    #region 오디오 클립을 받아와 재생
    public void PlayBGM(AudioClip clip)
    {
        PlaySound(Soundtype.BGM, bgmSource, clip, 0f);
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    /// <param name="clip"> 재생할 오디오 클립 </param>
    public void PlaySFX(AudioClip clip)
    {
        AudioSource source = soundPool.Get(sfxSource);

        if (source == null) return;

        PlaySound(Soundtype.SFX, source, clip, 0f);
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    /// <param name="clip"> 재생할 오디오 클립 </param>
    /// <param name="spatialAmount"> 공간 음향 수치값 (0 → 2D 음향 / 1 → 3D 음향) </param>
    /// <param name="maxDistance"> 3D 음향일 경우 들을 수 있는 최대 거리 </param>
    /// <param name="pos"> 재생시킬 위치 </param>
    public void PlaySFX(AudioClip clip, float spatialAmount, float maxDistance, Vector3 pos)
    {
        AudioSource source = soundPool.Get(sfxSource);

        if (source == null) return;

        source.transform.position = pos;
        PlaySound(Soundtype.SFX, source, clip, spatialAmount, maxDistance);
    }
    #endregion

    #region 오디오 클립 이름(string)을 받아와 재생
    public void PlayBGM(string bgmName)
    {
        if (bgmSource == null || !bgmDic.TryGetValue(bgmName, out var clip)) return;

        PlaySound(Soundtype.BGM, bgmSource, clip, 0f);
    }

    public void PlaySFX(string sfxName)
    {
        AudioSource source = soundPool.Get(sfxSource);

        if (source == null || !sfxDic.TryGetValue(sfxName, out var clip)) return;

        PlaySound(Soundtype.SFX, source, clip, 0f);
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    /// <param name="sfxName"> 재생할 오디오 클립 이름, 딕셔너리에서 찾을 키값 </param>
    /// <param name="spatialAmount"> 공간 음향 수치값 (0 → 2D 음향 / 1 → 3D 음향) </param>
    /// <param name="pos"> 재생시킬 위치 </param>
    public void PlaySFX(string sfxName, float spatialAmount, float maxDistance, Vector3 pos)
    {
        AudioSource source = soundPool.Get(sfxSource);

        if (source == null || !sfxDic.TryGetValue(sfxName, out var clip)) return;

        source.transform.position = pos;
        PlaySound(Soundtype.SFX, source, clip, spatialAmount, maxDistance);
    }
    #endregion
    
    private void PlaySound(Soundtype type, AudioSource source, AudioClip clip, float spatialAmount, float maxDistance = 500)
    {
        if (source == null || clip == null) return;

        source.clip = clip;
        source.spatialBlend = Mathf.Clamp(spatialAmount, 0.0f, 1.0f);
        source.maxDistance = maxDistance;
        source.Play();

        if(type == Soundtype.SFX)
            AddSfXList(source);
    }

    #region 사운드 재생 중지
    public void StopSound(AudioSource source)
    {
        if(source != null && source.isPlaying)
        {
            source.Stop();
        }
    }
    #endregion

    #region 오디오 소스 설정
    public void SetSoundVolume(Soundtype type, float volume, bool mute = false)
    {
        if (MasterAudioMixer == null) return;

        switch(type)
        {
            case Soundtype.BGM:
                SetVolume("BGM", volume);
                if (!mute)
                    PlayerPrefsDataManager.BgmVolume = volume;
                break;
            case Soundtype.SFX:
                SetVolume("SFX", volume);
                if (!mute)
                    PlayerPrefsDataManager.SFXVolume = volume;
                break;
        }
    }

    private void SetVolume(string mixerParam, float volume)
    {
        MasterAudioMixer.SetFloat(mixerParam, Mathf.Log10(volume) * 20);
    }
    #endregion

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopSound(bgmSource);
    }

    private void AddSfXList(AudioSource sfx)
    {
        if (sfx == null) return;

        sfxList.Add(sfx);

        if(sfxPlayCoroutine == null)
        {
            sfxPlayCoroutine = StartCoroutine(CheckPlayingSFX());
        }
    }

    // sfxList중 재생이 완료된 오디오 소스 정리
    private IEnumerator CheckPlayingSFX()
    {
        while (sfxList.Count > 0)
        {
            for (int i = sfxList.Count - 1; i >= 0; i--)
            {
                if (!sfxList[i].isPlaying)
                {
                    soundPool.Release(sfxList[i]);
                    sfxList.RemoveAt(i);
                }
            }

            Debug.Log("코루틴 도는중");
            yield return CoroutineManager.waitForSeconds(0.5f);
        }

        sfxPlayCoroutine = null;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if(sfxPlayCoroutine != null)
            StopCoroutine(sfxPlayCoroutine);
    }
}
