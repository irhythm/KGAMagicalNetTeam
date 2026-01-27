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

    public void Setup(TornadoSO data, int shooterID)
    {
        if (this.data == null) this.data = data;
        this.shooterID = shooterID;

        moveDirection = transform.forward;
        moveDirection.y = 0;
        moveDirection.Normalize();
        if (moveDirection == Vector3.zero) moveDirection = Vector3.forward;

        if (photonView.IsMine)
        {
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
        if (!photonView.IsMine) return;
        ControlSatellites();
    }

    private void MoveTornado()
    {
        Vector3 nextPosition = transform.position + (moveDirection * data.moveSpeed * Time.deltaTime);

        RaycastHit hit;
        int layerMask = 1 << LayerMask.NameToLayer("Ground");

        if (Physics.Raycast(nextPosition + Vector3.up * 5.0f, Vector3.down, out hit, 20.0f, layerMask))
        {
            nextPosition.y = hit.point.y;
        }
        transform.position = nextPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        if ((data.hitLayer.value & (1 << other.gameObject.layer)) == 0)
            return;
        

        PhotonView targetPv = other.GetComponent<PhotonView>();
        if (targetPv != null && targetPv.OwnerActorNr == shooterID) return;

        if (!activeTargets.Contains(rb))
        {
            activeTargets.Add(rb);

            ChunkNode node = other.GetComponent<ChunkNode>();
            if (node == null) node = other.GetComponentInParent<ChunkNode>();

            if (node != null)
            {
                node.Unfreeze();
            }
            else
            {
                if (rb.isKinematic) rb.isKinematic = false;
            }

            rb.transform.SetParent(null);

            rb.useGravity = false;

            rb.position += Vector3.up * 0.5f;

            other.GetComponent<IPhysicsObject>()?.OnStatusChange(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine) return;

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
            if (rb.isKinematic) rb.isKinematic = false;

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

            float heightFactor = (offset.y < 1.0f) ? 2.0f : 1.0f;
            float currentLift = data.liftSpeed * heightFactor;

            Vector3 targetVelocity = (tangentDir * data.orbitSpeed)
                                   + (horizontalDir * currentSuction);

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
        foreach (var rb in finalTargets)
        {
            ReleaseTarget(rb, true);
        }
        activeTargets.Clear();

        if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
    }
}