using System;
using Unity.Netcode;
using UnityEngine;

public class Bullets : NetworkBehaviour
{
    public int bullets;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // if this is a NetworkBehaviour

        PlayerGun playerGunScript = other.GetComponentInChildren<PlayerGun>();
        if (playerGunScript != null)
        {
            if(playerGunScript.bullets != playerGunScript.maxBullets)
            {
                playerGunScript.AddBullets(bullets);
                NetworkObject.Despawn(false);
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("Have Enough bulllets");
            }
        }
    }
}
