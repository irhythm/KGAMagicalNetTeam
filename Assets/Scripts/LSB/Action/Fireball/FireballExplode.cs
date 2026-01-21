using System.Collections;
using UnityEngine;

public class FireballExplode : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(ExplodeAfterDelay(3f));
    }

    IEnumerator ExplodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}