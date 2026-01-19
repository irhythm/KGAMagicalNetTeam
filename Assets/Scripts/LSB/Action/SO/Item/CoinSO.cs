using UnityEngine;

public class CoinSO : ItemDataSO
{
    public override ActionBase CreateInstance()
    {
        return new ItemCoin();
    }
}
