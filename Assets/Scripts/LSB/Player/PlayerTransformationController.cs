using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class PlayerTransformationController : MonoBehaviourPun
{
    [Header("Transformation")]
    [SerializeField] private ParticleSystem transformEffect;
    [SerializeField] private float transformDuration = 3.0f;

    private PlayableCharacter playableCharater;

    public bool IsWizard { get; private set; } = false;
    public bool IsTransforming { get; private set; } = false;

    private Coroutine transformCoroutine;
    private Animator currentAnimator;

    private void Awake()
    {
        playableCharater = GetComponent<PlayableCharacter>();
        playableCharater.RemoveLayer();
        playableCharater.CivilianModel.SetActive(true);
        playableCharater.WizardModel.SetActive(false);
        currentAnimator = playableCharater.CivilianModel.GetComponent<Animator>();
        IsWizard = false;

        if (playableCharater != null)
        {
            playableCharater.SetAnimator(currentAnimator);
        }
    }

    public void HandleTransformInput(bool isKeyPressed)
    {
        if (IsWizard || !photonView.IsMine) return;

        if (isKeyPressed)
        {
            if (transformCoroutine == null)
            {
                transformCoroutine = StartCoroutine(TransformationRoutine());
            }
        }
        else
        {
            if (transformCoroutine != null)
            {
                StopCoroutine(transformCoroutine);
                transformCoroutine = null;
                CancelTransformation();
            }
        }
    }

    private void CancelTransformation()
    {
        IsTransforming = false;
        Debug.Log("변신 취소");

        

        if (currentAnimator != null)
            currentAnimator.applyRootMotion = false;

        playableCharater.CivilianModel.transform.localRotation = Quaternion.identity;
        playableCharater.CivilianModel.transform.localPosition = Vector3.zero;

        if (playableCharater.InputHandler != null)
            playableCharater.InputHandler.OnPlayerInput();

        currentAnimator.SetBool(playableCharater.HashTransform, false);
    }

    private IEnumerator TransformationRoutine()
    {
        IsTransforming = true;
        Debug.Log("변신 시전 중...");

        if (playableCharater.InputHandler != null)
            playableCharater.InputHandler.OffPlayerInput();

        if (currentAnimator != null)
        {
            currentAnimator.SetBool(playableCharater.HashTransform, true);
            currentAnimator.applyRootMotion = true;
        }

        yield return CoroutineManager.waitForSeconds(transformDuration);

        if (currentAnimator != null)
            currentAnimator.applyRootMotion = false;

        playableCharater.CivilianModel.transform.localRotation = Quaternion.identity;
        playableCharater.CivilianModel.transform.localPosition = Vector3.zero;

        photonView.RPC(nameof(RPC_TransformToWizard), RpcTarget.All);


        PhotonNetwork.LocalPlayer.SetProps(NetworkProperties.PLAYER_ISWIZARD, true);

        IsTransforming = false;
        transformCoroutine = null;

        if (playableCharater.InputHandler != null)
            playableCharater.InputHandler.OnPlayerInput();
    }

    [PunRPC]
    private void RPC_TransformToWizard()
    {
        TransformToWizard();
    }

    public void TransformToWizard()
    {
        IsWizard = true;

        playableCharater.CivilianModel.SetActive(false);
        playableCharater.WizardModel.SetActive(true);

        currentAnimator = playableCharater.WizardModel.GetComponent<Animator>();

        if (transformEffect != null) transformEffect.Play();

        if (playableCharater != null)
        {
            playableCharater.SetAnimator(currentAnimator);
        }

        GuardManager.instance?.NotifyPlayerTransform();
        playableCharater.ChangePlayerLayer();
        Debug.Log("마법사로 변신 완료!");
    }
}