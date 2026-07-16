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

    [SerializeField] private float mussleFlashTimer;

    [SerializeField] private float mussleFlashDuration = 0.05f;

    private bool isAiming;

    private Vector3 gunNormalPosition;

    [Header("References")]
    public Recoil recoil;

    public Animator gunAnimator;

    public Transform gunObject;

    public PickupSystem pickupScript;

    public SkinnedMeshRenderer gunRenderer;

    public GameObject mussleFlash;

    public Transform gunTip;

    public override void OnNetworkSpawn()
    {
        firerateTimer = firerate;

        gunNormalPosition = transform.localPosition;

        // Make sure the flash starts hidden
        if (mussleFlash != null)
        {
            mussleFlash.transform.localScale = Vector3.zero;
        }

        shotsFired.OnValueChanged += (int prevValue, int newValue) =>
        {
            Debug.Log(OwnerClientId + ":shots fired" + shotsFired.Value);
        };
    }

    public void AddBullets(int amount)
    {
        bullets = Mathf.Min(bullets + amount, maxBullets);
        if (bullets >= maxBullets)
        {
            bullets = maxBullets;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        HandleShooting();
        HandleFirerateTimer();
        HandleMussleFlashTimer();
        HandleAiming();
        HandleBulletState();
        HandlePickupState();
    }

    private void HandleShooting()
    {
        bool wantsToShoot = automatic
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (wantsToShoot && isAiming && canShoot && haveBullets && firerateTimer <= 0)
        {
            Fire();
        }
    }

    private void Fire()
    {
        OnGunShoot?.Invoke();
        recoil.RecoilFire();
        bullets--;
        gunAnimator.Play("Fire", -1, 0f);
        firerateTimer = firerate;

        if (automatic)
        {
            shotsFired.Value += 1;
        }

        ShowMussleFlash();
    }

    private void ShowMussleFlash()
    {
        if (mussleFlash == null)
        {
            return;
        }

        mussleFlash.transform.localScale = Vector3.one;
        mussleFlashTimer = mussleFlashDuration;
    }

    private void HandleFirerateTimer()
    {
        if (firerateTimer >= 0)
        {
            firerateTimer -= Time.deltaTime;
        }
    }

    private void HandleMussleFlashTimer()
    {
        if (mussleFlashTimer > 0)
        {
            mussleFlashTimer -= Time.deltaTime;
            if (mussleFlashTimer <= 0 && mussleFlash != null)
            {
                mussleFlash.transform.localScale = Vector3.zero;
            }
        }
    }

    private void HandleAiming()
    {
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
    }

    private void HandleBulletState()
    {
        haveBullets = bullets > 0;
    }

    private void HandlePickupState()
    {
        bool holding = pickupScript.HoldingObject;
        canShoot = !holding;
        gunRenderer.enabled = !holding;
    }
}