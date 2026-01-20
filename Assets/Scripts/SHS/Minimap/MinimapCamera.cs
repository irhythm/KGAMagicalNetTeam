using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [field: SerializeField] public Transform FollowTarget { get; private set; }

    [SerializeField] private float yOffset;

    private void Awake()
    {
        if (FollowTarget == null)
        {
            var player = FindAnyObjectByType<PlayableCharacter>();

            if(player != null)
            {
                FollowTarget = player.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (FollowTarget != null)
        {
            Vector3 pos = new Vector3(FollowTarget.position.x, yOffset, FollowTarget.position.z);
            transform.position = pos;
        }
    }
}
