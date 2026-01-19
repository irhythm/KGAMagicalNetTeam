using Photon.Pun;
using System.Collections;
using UnityEngine;

public class FireballExplode : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ExplodeAfterDelay(3f));
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ExplodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(gameObject);
        // 폭발 효과 실행
    }
}
