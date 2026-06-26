using UnityEngine;

public class Recoil : MonoBehaviour
{
    public PlayerWeapon weaponScript;

    bool isAiming;

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    public float recoilX;
    public float recoilY;
    public float recoilZ;

    public float aimRecoilX;
    public float aimRecoilY;
    public float aimRecoilZ;

    public float snappiness;
    public float returnSpeed;

    private void Start()
    {
        
    }

    private void Update()
    {
        isAiming = weaponScript.isAiming;

        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire()
    {
        if(isAiming)
        {
            targetRotation += new Vector3(aimRecoilX, Random.Range(-aimRecoilY, aimRecoilY), Random.Range(-aimRecoilZ, aimRecoilZ));
        }
        else
        {
            targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));

        }
    }
}
