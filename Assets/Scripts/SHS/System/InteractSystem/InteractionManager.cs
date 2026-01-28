using System;
using System.Collections.Generic;
using System.Linq;
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

    private BaseInteractSystem interactSystem;      // 상호작용 클래스 캐싱

    private Dictionary<InteractionType, BaseInteractSystem> interactSystemDic = new Dictionary<InteractionType, BaseInteractSystem>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void RequestInteraction(InteractionDataSO data, IInteractable executer, params IInteractable[] receivers)
    {
        if (data == null || executer == null || receivers == null) return;

        if (!interactSystemDic.ContainsKey(data.type))
        {
            BaseInteractSystem newSystem = null;

            switch (data.type)
            {
                case InteractionType.Assassinate:
                    newSystem = new AssassinateInteract(data, executer, receivers);
                    break;
            }

            interactSystemDic.Add(data.type, newSystem);
        }
        else
        {
            interactSystemDic[data.type].Init(data, executer, receivers);
        }

        interactSystem = interactSystemDic[data.type];

        this.executer = executer;
        this.receivers = receivers.ToList();

        TimelineSetting(data, executer, receivers);

        Play();
    }

    // 타임라인 포지션 등 세팅
    private void TimelineSetting(InteractionDataSO data, IInteractable executer, params IInteractable[] receivers)
    {
        if (data == null) return;
        if (ProjectManager.Instance == null || ProjectManager.Instance.CinemachineControl == null) return;

        pd.playableAsset = data.timelineAsset;

        var tracks = data.timelineAsset.GetOutputTracks().ToArray();
        SetTrackValue(data, tracks, executer, receivers);
        SetTransform(data, executer, receivers);
    }

    private void Play()
    {
        ProjectManager.Instance.CinemachineControl.SetCameraType(Cinemachinetype.CutScene);

        interactSystem.PlayInteract();
        pd.stopped += Stop;

        pd.Play();
    }

    private void Stop(PlayableDirector pd)
    {
        ProjectManager.Instance.CinemachineControl.SetCameraType(Cinemachinetype.InGame);
        interactSystem.EndInteract();
        pd.playableAsset = null;
    }

    // 트랙에 맞게 매칭
    private void SetTrackValue(InteractionDataSO data, TrackAsset[] tracks, IInteractable executer, params IInteractable[] receivers)
    {
        pd.SetGenericBinding(tracks[Array.FindIndex(tracks, x => x.name.StartsWith("Camera"))], ProjectManager.Instance.CinemachineControl.cutSceneCamera.GetComponent<Animator>());
        pd.SetGenericBinding(tracks[Array.FindIndex(tracks, x => x.name.StartsWith("Executer"))], executer.ActorTrans.gameObject);

        for (int i = 0; i < data.offset_Receiver.Count; i++)
        {
            TrackAsset track = tracks[Array.FindIndex(tracks, x => x.name == data.offset_Receiver[i].name)];

            pd.SetGenericBinding(track, receivers[i].ActorTrans.gameObject);
        }

        foreach (var track in tracks)
        {
            if (track is SignalTrack signalTrack)
            {
                pd.SetGenericBinding(signalTrack, ProjectManager.Instance.CinemachineControl.cutSceneCamera.GetComponent<SignalReceiver>());
            }
        }
    }

    // 기준 트랜스폼에 맞게 나머지 트랙들의 오브젝트 트랜스폼도 반영
    private void SetTransform(InteractionDataSO data, IInteractable executer, params IInteractable[] receivers)
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
        if (data.offset_Executer.isApply)
        {
            executer.ActorTrans.root.position = pivotTrans.TransformPoint(data.offset_Executer.position);
            executer.ActorTrans.root.rotation = Quaternion.Euler(pivotTrans.eulerAngles + data.offset_Executer.rotation);
        }

        if (data.offset_Camera.isApply)
        {
            ProjectManager.Instance.CinemachineControl.cutSceneCamera.transform.position = pivotTrans.TransformPoint(data.offset_Camera.position);
            ProjectManager.Instance.CinemachineControl.cutSceneCamera.transform.rotation = Quaternion.Euler(pivotTrans.eulerAngles + data.offset_Camera.rotation);
        }

        for (int i = 0; i < data.offset_Receiver.Count; i++)
        {
            if (data.offset_Receiver[i].isApply == false) continue;

            // 트랜스폼 변경
            receivers[i].ActorTrans.root.position = pivotTrans.TransformPoint(data.offset_Receiver[i].position);
            receivers[i].ActorTrans.root.rotation = Quaternion.Euler(pivotTrans.eulerAngles + data.offset_Receiver[i].rotation);
        }
    }
}
