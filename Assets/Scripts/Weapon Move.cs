using UnityEngine;

public class WeaponMove : MonoBehaviour
{
    public float rotationLag = 6f;
    public float swayAmount = 2f;

    private Quaternion targetLocalRot;

    private void Awake()
    {
        targetLocalRot = transform.localRotation;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Quaternion swayX = Quaternion.AngleAxis(-mouseY * swayAmount, Vector3.right);
        Quaternion swayY = Quaternion.AngleAxis(mouseX * swayAmount, Vector3.up);

        Quaternion targetRot = targetLocalRot * swayX * swayY;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, rotationLag * Time.deltaTime);
    }
}