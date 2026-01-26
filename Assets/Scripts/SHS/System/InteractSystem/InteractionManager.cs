using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 상호작용을 관리할 중간 매개체 매니저
/// </summary>
public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private PlayableDirector pd;

    private IInteractable executer;
    private List<IInteractable> receivers = new List<IInteractable>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void RequestInteraction(InteractionDataSO data, CinemachineCamera camera, IInteractable executer, params IInteractable[] receivers)
    {
        if (data == null || executer == null || receivers == null) return;

        BaseInteractSystem system = null;

        switch (data.type)
        {
            case InteractionType.Assassinate:
                system = new AssassinateInteract(data, executer, receivers);
                break;
        }

        this.executer = executer;
        this.receivers = receivers.ToList();

        TimelineSetting(data, camera, executer, receivers);
    }

    // 타임라인 포지션 등 세팅
    private void TimelineSetting(InteractionDataSO data, CinemachineCamera camera, IInteractable executer, params IInteractable[] receivers)
    {
        if (data == null) return;

        pd.playableAsset = data.timelineAsset;

        var tracks = data.timelineAsset.GetOutputTracks().ToArray();
        SetTrackValue(data, tracks, camera, executer, receivers);
        SetTransform(data, camera, executer, receivers);

        pd.Play();
    }

    private void SetTrackValue(InteractionDataSO data, TrackAsset[] tracks, CinemachineCamera camera, IInteractable executer, params IInteractable[] receivers)
    {
        pd.SetGenericBinding(tracks[Array.FindIndex(tracks, x => x.name.StartsWith("Camera"))], camera.GetComponent<Animator>());
        pd.SetGenericBinding(tracks[Array.FindIndex(tracks, x => x.name.StartsWith("Executer"))], executer.ActorTrans.gameObject);

        for(int i = 0; i < data.offset_Receiver.Count; i++)
        {
            TrackAsset track = tracks[Array.FindIndex(tracks, x => x.name == data.offset_Receiver[i].name)];

            pd.SetGenericBinding(track, receivers[i].ActorTrans.gameObject);
        }

        foreach(var track in tracks)
        {
            if(track is SignalTrack signalTrack)
            {
                pd.SetGenericBinding(signalTrack, camera.GetComponent<SignalReceiver>());
            }
        }
    }

    private void SetTransform(InteractionDataSO data, CinemachineCamera camera, IInteractable executer, params IInteractable[] receivers)
    {
        // 누가 기준 트랜스폼인지에 따라 위치 보정
        Transform pivotTrans;

        if (data.isExecuterPivot)
        {
            // 상호작용 호출자가 기준일 때
            pivotTrans = executer.ActorTrans;
        }
        else
        {
            // 상호작용 수신자가 기준일 때
            int index = 0;

            if (receivers.Length > data.receiverPivotIndex)
            {
                index = data.receiverPivotIndex;
            }

            pivotTrans = receivers[index].ActorTrans;
        }

        // 기준 트랜스폼을 기준으로 실제 위치, 회전값 적용
        if(data.offset_Executer.isApply)
        {
            executer.ActorTrans.position = pivotTrans.TransformPoint(data.offset_Executer.position);
            executer.ActorTrans.rotation = Quaternion.Euler(pivotTrans.eulerAngles + data.offset_Executer.rotation);
        }
        
        if(data.offset_Camera.isApply)
        {
            camera.transform.position = pivotTrans.TransformPoint(data.offset_Camera.position);
            camera.transform.rotation = Quaternion.Euler(pivotTrans.eulerAngles + data.offset_Camera.rotation);
        }

        for (int i = 0; i < data.offset_Receiver.Count; i++)
        {
            if (data.offset_Receiver[i].isApply == false) continue;

            // 트랜스폼 변경
            receivers[i].ActorTrans.position = pivotTrans.TransformPoint(data.offset_Receiver[i].position);
            receivers[i].ActorTrans.rotation = Quaternion.Euler(pivotTrans.eulerAngles + data.offset_Receiver[i].rotation);
        }
    }
}
