using UnityEngine;
using UnityEngine.InputSystem;
public class CoinLogic : MonoBehaviour
{
    //[SerializeField] 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     
        InputSystem.actions.FindActionMap("Player").FindAction("Interact").performed += PickupCoin;


    }

    void OnDestroy()
    {
        InputSystem.actions.FindActionMap("Player").FindAction("Interact").performed -= PickupCoin;
    }

    void PickupCoin(InputAction.CallbackContext context)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 5f))
        {
            if (hit.transform.gameObject.GetComponent<CoinItself>() != null)
            {
                //CollectCoin();
            }
        }
        //EventSystem.Screen
        Debug.Log("Coin Picked Up!");
        // Add coin to player's inventory or increase coin count
        // Example: playerInventory.AddCoins(1);
        Destroy(gameObject); // Remove the coin from the scene
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
