using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

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

    public void RequestInteraction(InteractionDataSO data, IInteractable executer, params IInteractable[] receivers)
    {
        if (data == null || executer == null || receivers == null) return;

        BaseInteractSystem system = null;

        switch (data.type)
        {
            case InteractionType.Assassinate:
                system = new AssassinateInteract(data, executer, receivers);
                break;
        }

    }

    // 타임라인 포지션 등 세팅
    private void TimelineSetting(InteractionDataSO data)
    {
        if (data == null) return;

        for(int i = 0; i < data.trackNames.Count; i++)
        {
            if (data.offsets[i].isApply == false) continue;


        }
    }
}
