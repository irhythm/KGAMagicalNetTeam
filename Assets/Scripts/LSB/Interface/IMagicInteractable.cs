using UnityEngine;

public interface IMagicInteractable
{
    void OnMagicInteract(GameObject magic, MagicDataSO data, int attackerActorNr);
}
