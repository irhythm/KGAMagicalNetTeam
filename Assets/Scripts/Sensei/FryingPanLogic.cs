using UnityEngine;
using System.Collections;
//using Photon.Pun;
using System.Collections.Generic;

public class FryingPanLogic : MonoBehaviour
{
    [SerializeField] GameObject _mushroomPrefab;
    [SerializeField] Transform _mushroomSpawnPoint;
    [SerializeField] float _howLongToWait = 2f;

    [SerializeField] List<Transform> _targets = new List<Transform>();


    IEnumerator FiringMushroom()
    {
        yield return null;
        foreach (Transform target in _targets)
        {
            int i = 0;
            while (i<10)
            {
                //photonView.RPC("FireMushroom", RpcTarget.All, target);
                
                FireMushroom(target);
                i++;
                yield return new WaitForSeconds(_howLongToWait);
            }


            //MushroomLogic mushroomLogic = mushroom.GetComponent<MushroomLogic>();
            //mushroomLogic.SetTarget(target);
            //yield return new WaitForSeconds(0.2f);
        }





    }

    //[PunRPC]
    void FireMushroom(Transform target)
    {
        GameObject mushroom = Instantiate(_mushroomPrefab, _mushroomSpawnPoint.position, Quaternion.identity);
        
        Rigidbody rb = mushroom.GetComponent<Rigidbody>();
        Quaternion lookRotation = Quaternion.LookRotation(target.position - _mushroomSpawnPoint.position);
        Quaternion adjustedRotation = Quaternion.Euler(-45f, lookRotation.eulerAngles.y, 0);

        float distance = Vector3.Distance(target.position, new Vector3(0,0,0));
        float c = target.position.y;
        //float force =Mathf.Sqrt(9.8f*(distance-c/distance)/(Mathf.Sqrt(1f-c*c/(distance*distance))+distance -c/distance));
        float force = Mathf.Sqrt((9.8f * distance * distance) / (distance - c));
        rb.AddForce(adjustedRotation * Vector3.forward * force, ForceMode.VelocityChange);


    }




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(FiringMushroom());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
