using UnityEngine;
using Photon.Pun;

public class GhostCleaner : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        CleanUpGhosts();
    }

    private void CleanUpGhosts()
    {
        GuardAI[] ghosts = FindObjectsByType<GuardAI>(FindObjectsSortMode.None);

        if (ghosts.Length > 0)
        {
            Debug.LogWarning($"{ghosts.Length} 마리 삭제");

            foreach (var ghost in ghosts)
            {
                if (ghost != null && ghost.gameObject != null)
                {
                    PhotonNetwork.Destroy(ghost.gameObject);
                }
            }
        }
    }
}