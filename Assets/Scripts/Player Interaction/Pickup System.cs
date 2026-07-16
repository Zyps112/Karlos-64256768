using UnityEngine;
using Unity.Netcode;

public class PickupSystem : NetworkBehaviour
{
    public bool HoldingObject;
    public LayerMask pickupLayer;
    public Transform cam;
    public float maxDistance;
    public Transform objectHolder;

    [Header("Floaty movement")]
    public float smoothTime = 0.15f;
    public float bobAmount = 0.05f;
    public float bobSpeed = 2f;

    private Transform heldObject;
    private Rigidbody heldRb;
    private NetworkObject heldNetObj;      // set on whichever side needs it (client sets its own via RPC, server sets its own directly)
    private Pickupable heldPickupable;     // server-side reference so drop doesn't depend on client-only state
    private Vector3 currentVelocity;

    [Header("Distance control")]
    public float holdDistance = 2f;
    public float minHoldDistance = 1f;
    public float maxHoldDistance = 5f;
    public float scrollSensitivity = 2f;

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
                TryPickUp();
            else
                RequestDropServerRpc();
        }

        if (heldObject != null)
        {
            holdDistance += Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
            holdDistance = Mathf.Clamp(holdDistance, minHoldDistance, maxHoldDistance);
        }
    }

    private void FixedUpdate()
    {
        // Only the client currently holding (and owning) the object moves it
        if (!IsOwner || heldObject == null || heldRb == null) return;

        Vector3 targetPos = cam.position + cam.forward * holdDistance;
        targetPos.y += Mathf.Sin(Time.time * bobSpeed) * bobAmount;

        Vector3 newPos = Vector3.SmoothDamp(
            heldRb.position,
            targetPos,
            ref currentVelocity,
            smoothTime
        );
        heldRb.MovePosition(newPos);
    }

    private void TryPickUp()
    {
        if (cam == null)
        {
            Debug.LogError("[PickupSystem] cam is not assigned!");
            return;
        }

        Debug.Log($"[PickupSystem] Raycasting from {cam.position} forward {cam.forward}, maxDistance={maxDistance}, layer={pickupLayer.value}");

        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxDistance, pickupLayer))
        {
            Debug.Log($"[PickupSystem] Raycast hit: {hit.transform.name}");

            NetworkObject netObj = hit.transform.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogWarning($"[PickupSystem] {hit.transform.name} has no NetworkObject component.");
                return;
            }

            RequestPickupServerRpc(netObj.NetworkObjectId);
        }
        else
        {
            Debug.Log("[PickupSystem] Raycast hit nothing. Check maxDistance, pickupLayer, and that the object's Layer matches.");
        }
    }

    [ServerRpc]
    private void RequestPickupServerRpc(ulong targetNetworkObjectId, ServerRpcParams rpcParams = default)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject targetNetObj))
        {
            Debug.LogWarning($"[PickupSystem][Server] No spawned NetworkObject found for id {targetNetworkObjectId}");
            return;
        }

        // Reject anything not explicitly marked as pickupable
        if (!targetNetObj.TryGetComponent<Pickupable>(out Pickupable pickupable))
        {
            Debug.LogWarning($"[PickupSystem][Server] {targetNetObj.name} rejected: missing Pickupable component.");
            return;
        }

        // Reject if it's already held by someone (tracked explicitly, not via transform.parent -
        // objects can have a legitimate scene parent without being "held")
        if (pickupable.IsHeld)
        {
            Debug.LogWarning($"[PickupSystem][Server] {targetNetObj.name} rejected: already held.");
            return;
        }

        // Reject players/other non-prop objects just in case a stray NetworkObject ID is sent
        if (targetNetObj.CompareTag("Player"))
        {
            Debug.LogWarning($"[PickupSystem][Server] {targetNetObj.name} rejected: tagged as Player.");
            return;
        }

        // Server-side distance check - don't trust the client's raycast range
        float distance = Vector3.Distance(transform.position, targetNetObj.transform.position);
        if (distance > maxDistance + 1f) // small buffer for latency/positional drift
        {
            Debug.LogWarning($"[PickupSystem][Server] {targetNetObj.name} rejected: distance {distance} > maxDistance {maxDistance + 1f}.");
            return;
        }

        Debug.Log($"[PickupSystem][Server] {targetNetObj.name} passed all checks, picking up.");

        ulong requesterId = rpcParams.Receive.SenderClientId;

        // Mark as held immediately so a second pickup request can't race in before the client callback returns
        pickupable.IsHeld = true;
        heldNetObj = targetNetObj;      // server's own reference, used by RequestDropServerRpc
        heldPickupable = pickupable;

        // Give ownership to the requesting client so their local movement has authority
        targetNetObj.ChangeOwnership(requesterId);

        // Reparent on the server so it replicates to all clients
        targetNetObj.TrySetParent(objectHolder, false);

        // Tell only the requesting client to set up local physics/held state
        SetupHeldObjectClientRpc(targetNetworkObjectId, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { requesterId } }
        });
    }

    [ClientRpc]
    private void SetupHeldObjectClientRpc(ulong targetNetworkObjectId, ClientRpcParams rpcParams = default)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject targetNetObj))
            return;

        // Note: on a host, this ClientRpc and the server-side pickup code above both run in the
        // same process and will both assign heldNetObj on this same instance - that's fine, they
        // agree on the same object. On a dedicated server the server never receives this ClientRpc,
        // which is why the server sets its own heldNetObj/heldPickupable directly in the ServerRpc.
        heldObject = targetNetObj.transform;
        heldNetObj = targetNetObj;
        heldRb = heldObject.GetComponent<Rigidbody>();

        if (heldRb != null)
        {
            HoldingObject = true;
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
            heldRb.isKinematic = true;
            heldRb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        currentVelocity = Vector3.zero;
    }

    [ServerRpc]
    private void RequestDropServerRpc(ServerRpcParams rpcParams = default)
    {
        // Uses the server's own tracked reference (set directly in RequestPickupServerRpc),
        // not the client-only heldNetObj, so this works correctly on a dedicated server too.
        if (heldNetObj == null)
        {
            Debug.LogWarning("[PickupSystem][Server] Drop requested but server has no heldNetObj tracked.");
            return;
        }

        heldNetObj.TrySetParent((Transform)null, true);

        if (heldPickupable != null)
            heldPickupable.IsHeld = false;

        // Optional: hand ownership back to the server once dropped.
        // Comment this out if you want the last holder to keep authority (e.g. for physics settling).
        heldNetObj.RemoveOwnership();

        DropClientRpc();

        heldNetObj = null;
        heldPickupable = null;
    }

    [ClientRpc]
    private void DropClientRpc()
    {
        if (heldRb != null)
        {
            HoldingObject = false;
            heldRb.isKinematic = false;
            heldRb.interpolation = RigidbodyInterpolation.None;
        }

        heldObject = null;
        heldRb = null;
        // Note: do not null heldNetObj here on the server's own instance - it's already been
        // cleared in RequestDropServerRpc right before this ClientRpc was sent. On the client,
        // clearing it here is correct and intended.
        heldNetObj = null;
    }
}