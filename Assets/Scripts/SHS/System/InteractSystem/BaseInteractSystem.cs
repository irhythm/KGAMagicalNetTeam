using System;
using System.Collections.Generic;
using System.Linq;

public abstract class BaseInteractSystem
{
    protected Dictionary<int, List<InteractionDataSO>> datas = new Dictionary<int, List<InteractionDataSO>>();   // 같은 타입의 상호작용이여도 대상의 수에 따라 모션이 다를 경우

    protected IInteractable executer;           // 상호작용을 시작하는 트랜스폼
    protected List<IInteractable> receivers;    // 상호작용을 당하는 트랜스폼

    protected event Action StartAct;
    protected event Action EndAct;

    public BaseInteractSystem(InteractionDataSO data, IInteractable executer, params IInteractable[] receivers)
    {
        Init(data, executer, receivers);
    }

    // 초기화
    public virtual void Init(InteractionDataSO data, IInteractable executer, params IInteractable[] receivers)
    {
        if(datas.TryGetValue(receivers.Length, out var dataList))
        {
            if (!dataList.Contains(data))
                dataList.Add(data);
        }

        this.executer = executer;
        this.receivers = receivers.ToList();

        StartAct += executer.OnInteraction;
        EndAct += executer.OnStopped;

        foreach(var receiver in receivers)
        {
            StartAct += receiver.OnInteraction;
            EndAct += receiver.OnStopped;
        }
    }

    // 행동이 다 끝나고 빠져나오기 전 액션 실행 및 초기화
    public void EndInteract()
    {
        EndAct?.Invoke();

        executer = null;
        receivers = null;

        StartAct = null;
        EndAct = null;
    }

    // 상호작용 실행
    public virtual void PlayInteract()
    {
        StartAct?.Invoke();   // 시작 액션 실행
    }
}
