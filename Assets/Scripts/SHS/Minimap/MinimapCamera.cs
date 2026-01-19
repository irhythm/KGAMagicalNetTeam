using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [field: SerializeField] public Transform FollowTarget { get; private set; }

    [SerializeField] private float yOffset;

    private void LateUpdate()
    {
        if (FollowTarget != null)
        {
            Vector3 pos = new Vector3(FollowTarget.position.x, yOffset, FollowTarget.position.z);
            transform.position = pos;
        }
    }
}
