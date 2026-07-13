using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerGun : NetworkBehaviour
{
    public UnityEvent OnGunShoot;

    public bool automatic;

    public float firerate;

    private NetworkVariable<int> shotsFired = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField]
    private float firerateTimer;

    [Header("References")]
    public Recoil recoil;

    public override void OnNetworkSpawn()
    {
        firerateTimer = firerate;

        shotsFired.OnValueChanged += (int prevValue, int newValue) =>
        {
            Debug.Log(OwnerClientId + ":shots fired" + shotsFired.Value);
        };
    }

    private void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        if(automatic)
        {
            if(Input.GetMouseButton(0))
            {
                if(firerateTimer <= 0)
                {
                    OnGunShoot?.Invoke();
                    recoil.RecoilFire();
                    firerateTimer = firerate;
                    shotsFired.Value += 1;
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
