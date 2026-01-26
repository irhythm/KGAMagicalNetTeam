using UnityEngine;

[CreateAssetMenu(fileName = "New Coin", menuName = "Game/CoinSO")]
public class CoinSO : ActionItemDataSO
{
    [SerializeField] public float CoinThrowForce = 2f;


    public override ActionBase CreateInstance()
    {
        return new ItemCoin(this);
    }
}
