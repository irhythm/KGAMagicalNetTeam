using UnityEngine;

public interface IInteractable
{
    public bool isInteracted { get; }
    public Transform ActorTrans { get; }
    public void OnExecuterInteraction();    // 상호작용 호출자가 실행할 메서드
    public void OnReceiverInteraction();    // 상호작용 수신자가 호출할 메서드
    public void OnStopped();                // 상호작용이 끝날 때 실행할 메서드
}
