using Photon.Pun;
using System;
using UnityEngine;

public class PlayerMagicSystem : MonoBehaviourPun
{
    [Header("Action Settings")]
    public InventoryDataSO LeftHandSlot;
    public InventoryDataSO RightHandSlot;

    private ActionBase _leftAction;
    private ActionBase _rightAction;

    [Header("Spawn Points")]
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    [Header("Aiming Config")]
    [SerializeField] private float maxAimDistance = 50f;
    [SerializeField] private float minAimDistance = 2f;
    [SerializeField] private LayerMask aimLayerMask;

    private PlayableCharacter _player;
    private Camera _mainCamera;

    public event Action<InventoryDataSO, bool> OnHandItemChanged;
    public event Action<ActionBase, bool> OnHandCooldownStarted;
    public event Action<InventoryDataSO, InventoryDataSO> OnInventoryCooldownCheck;

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
    }

    public void UseAction(bool isLeftHand)
    {
        if (!photonView.IsMine) return;

        ActionBase targetAction = isLeftHand ? _leftAction : _rightAction;
        Transform spawnPoint = isLeftHand ? leftSpawnPoint : rightSpawnPoint;

        if (targetAction == null || !targetAction.CanUse()) return;

        targetAction.InitCooldown();
        Debug.Log($"액션 시스템 {(isLeftHand ? "Left" : "Right")} 쿨다운 시작");

        if (targetAction is MagicAction magic)
        {
            Vector3 dir = GetDir(spawnPoint);
            Vector3 spawnPos = spawnPoint.position;

            RPC_UseMagic(isLeftHand, spawnPos, dir);
        }

        OnHandCooldownStarted?.Invoke(targetAction, isLeftHand);
    }

    private void RPC_UseMagic(bool isLeftHand, Vector3 spawnPos, Vector3 direction)
    {
        ActionBase targetAction = isLeftHand ? _leftAction : _rightAction;

        if (targetAction is MagicAction magic)
        {
            //GuardManager.instance.RegisterMagicNoise(transform.position);
            magic.OnCast(spawnPos, direction, isLeftHand, photonView.OwnerActorNr);
        }
    }

    public ActionBase GetAction(bool isLeftHand)
    {
        return isLeftHand ? _leftAction : _rightAction;
    }

    public bool IsActionReady(bool isLeftHand)
    {
        var action = GetAction(isLeftHand);
        return action != null && action.CanUse();
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
        InventoryDataSO oldItem = isLeft ? LeftHandSlot : RightHandSlot;
        if (oldItem != null)
        {
            OnInventoryCooldownCheck?.Invoke(oldItem, item);
        }

        if (isLeft) LeftHandSlot = item;
        else RightHandSlot = item;

        OnHandItemChanged?.Invoke(item, isLeft);

        ActionBase targetAction = null;

        if (item is ActionItemDataSO actionData)
        {
            targetAction = _player.Inventory.GetActionInstance(actionData);

            if (targetAction == null)
            {
                Debug.LogWarning($"[System] 인벤토리에 없는 액션을 장착 시도함: {item.itemName}. 새로 생성합니다.");
                targetAction = actionData.CreateInstance();
            }

            OnHandCooldownStarted?.Invoke(targetAction, isLeft);
        }

        if (isLeft) _leftAction = targetAction;
        else _rightAction = targetAction;

        Debug.Log($"{(isLeft ? "왼손" : "오른손")} 장착: {item?.itemName}");
    }
}