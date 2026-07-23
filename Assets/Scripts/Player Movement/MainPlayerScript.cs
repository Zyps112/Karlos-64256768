using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class MainPlayerScript : NetworkBehaviour
{
    [Header("Gound Movement Config")]
    [SerializeField] private float speed;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float gravity = -29.43f;
    [SerializeField] private float jumpHeight = 2.5f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 30f;

    [Header("Air Movement Config")]
    [SerializeField] private float airAcceleration = 8f;
    [SerializeField] private float airDeceleration = 7f;
    [SerializeField] private float maxAirSpeed = 10f;

    [Header("Air Movement Config")]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private LayerMask ceilingLayer;

    [Header("Camera Config")]
    [SerializeField] private float baseFov = 60f;
    [SerializeField] private float fovMultiplayer = 1.5f;
    [SerializeField] private bool isCrouching;

    [Header("Ground Check")]
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private bool isGrounded;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;

    [Header("Refereces")]
    [SerializeField] CharacterController controller;
    [SerializeField] AudioListener audioListener;
    [SerializeField] Camera playerCam;
    [SerializeField] Transform cam;

    [Header("Input")]
    public KeyCode jumpKey;
    public KeyCode crouchKey;
    public KeyCode sprintKey;

    Vector3 velocity;
    Vector3 currentVelocity;

    public override void OnNetworkSpawn()
    {
        if(!IsOwner)
        {
            if(playerCam != null) playerCam.gameObject.SetActive(false);

            if(audioListener != null ) audioListener.enabled = false;

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

        HandleMovement();
        HandleJump();
        HandleSprint();
        HandleCrouch();

        GroundCheck();

        PlayerCamera();
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
        
        if(isGrounded && velocity.y <= 0)
        {
            velocity.y = 0f;
        }
    }

    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 targetMovement = (transform.right * x + transform.forward * z).normalized * speed;

        float rate;
        if(isGrounded)
        {
            rate = targetMovement.magnitude > 0 ? acceleration : deceleration;
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetMovement, rate * Time.deltaTime);
        }
        else
        {
            rate = targetMovement.magnitude > 0 ? airAcceleration : airDeceleration;
            Vector3 targetAirMovement = Vector3.ClampMagnitude(targetMovement, maxAirSpeed);
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetAirMovement, rate * Time.deltaTime);
        }

        controller.Move(velocity * Time.deltaTime);

        controller.Move(currentVelocity * Time.deltaTime);

        currentVelocity = Vector3.MoveTowards(currentVelocity, targetMovement, rate * Time.deltaTime);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(jumpKey))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
    }

    void HandleSprint()
    {
        speed = Input.GetKey(sprintKey) ? sprintSpeed : walkSpeed;
    }

    void PlayerCamera()
    {
        float velocityClamped = Mathf.Clamp(controller.velocity.magnitude, 0.5f, sprintSpeed * 2.0f);
        float targetFov = baseFov + fovMultiplayer * velocityClamped;
        playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, targetFov, Time.deltaTime * 10f);
    }

    void HandleCrouch()
    {
        bool wantsToCrouch = Input.GetKey(crouchKey);

        if(isCrouching && !wantsToCrouch)
        {
            bool ceilingAbove = Physics.CheckSphere(transform.position + Vector3.up * standHeight, 0.2f, ceilingLayer);

            if (ceilingAbove) wantsToCrouch = false;
        }

        isCrouching = wantsToCrouch;

        float targetHeight = isCrouching ? crouchHeight : standHeight;

        float currentHeight = controller.height;
        float newHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        float heightDiff = currentHeight - newHeight;
        controller.height = newHeight;
        controller.center = new Vector3(0, controller.center.y - (heightDiff / 2), 0);

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
