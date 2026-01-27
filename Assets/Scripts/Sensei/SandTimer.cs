using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;

public class SandTimer : MonoBehaviourPun
{
    [SerializeField] List<Sprite> _sandTimerSprites;
    [SerializeField] GuardManager _guardManager;
    [SerializeField] Image _image;

    Coroutine _sandDropping;
    Coroutine _flipTimer;
    WaitForSeconds _sandAnimationDelay = new WaitForSeconds(0.15f);
    WaitForSeconds _flipAnimationDelay = new WaitForSeconds(0.02f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        
        yield return new WaitUntil(() => _guardManager.IsTimerRunning);
        _guardManager.OnTimerSecondChanged += RestartSandDropping;
        _guardManager.OnWaveChanged += FlipAnimation;

    }

    void RestartSandDropping(string nouse)
    {
        if (_sandDropping != null)
            StopCoroutine(_sandDropping);
        _sandDropping = StartCoroutine(SandDropping());
    }

    IEnumerator SandDropping()
    {
        for (int i = 0; i < _sandTimerSprites.Count; i++)
        {
            _image.sprite = _sandTimerSprites[i];
            yield return _sandAnimationDelay;
        }
        //yield return _sandAnimationDelay;
        //_image.sprite = _sandTimerSprites[1];
        _sandDropping = null;
    }

    
    void FlipAnimation()
    {
        GetComponent<PhotonView>().RPC("OrderAllFlip", RpcTarget.All);
    }

    [PunRPC]
    void OrderAllFlip()
    {
        if (_flipTimer != null)
        {
            StopCoroutine(_flipTimer);
        }
        _flipTimer = StartCoroutine(Flip());
    }





    IEnumerator Flip()
    {
        _guardManager.OnTimerSecondChanged -= RestartSandDropping;
        if (_sandDropping != null)
        {
            StopCoroutine(_sandDropping);
            _sandDropping = null;
        }

        for (int i = 0; i<36; i++)
        {
            Debug.Log("Flip");
            _image.rectTransform.Rotate(Vector3.forward, 10f);
            yield return _flipAnimationDelay;
        }
        _image.rectTransform.rotation = Quaternion.Euler(0, 0, 0);


        _flipTimer = null;
        
        _guardManager.OnTimerSecondChanged += RestartSandDropping;

    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
