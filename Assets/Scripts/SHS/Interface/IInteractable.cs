using UnityEngine;

public interface IInteractable
{
    public bool isInteracted { get; }
    public void OnInteract();
}
