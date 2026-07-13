using UnityEngine;
using UnityEngine.Rendering;
using Unity.Netcode;
public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform cam;
    public Camera playerCam;
    public AudioListener audioListener; // add this if you have one

    [Header("Ground Movement Config")]
    public float speed;

    public float walkSpeed = 5f;

    public float sprintSpeed = 10f;

    public float gravity = -9.81f;

    public float jumpHeight = 10f;

    public float acceleration = 10f; 
    
    public float deceleration = 15f;

    [Header("Air Movement Config")]
    public float airAcceleration = 2f;

    public float airDeceleration = 1f;

    public float maxAirSpeed = 10f;

    [Header("Crouch Config")]
    public float standHeight = 2f;

    public float crouchHeight = 1f;

    public float crouchTransitionSpeed = 10f;

    public LayerMask ceilingLayer;

    [Header("Camera Config")]
    public float baseFOV = 60f;

    public float sprintFOVMultiplier = 1.5f;

    [Header("Ground Check")]
    public Transform groundCheck;

    public LayerMask groundLayer;

    public float groundDistance = 0.4f;

    public bool isGrounded;

    bool isCrouching;
    Vector3 velocity;
    Vector3 currentMoveVelocity;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (playerCam != null) playerCam.gameObject.SetActive(false);
            if (audioListener != null) audioListener.enabled = false;
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        HandleCrouch();

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

        if (isGrounded && velocity.y <= 0)
        {
            velocity.y = 0f;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 targetMove = (transform.right * x + transform.forward * z).normalized * speed;

        float rate;
        if(isGrounded)
        {
            rate = targetMove.magnitude > 0 ? acceleration : deceleration;
            currentMoveVelocity = Vector3.MoveTowards(currentMoveVelocity, targetMove, rate * Time.deltaTime);
        }
        else
        {
            rate = targetMove.magnitude > 0 ? airAcceleration : airDeceleration;
            Vector3 airTargetMove = Vector3.ClampMagnitude(targetMove, maxAirSpeed);
            currentMoveVelocity = Vector3.MoveTowards(currentMoveVelocity, airTargetMove, rate * Time.deltaTime);
        }

            controller.Move(velocity * Time.deltaTime);

        currentMoveVelocity = Vector3.MoveTowards(currentMoveVelocity, targetMove, rate * Time.deltaTime);

        controller.Move(currentMoveVelocity * Time.deltaTime);



        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);

        velocity.y += gravity * Time.deltaTime;

        if(Input.GetKey(KeyCode.LeftShift))
        {
            speed = sprintSpeed;
        }
        else
        {
            speed = walkSpeed;
        }

        float velocityClampled = Mathf.Clamp(controller.velocity.magnitude, 0.5f, sprintSpeed * 2.0f);
        float targetFOV = baseFOV + sprintFOVMultiplier * velocityClampled;
        playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, targetFOV, Time.deltaTime * 10.0f);
    }

    void HandleCrouch()
    {
        bool wantsToCrouch = Input.GetKey(KeyCode.LeftControl);

        if (isCrouching && !wantsToCrouch)
        {
            bool ceilingAbove = Physics.CheckSphere(
                transform.position + Vector3.up * standHeight,
                0.2f,
                ceilingLayer
            );
            if (ceilingAbove) wantsToCrouch = true;
        }

        isCrouching = wantsToCrouch;

        float targetHeight = isCrouching ? crouchHeight : standHeight;

        float currentHeight = controller.height;
        float newHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        float heightDiff = currentHeight - newHeight;
        controller.height = newHeight;
        controller.center = new Vector3(0, controller.center.y - (heightDiff / 2), 0);

        // Move camera to top of controller, smoothly
        float targetCamY = controller.center.y + (controller.height / 2) - 0.1f;
        float newCamY = Mathf.Lerp(cam.localPosition.y, targetCamY, crouchTransitionSpeed * Time.deltaTime);
        cam.localPosition = new Vector3(cam.localPosition.x, newCamY, cam.localPosition.z);

        if (isCrouching)
            speed = Mathf.Min(speed, walkSpeed * 0.5f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}