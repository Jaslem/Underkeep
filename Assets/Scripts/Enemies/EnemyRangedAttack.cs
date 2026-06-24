// EnemyRangedAttack.cs
using System.Collections;
using UnityEngine;

public class EnemyRangedAttack : MonoBehaviour
{
    [Header("Fire Point")]
    public Transform firePoint;

    private EnemyWeaponController weaponController;
    private float nextFireTime = 0f;

    private void Awake()
    {
        weaponController = GetComponent<EnemyWeaponController>();
    }

    public void TryAttack(Vector3 targetPos)
    {
        if (weaponController.EquippedWeapon == null) return;
        if (Time.time < nextFireTime) return;

        Shoot(targetPos);
    }

    private void Shoot(Vector3 targetPos)
    {
        WeaponDataSO weapon = weaponController.EquippedWeapon;

        if (weapon.projectilePrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: No projectile prefab on {weapon.weaponName}!");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 shootDir = ((Vector2)(targetPos - spawnPos)).normalized;

        nextFireTime = Time.time + weapon.fireRate;

        GameObject projectileGO      = Instantiate(weapon.projectilePrefab, spawnPos, Quaternion.identity);
        EnemyProjectile projectile   = projectileGO.GetComponent<EnemyProjectile>();
        if (projectile != null)
            projectile.Initialize(
                weaponController.GetFinalRangedDamage(),
                weapon.projectileSpeed,
                shootDir,
                gameObject  // pass owner so projectile ignores this enemy
            );

        // Trigger shoot animation
        StartCoroutine(PlayShootAnimation(weapon.fireRate));
    }

    private IEnumerator PlayShootAnimation(float duration)
    {
        weaponController.TriggerAttackAnimation(true);
        yield return new WaitForSeconds(duration);
        weaponController.TriggerAttackAnimation(false);
    }
}