using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;

public class SHSTest : MonoBehaviour, IInteractable
{
    public bool IsInteracted { get; private set; }

    public Transform ActorTrans => transform;

    [field: SerializeField] public InteractionDataSO interactionData { get; set; }

    [SerializeField] private InteractionDataSO test;
    [SerializeField] private Transform p1;
    [SerializeField] private Transform p2;
    [SerializeField] private CinemachineCamera cam;

    // 상호작용 인터페이스 메서드
    public void OnInteraction()
    {

    }

    public void OnStopped()
    {

    }
}