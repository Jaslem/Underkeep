// RangedWeapon.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RangedWeapon : WeaponBase
{
    [Header("Ranged Components")]
    [Tooltip("The point from which projectiles spawn. Falls back to this transform if not assigned.")]
    public Transform firePoint;

    public int  CurrentAmmo { get; private set; }
    public bool IsReloading { get; private set; }

    public UnityEvent<int, int> OnAmmoChanged = new UnityEvent<int, int>();
    public UnityEvent           OnReloadStart = new UnityEvent();
    public UnityEvent           OnReloadEnd   = new UnityEvent();

    private float     nextFireTime;
    private Coroutine reloadCoroutine;

    public override void Initialize(PlayerStats stats)
    {
        base.Initialize(stats);
        CurrentAmmo = weaponData.magazineSize;
    }

    public override void OnEquip()
    {
        base.OnEquip();
        OnAmmoChanged.Invoke(CurrentAmmo, weaponData.magazineSize);
    }

    public override void HandleAttackInput()
    {
        AimAtMouse();

        bool shootPressed = weaponData.isAutomatic
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (shootPressed && !IsReloading)
            TryShoot();

        if (Input.GetKeyDown(KeyCode.R) && !IsReloading && CurrentAmmo < weaponData.magazineSize)
            reloadCoroutine = StartCoroutine(Reload());
    }

    private void TryShoot()
    {
        if (Time.time < nextFireTime) return;

        if (CurrentAmmo <= 0)
        {
            if (!IsReloading)
                reloadCoroutine = StartCoroutine(Reload());
            return;
        }

        Shoot();
    }

    private void Shoot()
    {
        if (weaponData.projectilePrefab == null)
        {
            Debug.LogWarning($"{weaponData.weaponName}: No projectile prefab assigned!");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        float effectiveFireRate = weaponData.fireRate * playerStats.AttackSpeedMultiplier;
        nextFireTime = Time.time + effectiveFireRate;

        CurrentAmmo--;
        OnAmmoChanged.Invoke(CurrentAmmo, weaponData.magazineSize);

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 shootDir = ((Vector2)(mouseWorldPos - spawnPos)).normalized;

        GameObject projectileGO = Instantiate(
            weaponData.projectilePrefab,
            spawnPos,
            Quaternion.identity
        );

        float finalDamage = playerStats.GetFinalRangedDamage(weaponData.rangedDamageMultiplier);

        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
            projectile.Initialize(finalDamage, weaponData.projectileSpeed, shootDir);

        // Trigger shoot animation
        StartCoroutine(PlayShootAnimation(effectiveFireRate));
    }

    private IEnumerator PlayShootAnimation(float duration)
    {
        TriggerAttackAnimation(true);
        yield return new WaitForSeconds(duration);
        TriggerAttackAnimation(false);
    }

    private IEnumerator Reload()
    {
        IsReloading = true;
        OnReloadStart.Invoke();
        Debug.Log($"{weaponData.weaponName}: Reloading...");

        yield return new WaitForSeconds(weaponData.reloadTime);

        CurrentAmmo  = weaponData.magazineSize;
        IsReloading  = false;
        OnReloadEnd.Invoke();
        OnAmmoChanged.Invoke(CurrentAmmo, weaponData.magazineSize);
    }
}