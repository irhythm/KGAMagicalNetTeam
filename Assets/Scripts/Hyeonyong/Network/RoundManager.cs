using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;
    public Transform[] spawnPos;
    [SerializeField] AudioClip roundAudio;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        SoundManager.Instance.PlayBGM(roundAudio);
    }
}
