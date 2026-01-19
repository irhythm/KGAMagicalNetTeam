using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class CoinLogic : MonoBehaviour
{
    [SerializeField] float _pickupRange = 10f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     
        //InputSystem.actions.FindActionMap("Player").FindAction("Interact").performed += PickupCoin;
        Debug.Log(GetComponent<PlayerInputHandler>());
        GetComponent<PlayerInputHandler>().OnInteractEvent += PickupCoin;

    }

    void OnDestroy()
    {
       // InputSystem.actions.FindActionMap("Player").FindAction("Interact").performed -= PickupCoin;
    }

    private Vector3 _debugRayStart;
    private Vector3 _debugRayEnd;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(_debugRayStart, _debugRayEnd);
        Gizmos.DrawSphere(_debugRayEnd, 0.1f);
    }



    void PickupCoin()
    {
        _debugRayStart = Camera.main.transform.position;
        _debugRayEnd = _debugRayStart + Camera.main.transform.forward * _pickupRange;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit[] hits = Physics.RaycastAll(ray, _pickupRange);
        int index = 0;
        foreach (var hit in hits)
        {
            index++;
            if (index >3)
            {
                Debug.Log("Too many hits, breaking out of loop.");
                break;
            }

            Debug.Log("Raycast Hit All: " + hit.transform.name);



            if (hit.transform.gameObject.GetComponent<CoinItself>() != null)
            {
                GameManager.Instance.LocalPlayer.GetComponent<PlayableCharacter>().Inventory.AddItem(hit.transform.gameObject.GetComponent<CoinItself>().CoinData);
                hit.transform.gameObject.GetComponent<CoinItself>().RequestDestroy();
                Debug.Log("Coin Picked Up! Really");
            }


        }
        //EventSystem.Screen
        Debug.Log("Interaction F Clicked");
        
        // Add coin to player's inventory or increase coin count
        // Example: playerInventory.AddCoins(1);
        //Destroy(gameObject); // Remove the coin from the scene
    }

}
