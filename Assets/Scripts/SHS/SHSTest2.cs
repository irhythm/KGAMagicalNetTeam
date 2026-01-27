using UnityEngine;

public class SHSTest2 : MonoBehaviour, IInteractable
{
    public bool IsInteracted { get; private set; }

    public Transform ActorTrans => transform;

    [field: SerializeField] public InteractionDataSO interactionData { get; set; }

    public void OnInteraction()
    {

    }

    public void OnStopped()
    {

    }
}
