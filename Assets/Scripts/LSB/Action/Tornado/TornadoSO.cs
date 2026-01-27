using UnityEngine;

[CreateAssetMenu(fileName = "New Tornado", menuName = "Game/Tornado")]
public class TornadoSO : MagicDataSO
{
    [Header("Tornado Move")]
    public float moveSpeed = 5f;          // 토네이도 이동 속도
    public float rotationSpeed = 300f;    // 토네이도 회전 속도
    public float duration = 8f;           // 지속 시간

    public LayerMask hitLayer;            

    public float orbitSpeed = 10f;        // 오브젝트가 도는 속도

    public float suctionSpeed = 15f;      // 빨아들이는 속도

    public float liftSpeed = 3f;          // 위로 올리는 속도

    public float captureStrength = 3f;    // 붙잡는 힘

    [Header("Ejection")]
    public float releaseHeight = 8f;      // 이 높이에 도달하면 방출
    public float ejectForce = 20f;        // 방출 시 날리는 힘
    public float maxDistance = 7f;        // 최대 빨아들이는 거리

    public override ActionBase CreateInstance()
    {
        return new MagicTornado(this);
    }
}