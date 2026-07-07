using UnityEngine;

public class PlayerGunDamage : MonoBehaviour
{
    public float damage;
    public float range;

    [Header("References")]
    public Transform playerCam;

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
        }
    }
}
