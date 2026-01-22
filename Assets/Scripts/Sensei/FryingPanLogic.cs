using UnityEngine;
using System.Collections;
using Photon.Pun;
using System.Collections.Generic;

public class FryingPanLogic : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject _mushroomPrefab;
    [SerializeField] Transform _mushroomSpawnPoint;
    [SerializeField] float _howLongToWait = 2f;
    [SerializeField] float _howLongToAlive = 3f;

    [SerializeField] List<Transform> _targets = new List<Transform>();
    PhotonView pv;

    Queue<GameObject> _mushrooms = new Queue<GameObject>();
    Transform removeTarget;

    IEnumerator FiringMushroom()
    {
        yield return null;    
        {
            int i = 0;
            while (i<1000)
            {
                Debug.Log("쏜다");
                //photonView.RPC("FireMushroom", RpcTarget.All, target);

                //한 번에 모든 플레이어에게 날리는 용도
                foreach (Transform target in _targets)
                {
                    if (target == null)
                    {
                        removeTarget = target;
                    }
                    //Debug.Log(target.gameObject.name+"에게 쏜다");
                    //FireMushroom(target);
                    if(PhotonNetwork.IsMasterClient)
                        pv.RPC(nameof(FireMushroom), RpcTarget.All, target.position);
                }
                _targets.Remove(removeTarget);
                Debug.Log("쏜다 끝");
                i++;
                yield return new WaitForSeconds(_howLongToWait);
            }
            //MushroomLogic mushroomLogic = mushroom.GetComponent<MushroomLogic>();
            //mushroomLogic.SetTarget(target);
            //yield return new WaitForSeconds(0.2f);
        }
    }

    [PunRPC]
    void FireMushroom(Vector3 target)
    {
        Debug.Log("쏜다 코드 실행");

        GameObject mushroom=PoolMushRoom();
        if (mushroom == null)
        {
            mushroom = Instantiate(_mushroomPrefab, _mushroomSpawnPoint.position, Quaternion.identity);
        }
        else
        {
            mushroom.SetActive(true);
            mushroom.transform.position = _mushroomSpawnPoint.position;
            mushroom.transform.rotation = Quaternion.identity;
        }

        Rigidbody rb = mushroom.GetComponent<Rigidbody>();
        //Quaternion lookRotation = Quaternion.LookRotation(target.position - _mushroomSpawnPoint.position);
        Quaternion lookRotation = Quaternion.LookRotation(target - _mushroomSpawnPoint.position);
        Quaternion adjustedRotation = Quaternion.Euler(-45f, lookRotation.eulerAngles.y, 0);

        //float distance = Vector3.Distance(target.position, new Vector3(0,0,0));
        float distance = Vector3.Distance(target, new Vector3(0,0,0));
        //float c = target.position.y;
        float c = target.y;
        //float force =Mathf.Sqrt(9.8f*(distance-c/distance)/(Mathf.Sqrt(1f-c*c/(distance*distance))+distance -c/distance));
        float force = Mathf.Sqrt((9.8f * distance * distance) / (distance - c));
        rb.AddForce(adjustedRotation * Vector3.forward * force, ForceMode.VelocityChange);

        StartCoroutine(DeActiveMushroom(mushroom, rb));
    }


    GameObject PoolMushRoom()
    {
        if(_mushrooms.Count > 0) 
            return _mushrooms.Dequeue();
        else return null;
    }

    IEnumerator DeActiveMushroom(GameObject mushroom, Rigidbody rb)
    { 
        yield return new WaitForSeconds(_howLongToAlive);
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        mushroom.SetActive(false);
        _mushrooms.Enqueue(mushroom);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pv = GetComponent<PhotonView>();
        StartCoroutine(FiringMushroom());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddTarget(Transform transform)
    {
        Debug.Log("더하기");
        _targets.Add(transform);
    }
}
