using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public Transform cam;
    public Transform guntip;
    public LayerMask shootableLayers;

    public float distanceToSwitchToGun;

    public float maxDistance;
    public float minDistance;

    public float knockbackForce;

    bool switchToGun;

    Transform rayTransform;

    private void Start()
    {
        rayTransform = cam;
    }

    private void Update()
    {
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
                    if(Input.GetMouseButtonDown(0))
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
                    if (Input.GetMouseButtonDown(0))
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
}
