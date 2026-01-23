using UnityEngine;

public class AssassinateInteract : BaseInteractSystem
{
    public AssassinateInteract(InteractionDataSO data, IInteractable executer, params IInteractable[] receivers) : base(data,executer, receivers)
    {

    }

    public override void Init(InteractionDataSO data, IInteractable executer, params IInteractable[] receivers)
    {
        base.Init(data, executer, receivers);
    }

    public override void PlayInteract()
    {
        base.PlayInteract();
    }
}
