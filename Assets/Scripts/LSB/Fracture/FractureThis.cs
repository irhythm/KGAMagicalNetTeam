using UnityEngine;

public class FractureThis : MonoBehaviour
{
    [SerializeField] private Anchor anchor = Anchor.Bottom;
    [SerializeField] private int chunks = 500;
    [SerializeField] private float density = 50;
    [SerializeField] private float internalStrength = 100;

    [SerializeField] private Material insideMaterial;
    [SerializeField] private Material outsideMaterial;

    private void Start()
    {
        FractureGameobject();
        gameObject.SetActive(false);
    }

    public ChunkGraphManager FractureGameobject()
    {
        return Fracture.FractureGameObject(
            gameObject,
            anchor,
            1594236506, // 고정 시드 유지
            chunks,
            insideMaterial,
            outsideMaterial,
            internalStrength,
            density
        );
    }
}