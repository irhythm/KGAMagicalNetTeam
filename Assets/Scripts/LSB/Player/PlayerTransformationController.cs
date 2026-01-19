using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class PlayerTransformationController : MonoBehaviourPun
{
    [Header("Transformation")]
    [SerializeField] private ParticleSystem transformEffect;
    [SerializeField] private float transformDuration = 3.0f;

    private PlayableCharacter player;

    public bool IsWizard { get; private set; } = false;
    public bool IsTransforming { get; private set; } = false;

    private Coroutine transformCoroutine;
    private Animator currentAnimator;

    private void Awake()
    {
        player = GetComponent<PlayableCharacter>();
        player.RemoveLayer();
        player.CivilianModel.SetActive(true);
        player.WizardModel.SetActive(false);
        currentAnimator = player.CivilianModel.GetComponent<Animator>();
        IsWizard = false;

        if (player != null)
        {
            player.SetAnimator(currentAnimator);
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

        player.CivilianModel.transform.localRotation = Quaternion.identity;
        player.CivilianModel.transform.localPosition = Vector3.zero;

        if (player.InputHandler != null)
            player.InputHandler.OnPlayerInput();

        currentAnimator.SetBool(player.HashTransform, false);
    }

    private IEnumerator TransformationRoutine()
    {
        IsTransforming = true;
        Debug.Log("변신 시전 중...");

        if (player.InputHandler != null)
            player.InputHandler.OffPlayerInput();

        if (currentAnimator != null)
        {
            currentAnimator.SetBool(player.HashTransform, true);
            currentAnimator.applyRootMotion = true;
        }

        yield return CoroutineManager.waitForSeconds(transformDuration);

        if (currentAnimator != null)
            currentAnimator.applyRootMotion = false;

        player.CivilianModel.transform.localRotation = Quaternion.identity;
        player.CivilianModel.transform.localPosition = Vector3.zero;

        photonView.RPC(nameof(RPC_TransformToWizard), RpcTarget.All);

        IsTransforming = false;
        transformCoroutine = null;

        if (player.InputHandler != null)
            player.InputHandler.OnPlayerInput();
    }

    [PunRPC]
    private void RPC_TransformToWizard()
    {
        IsWizard = true;

        player.CivilianModel.SetActive(false);
        player.WizardModel.SetActive(true);

        currentAnimator = player.WizardModel.GetComponent<Animator>();

        if (transformEffect != null) transformEffect.Play();

        if (player != null)
        {
            player.SetAnimator(currentAnimator);
        }

        GuardManager.instance.NotifyPlayerTransform();
        player.ChangePlayerLayer();
        Debug.Log("마법사로 변신 완료!");
    }
}