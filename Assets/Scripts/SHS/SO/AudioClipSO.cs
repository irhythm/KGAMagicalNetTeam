using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipSO", menuName = "Scriptable Objects/AudioClipSO")]
public class AudioClipSO : ScriptableObject
{
    [field: SerializeField] public AudioClip[] bgmClips { get; private set; }    // 배경음 오디오 클립
    [field: SerializeField] public AudioClip[] sfxClips { get; private set; }    // 효과음 오디오 클립
}
