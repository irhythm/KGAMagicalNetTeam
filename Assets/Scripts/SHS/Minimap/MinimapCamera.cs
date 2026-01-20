using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [field: SerializeField] public Transform FollowTarget { get; private set; }

    [SerializeField] private float yOffset;

    public void SetTarget(Transform target) => FollowTarget = target;

    private void Start()
    {
        if (FollowTarget == null)
        {
            var players = FindObjectsByType<PlayableCharacter>(FindObjectsSortMode.None);

            foreach(var player in players)
            {
                if (player.photonView.IsMine)
                {
                    FollowTarget = player.transform;
                }
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
