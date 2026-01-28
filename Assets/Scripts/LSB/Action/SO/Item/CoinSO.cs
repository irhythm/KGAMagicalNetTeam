using UnityEngine;

[CreateAssetMenu(fileName = "New Coin", menuName = "Game/CoinSO")]
public class CoinSO : ActionItemDataSO
{
    [SerializeField] public float CoinThrowForce = 2f;

    [SerializeField] public AudioClip CoinPickupSFX;
    [SerializeField] public AudioClip CoinThrowSFX;

    public override ActionBase CreateInstance()
    {
        return new ItemCoin(this);
    }
}
