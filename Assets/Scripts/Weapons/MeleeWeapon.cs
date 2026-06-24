// MeleeWeapon.cs
using System.Collections;
using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    [Header("Melee Visual")]
    public Transform hitPoint;

    private Coroutine attackCoroutine;

    public override void HandleAttackInput()
    {
        AimAtMouse();

        if (Input.GetMouseButtonDown(0) && canAttack)
            attackCoroutine = StartCoroutine(PerformMeleeAttack());
    }

    private IEnumerator PerformMeleeAttack()
    {
        canAttack = false;

        // Trigger attack animation
        TriggerAttackAnimation(true);

        Vector3 attackOrigin = hitPoint != null ? hitPoint.position : transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin, weaponData.meleeRange);

        float finalDamage = playerStats.GetFinalMeleeDamage(weaponData.meleeDamageMultiplier);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyStats enemy = hit.GetComponent<EnemyStats>();
                if (enemy != null)
                    enemy.TakeDamage(finalDamage);

                Rigidbody2D enemyRb = hit.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDir * weaponData.meleeKnockback, ForceMode2D.Impulse);
                }
            }
        }

        float effectiveDuration = weaponData.meleeAttackDuration * playerStats.AttackSpeedMultiplier;
        float effectiveCooldown = weaponData.meleeCooldown       * playerStats.AttackSpeedMultiplier;

        yield return new WaitForSeconds(effectiveDuration);

        // End attack animation
        TriggerAttackAnimation(false);

        yield return new WaitForSeconds(effectiveCooldown);

        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (weaponData == null) return;
        Gizmos.color = Color.red;
        Vector3 origin = hitPoint != null ? hitPoint.position : transform.position;
        Gizmos.DrawWireSphere(origin, weaponData.meleeRange);
    }
}