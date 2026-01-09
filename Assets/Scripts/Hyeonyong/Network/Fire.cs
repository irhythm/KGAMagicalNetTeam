using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class Fire : MonoBehaviour
{
    [SerializeField] Transform firePos;
    [SerializeField] GameObject bulletPrefab;

    //네트워크에서 리젠되는 이들
    PhotonView pv;
    bool isMouseClick=>Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

    private void Start()
    {
        pv=GetComponent<PhotonView>();
    }
    private void Update()
    {
        if(!pv.IsMine)
            return;
        if (isMouseClick)
        {
            FireBullet(pv.Owner.ActorNumber);
            //내가 아닌 다른 이들
            pv.RPC(nameof(FireBullet),RpcTarget.Others,pv.Owner.ActorNumber);
            //pv.RPC(nameof(FireBullet),RpcTarget.All,pv.Owner.ActorNumber);
        }
    }

    [PunRPC]
    void FireBullet(int actorNum)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePos.position, firePos.rotation);
        bullet.GetComponent<Bullet>().actorNumber = actorNum;
    }

















    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("내 체력 깎는 로직");
        }
    }

    public void TakeDamage(int Damage)
    {
        
    }
}
