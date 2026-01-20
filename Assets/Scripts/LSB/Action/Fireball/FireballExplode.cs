using System.Collections;
using UnityEngine;

// [수정] Photon 관련 코드 제거 (단순 로컬 이펙트)
public class FireballExplode : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(ExplodeAfterDelay(3f));
    }

    IEnumerator ExplodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // [수정] 로컬 오브젝트 파괴
        Destroy(gameObject);
    }
}