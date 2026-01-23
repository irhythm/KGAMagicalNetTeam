using UnityEngine;

public class SHSTest : MonoBehaviour, IInteractable
{
    public bool isInteracted { get; private set; }

    // 상호작용 인터페이스 메서드
    public void OnExecuterInteraction()
    {

    }

    public void OnReceiverInteraction()
    {
        
    }
}