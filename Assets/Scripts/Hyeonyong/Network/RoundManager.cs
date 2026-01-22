using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;
    public Transform[] spawnPos;
    private void Awake()
    {
        Instance = this;
    }
}
