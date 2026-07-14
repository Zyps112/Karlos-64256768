using UnityEngine;

public class PickupSystem : MonoBehaviour
{
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
    private Vector3 currentVelocity;

    [Header("Distance control")]
    public float holdDistance = 2f;
    public float minHoldDistance = 1f;
    public float maxHoldDistance = 5f;
    public float scrollSensitivity = 2f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
                TryPickUp();
            else
                Drop();
        }

        if (heldObject != null)
        {
            holdDistance += Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
            holdDistance = Mathf.Clamp(holdDistance, minHoldDistance, maxHoldDistance);
        }
    }

    private void FixedUpdate()
    {
        if (heldObject != null && heldRb != null)
        {
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
    }

    private void TryPickUp()
    {
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxDistance, pickupLayer))
        {
            heldObject = hit.transform;
            heldRb = heldObject.GetComponent<Rigidbody>();

            if (heldRb != null)
            {
                heldRb.linearVelocity = Vector3.zero;
                heldRb.angularVelocity = Vector3.zero;
                heldRb.isKinematic = true;
                heldRb.interpolation = RigidbodyInterpolation.Interpolate; // smooths visual position
            }

            currentVelocity = Vector3.zero;
        }
    }

    private void Drop()
    {
        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.interpolation = RigidbodyInterpolation.None;
        }

        heldObject = null;
        heldRb = null;
    }
}