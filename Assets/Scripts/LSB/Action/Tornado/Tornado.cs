using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tornado : MonoBehaviourPun
{
    [Header("Data")]
    [SerializeField] private TornadoSO data;

    private HashSet<Rigidbody> activeTargets = new HashSet<Rigidbody>();
    private int shooterID;
    private Vector3 moveDirection;

    [PunRPC]
    public void RPC_Setup(int shooterID)
    {
        this.shooterID = shooterID;

        if (photonView.IsMine)
        {
            moveDirection = transform.forward;
            moveDirection.y = 0;
            moveDirection.Normalize();
            if (moveDirection == Vector3.zero) moveDirection = Vector3.forward;

            StartCoroutine(LifetimeRoutine());
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * data.rotationSpeed * Time.deltaTime, Space.World);

        if (photonView.IsMine)
        {
            MoveTornado();
        }
    }

    private void LateUpdate()
    {
        if (!photonView.IsMine) return;
        Vector3 currentEuler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, currentEuler.y, 0);
    }

    private void FixedUpdate()
    {
        ControlSatellites();
    }

    private void MoveTornado()
    {
        Vector3 nextPosition = transform.position + (moveDirection * data.moveSpeed * Time.deltaTime);

        RaycastHit hit;
        int layerMask = 1 << LayerMask.NameToLayer("Ground");
        if (layerMask == 0) layerMask = ~0;

        if (Physics.Raycast(nextPosition + Vector3.up * 5.0f, Vector3.down, out hit, 20.0f, layerMask))
        {
            nextPosition.y = hit.point.y;
        }
        transform.position = nextPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Rigidbody>(out Rigidbody rb))
            return;
        if (other.TryGetComponent<PhotonView>(out PhotonView pv) && pv.OwnerActorNr == shooterID)
            return;


        if (!other.TryGetComponent<IPhysicsObject>(out IPhysicsObject obj))
            return;

        if (!activeTargets.Contains(rb))
        {
            activeTargets.Add(rb);

            obj?.OnStatusChange(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && activeTargets.Contains(rb))
        {
            ReleaseTarget(rb, false);
        }
    }

    private void ControlSatellites()
    {
        List<Rigidbody> toRelease = new List<Rigidbody>();

        foreach (var rb in activeTargets)
        {
            if (rb == null) continue;

            Vector3 objectPos = rb.position;
            Vector3 offset = objectPos - transform.position;
            float distance = offset.magnitude;

            if (offset.y > data.releaseHeight)
            {
                toRelease.Add(rb);
                continue;
            }

            Vector3 dirToCenter = -offset.normalized;
            Vector3 horizontalDir = new Vector3(dirToCenter.x, 0, dirToCenter.z).normalized;
            Vector3 tangentDir = Vector3.Cross(horizontalDir, Vector3.up).normalized;

            float distFactor = Mathf.Clamp01(distance / data.maxDistance);
            float currentSuction = Mathf.Lerp(data.suctionSpeed, data.suctionSpeed * 2.5f, distFactor);
            float heightFactor = (offset.y < 2.0f) ? 2.0f : 1.0f;
            float currentLift = data.liftSpeed * heightFactor;

            Vector3 targetVelocity = (tangentDir * data.orbitSpeed) + (horizontalDir * currentSuction);
            Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * data.captureStrength);
            newVelocity.y = currentLift;

            rb.linearVelocity = newVelocity;
        }

        foreach (var rb in toRelease)
        {
            ReleaseTarget(rb, true);
        }
    }

    private void ReleaseTarget(Rigidbody rb, bool applyForce)
    {
        if (activeTargets.Contains(rb))
        {
            activeTargets.Remove(rb);

            if (rb != null)
            {
                rb.useGravity = true;
                rb.GetComponent<IPhysicsObject>()?.OnStatusChange(false);

                if (applyForce)
                {
                    Vector3 explodeDir = (rb.position - transform.position).normalized + Vector3.up;
                    rb.AddForce(explodeDir * data.ejectForce, ForceMode.Impulse);
                }
            }
        }
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(data.duration);

        List<Rigidbody> finalTargets = new List<Rigidbody>(activeTargets);
        foreach (var rb in finalTargets) ReleaseTarget(rb, true);
        activeTargets.Clear();

        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}