using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;

public class SHSTest : MonoBehaviour, IInteractable
{
    public bool isInteracted { get; private set; }

    public Transform ActorTrans => transform;

    [SerializeField] private InteractionDataSO test;
    [SerializeField] private Transform p1;
    [SerializeField] private Transform p2;
    [SerializeField] private CinemachineCamera cam;

    private void Update()
    {
        if(Keyboard.current.uKey.wasPressedThisFrame)
        {
            InteractionManager.Instance.RequestInteraction(test, cam, p1.GetComponent<IInteractable>(), p2.GetComponent<IInteractable>());
        }
    }

    // 상호작용 인터페이스 메서드
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