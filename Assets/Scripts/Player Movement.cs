using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform cam;

    [Header("Config")]
    public float speed;

    public float walkSpeed = 5f;

    public float sprintSpeed = 10f;

    public float gravity = -9.81f;

    public float jumpHeight = 10f;

    public float acceleration = 10f; 
    
    public float deceleration = 15f;

    [Header("Crouch Config")]
    public float standHeight = 2f;

    public float crouchHeight = 1f;

    public float crouchTransitionSpeed = 10f;

    public LayerMask ceilingLayer;

    [Header("Ground Check")]
    public Transform groundCheck;

    public LayerMask groundLayer;

    public float groundDistance = 0.4f;

    public bool isGrounded;

    bool isCrouching;
    Vector3 velocity;
    Vector3 currentMoveVelocity;

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);

        if (isGrounded && velocity.y <= 0)
        {
            velocity.y = 0f;
        }

        HandleCrouch();

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 targetMove = (transform.right * x + transform.forward * z).normalized * speed;

        float rate;
        if (targetMove.magnitude > 0)
        {
            rate = acceleration;
        }
        else
        {
            rate = deceleration;
        }

        currentMoveVelocity = Vector3.MoveTowards(currentMoveVelocity, targetMove, rate * Time.deltaTime);

        controller.Move(currentMoveVelocity * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if(Input.GetKey(KeyCode.LeftShift))
        {
            speed = sprintSpeed;
        }
        else
        {
            speed = walkSpeed;
        }
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