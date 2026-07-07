using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public Transform cam;
    public Camera mainCamera;
    public Transform guntip;
    public LayerMask shootableLayers;
    public Recoil recoilScript;
    public Animations_PlaceHolder animScript;

    public bool isAiming;

    public float distanceToSwitchToGun;

    public float maxDistance;
    public float minDistance;

    public float knockbackForce;

    public float firerate;
    float firerateTimer;

    bool switchToGun;
    bool isShooting;

    Transform rayTransform;

    private void Start()
    {
        rayTransform = cam;
    }

    private void Update()
    {
        Shoot();
        animScript.FireAnim(isShooting);

        RaycastHit hit;
        if(switchToGun == false)
        {
            if(Physics.Raycast(cam.position, cam.TransformDirection(Vector3.forward), out hit, maxDistance, shootableLayers))
            {
                Debug.DrawRay(cam.position, cam.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                if(hit.distance <= distanceToSwitchToGun)
                {
                    rayTransform = guntip;
                }
                else
                {
                    rayTransform = cam;
                }

                int layer = hit.collider.gameObject.layer;
                string layerName = LayerMask.LayerToName(layer);

                if(layerName == "Shootable Physics Objects")
                {
                    if(isShooting)
                    {
                        hit.rigidbody.AddForce(cam.TransformDirection(Vector3.forward) * knockbackForce, ForceMode.Impulse);
                    }
                }

                if(layerName == "Enemy")
                {
                    if(isShooting)
                    {
                        hit.rigidbody.AddForce(cam.TransformDirection(Vector3.forward) * knockbackForce, ForceMode.Impulse);
                    }
                }
            }
        }

        if (Physics.Raycast(guntip.position, guntip.TransformDirection(Vector3.forward), out hit, minDistance, shootableLayers))
        {
            Debug.DrawRay(guntip.position, guntip.TransformDirection(Vector3.forward) * hit.distance, Color.red);
            switchToGun = true;
            if(switchToGun)
            {
                int layer = hit.collider.gameObject.layer;
                string layerName = LayerMask.LayerToName(layer);

                if (layerName == "Shootable Physics Objects")
                {
                    if(isShooting)
                    {
                        hit.rigidbody.AddForce(guntip.TransformDirection(Vector3.forward) * knockbackForce, ForceMode.Impulse);
                    }
                }

                if(layerName == "Enemy")
                {
                    if(isShooting)
                    {
                        hit.rigidbody.AddForce(guntip.TransformDirection(Vector3.forward) * knockbackForce, ForceMode.Impulse);
                    }
                }
            }
        }
        else
        {
            switchToGun = false;
        }
    }

    public void Shoot()
    {
        if(Input.GetMouseButton(0))
        {
            recoilScript.RecoilFire();
            isShooting = true;
        }
        else
        {
            isShooting = false;
        }

        if (Input.GetMouseButton(1))
        {
            isAiming = true;
            mainCamera.fieldOfView = Mathf.Lerp(
                mainCamera.fieldOfView,
                40f,
                Time.deltaTime * 10f
            );
        }
        else
        {
            isAiming = false;
            mainCamera.fieldOfView = Mathf.Lerp(
                mainCamera.fieldOfView,
                60f,
                Time.deltaTime * 10f
            );
        }
    }

    public void FireRate()
    {
        if (isShooting && firerateTimer == firerate)
        {
            isShooting = false;
        }
    }
}
