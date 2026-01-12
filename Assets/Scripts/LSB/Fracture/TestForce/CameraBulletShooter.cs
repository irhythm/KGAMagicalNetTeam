using UnityEngine;
using UnityEngine.InputSystem;

public class CameraBulletShooter : MonoBehaviour
{
    [SerializeField] private float bulletRadius = 0.05f;
    [SerializeField] private float bulletSpeed = 1000f;
    [SerializeField] private float bulletMass = 0.5f;

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Fire();
        }
    }

    private void Fire()
    {
        var bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.transform.position = transform.position + transform.forward * 0.5f;
        bullet.transform.localScale = Vector3.one * bulletRadius;

        var rb = bullet.AddComponent<Rigidbody>();
        rb.mass = bulletMass;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearVelocity = transform.forward * bulletSpeed;

        Destroy(bullet, 3f);
    }
}