using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float Health;
    
    public void TakeDamage(float damage)
    {
        Health -= damage;
    }

    void Update()
    {
        if(Health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
