using Photon.Pun;
using UnityEngine;

public class PlayerMagicSystem : MonoBehaviourPun
{
    [Header("Magic Settings")]
    public InventoryDataSO LeftHandSlot;
    public InventoryDataSO RightHandSlot;

    private MagicBase _leftMagicLogic;
    private MagicBase _rightMagicLogic;

    [Header("Spawn Points")]
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    [Header("Aiming Config")]
    [SerializeField] private float maxAimDistance = 50f;
    [SerializeField] private float minAimDistance = 2f;
    [SerializeField] private LayerMask aimLayerMask;

    private PlayableCharacter _player;

    private Camera _mainCamera;

    private void Start()
    {
        _player = GetComponent<PlayableCharacter>();
        _mainCamera = Camera.main;
        if (photonView.IsMine)
        {
            if (LeftHandSlot != null)
            {
                _player.Inventory.AddItem(LeftHandSlot);
                EquipItem(LeftHandSlot, true);
            }
            if (RightHandSlot != null)
            {
                _player.Inventory.AddItem(RightHandSlot);
                EquipItem(RightHandSlot, false);
            }
        }
        else
        {
            if (LeftHandSlot is MagicDataSO leftData) _leftMagicLogic = leftData.CreateInstance();
            if (RightHandSlot is MagicDataSO rightData) _rightMagicLogic = rightData.CreateInstance();
        }
    }

    public void CastMagic(bool isLeftHand)
    {
        if (!photonView.IsMine) return;

        MagicBase targetLogic = isLeftHand ? _leftMagicLogic : _rightMagicLogic;
        Transform spawnPoint = isLeftHand ? leftSpawnPoint : rightSpawnPoint;

        if (targetLogic == null || !targetLogic.CanCast()) return;

        Vector3 dir = GetDir(spawnPoint);
        Vector3 spawnPos = spawnPoint.position;

        targetLogic.InitCooldown();
        Debug.Log($"매직 시스템 {(isLeftHand ? "Left" : "Right")} 쿨다운 시작");

        photonView.RPC(nameof(RPC_CastMagic), RpcTarget.All, isLeftHand, spawnPos, dir);
    }

    [PunRPC]
    private void RPC_CastMagic(bool isLeftHand, Vector3 spawnPos, Vector3 direction)
    {
        MagicBase targetLogic = isLeftHand ? _leftMagicLogic : _rightMagicLogic;

        if (targetLogic != null)
        {
            targetLogic.OnCast(spawnPos, direction, isLeftHand);
        }
    }

    private Vector3 GetDir(Transform spawnPoint)
    {
        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, maxAimDistance, aimLayerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(maxAimDistance);
        }

        Vector3 direction = targetPoint - spawnPoint.position;
        float currentDist = direction.magnitude;

        if (currentDist < minAimDistance)
        {
            targetPoint = spawnPoint.position + direction.normalized * minAimDistance;
        }

        return (targetPoint - spawnPoint.position).normalized;
    }

    public void EquipItem(InventoryDataSO item, bool isLeft)
    {
        if (isLeft) LeftHandSlot = item;
        else RightHandSlot = item;

        MagicBase targetLogic = null;

        if (item is MagicDataSO magicData)
        {
            targetLogic = _player.Inventory.GetMagicInstance(magicData);

            if (targetLogic == null)
            {
                Debug.LogWarning($"[System] 인벤토리에 없는 마법을 장착 시도함: {item.itemName}. 새로 생성합니다.");
                targetLogic = magicData.CreateInstance();
            }
        }

        if (isLeft) _leftMagicLogic = targetLogic;
        else _rightMagicLogic = targetLogic;

        Debug.Log($"{(isLeft ? "왼손" : "오른손")} 장착: {item?.itemName}");
    }
}