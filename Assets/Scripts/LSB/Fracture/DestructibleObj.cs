using UnityEngine;

/// <summary>
/// 이 오브젝트가 파괴 가능한 벽임을 식별하는 클래스입니다.
/// </summary>
public class DestructibleWall : MonoBehaviour
{
    private void Awake()
    {
        // ChunkGraphManager나 ChunkNode들이 각자 Awake에서 초기화되므로
        // 여기서는 특별한 로직 없이 식별자 역할만 수행합니다.
    }
}