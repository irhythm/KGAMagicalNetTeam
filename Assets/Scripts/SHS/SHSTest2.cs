using UnityEngine;

public class SHSTest2 : MonoBehaviour, IInteractable
{
    public bool isInteracted { get; private set; }

    public Transform ActorTrans => transform;

    public void OnExecuterInteraction()
    {

    }

    public void OnReceiverInteraction()
    {

    }

    public void OnStopped()
    {

    }
}
