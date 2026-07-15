using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerGun : NetworkBehaviour
{
    public UnityEvent OnGunShoot;

    public bool automatic;

    public float firerate;

    public float aimSpeed;

    public int bullets;

    public int maxBullets;

    public bool canShoot;

    public bool haveBullets;

    private NetworkVariable<int> shotsFired = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private float firerateTimer;

    [SerializeField] private Vector3 aimPosition = new Vector3(0.005f, -0.109f, 0.787f);

    private bool isAiming;

    private Vector3 gunNormalPosition;

    [Header("References")]
    public Recoil recoil;

    public Animator gunAnimator;

    public Transform gunObject;

    public PickupSystem pickupScript;

    public SkinnedMeshRenderer gunRenderer;

    public override void OnNetworkSpawn()
    {
        firerateTimer = firerate;

        gunNormalPosition = transform.localPosition;

        shotsFired.OnValueChanged += (int prevValue, int newValue) =>
        {
            Debug.Log(OwnerClientId + ":shots fired" + shotsFired.Value);
        };
    }

    public void AddBullets(int amount)
    {
        bullets = Mathf.Min(bullets + amount, maxBullets);
        if(bullets >= maxBullets)
        {
            bullets = maxBullets;
        }
    }

    private void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        if(automatic)
        {
            if(Input.GetMouseButton(0) && isAiming && canShoot && haveBullets)
            {
                if(firerateTimer <= 0)
                {
                    OnGunShoot?.Invoke();
                    recoil.RecoilFire();
                    bullets--;
                    gunAnimator.Play("Fire", -1, 0f);
                    firerateTimer = firerate;
                    shotsFired.Value += 1;
                }
            }
        }
        else
        {
            if(Input.GetMouseButtonDown(0) && isAiming && canShoot && haveBullets)
            {
                if(firerateTimer <= 0)
                {
                    OnGunShoot?.Invoke();
                    recoil.RecoilFire();
                    bullets--;
                    gunAnimator.Play("Fire", -1, 0f);
                    firerateTimer = firerate;
                }
            }
        }

        if(firerateTimer >= 0)
        {
            firerateTimer -= Time.deltaTime;
        }

        if (Input.GetMouseButton(1) && canShoot)
        {
            isAiming = true;
            gunObject.localPosition = Vector3.Lerp(
                gunObject.localPosition,
                aimPosition,
                Time.deltaTime * aimSpeed
            );
        }
        else
        {
            isAiming = false;
            gunObject.localPosition = Vector3.Lerp(
                gunObject.localPosition,
                gunNormalPosition,
                Time.deltaTime * aimSpeed
            );
        }

        if(bullets <= 0)
        {
            haveBullets = false;
        }
        else
        {
            haveBullets = true;
        }

        if(pickupScript.HoldingObject)
        {
            canShoot = false;
            gunRenderer.enabled = false;
        }
        else if(!pickupScript.HoldingObject)
        {
            canShoot = true;
            gunRenderer.enabled = true;
        }
    }
}
