using System;
using UnityEngine;

public abstract class BaseInteractSystem
{
    protected IInteractable executer;       // 상호작용을 시작하는 트랜스폼
    protected IInteractable receiver;       // 상호작용을 당하는 트랜스폼

    protected event Action executeAct;
    protected event Action receiveAct;

    public BaseInteractSystem(IInteractable executer, IInteractable receiver)
    {
        Init(executer, receiver);
    }

    // 초기화
    public virtual void Init(IInteractable executer, IInteractable receiver)
    {
        this.executer = executer;
        this.receiver = receiver;

        executeAct = executer.OnInteract;
        receiveAct = executer.OnInteract;
    }

    // 상호작용 실행
    public virtual void PlayInteract()
    {
        executeAct?.Invoke();   // 실행자 액션 실행
        receiveAct?.Invoke();   // 리시버 액션 실행
    }
}
