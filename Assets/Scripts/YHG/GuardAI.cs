using UnityEngine;

public class GuardAI : BaseAI
{
    [Header("경비 세팅")]
    public float viewRadius = 10f; //시야거리

    [Range(0, 360)]
    public float viewAngle = 90f; //시야각

    public float attackRange = 2.0f;
    public float attackCooldown = 1.5f;

    protected override void SetInitialState()
    {
        throw new System.NotImplementedException();
    }
}
