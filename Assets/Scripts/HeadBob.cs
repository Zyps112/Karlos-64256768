using System;
using UnityEngine;
using UnityEngine.Rendering;

public class HeadBob : MonoBehaviour
{
    public bool enable = true;

    public float amp = 0.015f;
    public float freq = 10.0f;

    public Transform cam;
    public Transform camHolder;

    public float toggleSpeed = 3.0f;
    public Vector3 startPos;
    public CharacterController playerController;
    public PlayerMovement playerMovementScript;

    private void Awake()
    {
        startPos = cam.localPosition;
    }

    private void Update()
    {
        if (!enable) return;

        CheckMotion();
        ResetPosition();
        cam.LookAt(FocusTarget());
    }


    private void PlayMotion(Vector3 motion)
    {
        cam.localPosition += motion;
    }

    private void CheckMotion()
    {
        float speed = new Vector3(playerController.velocity.x, 0, playerController.velocity.z).magnitude;

        if (speed < toggleSpeed) return;
        if (!playerMovementScript.isGrounded) return;

        PlayMotion(FootStepMotion());
    }
    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * freq) * amp;
        pos.x += Mathf.Cos(Time.time * freq / 2) * amp * 2;
        return pos;
    }

    private void ResetPosition()
    {
        if (cam.localPosition == startPos) return;
        cam.localPosition = Vector3.Lerp(cam.localPosition, startPos, 1 * Time.deltaTime);
    }

    private Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + camHolder.localPosition.y, transform.position.z);
        pos += camHolder.forward * 15.0f;
        return pos;
    }
}
