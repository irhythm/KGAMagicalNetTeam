using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class MeleeWeapon : MonoBehaviour
{
    private int damageAmount;
    private Collider weaponCol;

    //1어택 1피격
    private List<GameObject> hitTargets = new List<GameObject>();

    private void Awake()
    {
        weaponCol = GetComponent<Collider>();
        weaponCol.enabled = false; //평소 off
        weaponCol.isTrigger = true; //물리충돌 꺼두는게 맞나?
    }
    // GuardAI가 공격력을 주입해줌
    public void SetDamage(int damage)
    {
        this.damageAmount = damage;
    }

    //온힛에서 호출
    public void EnableHitbox()
    {
        hitTargets.Clear(); // 맞은 목록 초기화
        weaponCol.enabled = true;
    }

    //OnHitEnd에서 호출
    public void DisableHitbox()
    {
        weaponCol.enabled = false;
    }

    //실제 충돌 감지
    private void OnTriggerEnter(Collider other)
    {
        //방장만
        if (!PhotonNetwork.IsMasterClient) return;
        //중복타격방지
        if (hitTargets.Contains(other.gameObject)) return;

        //레이어체크
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            IDamageable target = other.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage((float)damageAmount);
                hitTargets.Add(other.gameObject);
                Debug.Log($"{other.name} 타격 완료");
            }
        }
    }

}
