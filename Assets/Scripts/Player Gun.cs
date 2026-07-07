using UnityEngine;
using UnityEngine.Events;

public class PlayerGun : MonoBehaviour
{
    public UnityEvent OnGunShoot;

    public bool automatic;

    public float firerate;

    [SerializeField]
    private float firerateTimer;

    [Header("References")]
    public Recoil recoil;

    private void Start()
    {
        firerateTimer = firerate;
    }

    private void Update()
    {
        if(automatic)
        {
            if(Input.GetMouseButton(0))
            {
                if(firerateTimer <= 0)
                {
                    OnGunShoot?.Invoke();
                    recoil.RecoilFire();
                    firerateTimer = firerate;
                }
            }
        }
        else
        {
            if(Input.GetMouseButtonDown(0))
            {
                if(firerateTimer <= 0)
                {
                    OnGunShoot?.Invoke();
                    recoil.RecoilFire();
                    firerateTimer = firerate;
                }
            }
        }

        if(firerateTimer >= 0)
        {
            firerateTimer -= Time.deltaTime;
        }
    }
}
