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

    Coroutine _mushroomCoroutine;

    IEnumerator FiringMushroom()
    {

        while (true)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _targets.RemoveAll(target => target == null);
                //한 번에 모든 플레이어에게 날리는 용도
                foreach (Transform target in _targets)
                {
                    pv.RPC(nameof(FireMushroom), RpcTarget.All, target.position);
                }
            }
            yield return new WaitForSeconds(_howLongToWait);
        }
    }

    [PunRPC]
    void FireMushroom(Vector3 target)
    {
        GameObject mushroom=PoolMushRoom();
        if (mushroom == null)
        {
            mushroom = Instantiate(_mushroomPrefab, _mushroomSpawnPoint.position, Quaternion.identity);
            mushroom.transform.SetParent(transform);
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
        _mushroomCoroutine=StartCoroutine(FiringMushroom());
    }

    private void OnDisable()
    {
        base.OnDisable();
        StopCoroutine(_mushroomCoroutine);
    }

    public bool CheckTargetAlreadyContain(Transform transform)
    {
        if(_targets.Contains(transform)) return true;
        return false;
    }
    public void AddTarget(Transform transform)
    {
        if (CheckTargetAlreadyContain(transform))
            return;
        _targets.Add(transform);
    }
}
