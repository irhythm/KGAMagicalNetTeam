using UnityEngine;

public class Bullet : MonoBehaviour
{
    //총알을 발사한 플레이어 고유번호
    public int actorNumber;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //로컬방향 기준으로 힘을 가함
        GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * 1000f);
        Destroy(gameObject, 3.0f);
    }
}
