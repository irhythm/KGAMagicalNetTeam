using UnityEngine;

public class PlayerMagicSystem : MonoBehaviour
{
    [Header("Magic Settings")]
    public InventoryData LeftHandSlot;
    public InventoryData RightHandSlot;

    [Header("Spawn Points")]
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    [Header("Aiming Config")]
    [SerializeField] private float maxAimDistance = 50f;
    [SerializeField] private float minAimDistance = 2f;
    [SerializeField] private LayerMask aimLayerMask;

    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    public void CastMagic(bool isLeftHand)
    {
        InventoryData magic = isLeftHand ? LeftHandSlot : RightHandSlot;
        Transform spawnPoint = isLeftHand ? leftSpawnPoint : rightSpawnPoint;

        if (magic == null || spawnPoint == null) return;

        Vector3 Dir = GetDir(spawnPoint);

        //TODO: ÄðÅ¸ÀÓ ·ÎÁ÷
        if(magic is MagicData)
        {
            (magic as MagicData)?.OnCast(spawnPoint.position, Dir, isLeftHand);
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

    public void EquipItem(InventoryData item, bool isLeft)
    {
        if (isLeft) LeftHandSlot = item;
        else RightHandSlot = item;

        Debug.Log($"{(isLeft ? "ÁÂÃø(Q)" : "¿ìÃø(E)")} ½½·Ô ÀåÂø: {item.itemName}");
    }
}