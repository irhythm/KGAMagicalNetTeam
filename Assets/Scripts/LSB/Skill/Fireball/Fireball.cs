using UnityEngine;

public class Fireball : MonoBehaviour
{
    private FireballSO fireballData;

    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * fireballData.speed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

    }

    public void Init(FireballSO data)
    {
        fireballData = data;
    }

    
}
