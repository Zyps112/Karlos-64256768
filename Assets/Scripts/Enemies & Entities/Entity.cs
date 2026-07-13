using UnityEngine;

public class Entity : MonoBehaviour
{
    public float health;

    private void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
