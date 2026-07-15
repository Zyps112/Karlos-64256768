using UnityEngine;

public class PlayerGunDamage : MonoBehaviour
{
    public float damage;
    public float range;
    public float knockback;

    [Header("References")]
    public Transform playerCam;

    public GameObject bulletImpact;

    public void Shoot()
    {
        Ray ray = new Ray(playerCam.position, playerCam.forward);
        if(Physics.Raycast(ray, out RaycastHit hit, range))
        {
            if(hit.collider.gameObject.TryGetComponent(out Entity enemy))
            {
                enemy.health -= damage;
                Debug.Log(enemy.health);
            }

            if(hit.collider.gameObject.TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce(playerCam.TransformDirection(Vector3.forward) * knockback, ForceMode.Impulse);
            }

            if (LayerMask.LayerToName(hit.collider.gameObject.layer) == "Ground")
            {
                Vector3 spawnPos = hit.point + hit.normal * 0.01f;
                Quaternion spawnRot = Quaternion.FromToRotation(Vector3.up, hit.normal);

                GameObject impact = Instantiate(bulletImpact, spawnPos, spawnRot, hit.transform);
                Destroy(impact, 5f);
            }

        }
    }
}
