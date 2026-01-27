using UnityEngine;

public interface IInteractable
{
    public bool IsInteracted { get; }
    public InteractionDataSO interactionData { get; set; }
    public Transform ActorTrans { get; }
    public void OnInteraction();    // 상호작용 호출자가 실행할 메서드
    public void OnStopped();                // 상호작용이 끝날 때 실행할 메서드
}
