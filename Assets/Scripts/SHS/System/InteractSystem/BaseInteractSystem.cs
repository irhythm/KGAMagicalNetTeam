using System;
using System.Collections.Generic;
using System.Linq;

public abstract class BaseInteractSystem
{
    protected Dictionary<int, List<InteractionDataSO>> datas;   // 같은 타입의 상호작용이여도 대상의 수에 따라 모션이 다를 경우

    protected IInteractable executer;           // 상호작용을 시작하는 트랜스폼
    protected List<IInteractable> receivers;    // 상호작용을 당하는 트랜스폼

    protected event Action executeAct;
    protected event Action receiveAct;

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

        executeAct = executer.OnExecuterInteraction;
        receiveAct = executer.OnExecuterInteraction;
    }

    // 상호작용 실행
    public virtual void PlayInteract()
    {
        executeAct?.Invoke();   // 실행자 액션 실행
        receiveAct?.Invoke();   // 리시버 액션 실행
    }
}
