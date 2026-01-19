using UnityEngine;

[CreateAssetMenu(fileName = "New Coin", menuName = "Game/CoinSO")]
public class CoinSO : ItemDataSO
{
    public override ActionBase CreateInstance()
    {
        return new ItemCoin(this);
    }
}
