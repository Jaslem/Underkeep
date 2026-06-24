// EnemyMeleeAttack.cs
using System.Collections;
using UnityEngine;

public class EnemyMeleeAttack : MonoBehaviour
{
    private EnemyWeaponController weaponController;
    private bool canAttack = true;

    private void Awake()
    {
        weaponController = GetComponent<EnemyWeaponController>();

        if (weaponController == null)
            Debug.LogError($"{gameObject.name}: EnemyMeleeAttack can't find EnemyWeaponController!", this);
    }

    public void TryAttack(Transform target)
    {
        if (!canAttack) return;
        if (weaponController == null) return;
        if (weaponController.EquippedWeapon == null) return;

        StartCoroutine(PerformAttack(target));
    }

    private IEnumerator PerformAttack(Transform target)
{
    canAttack = false;
    Debug.Log($"[EnemyMeleeAttack] Starting attack — calling TriggerAttackAnimation(true)");
    weaponController.TriggerAttackAnimation(true);
    Debug.Log($"[EnemyMeleeAttack] Called TriggerAttackAnimation(true)");

        // Trigger attack animation
        weaponController.TriggerAttackAnimation(true);

        WeaponDataSO weapon = weaponController.EquippedWeapon;
        Collider2D[] hits   = Physics2D.OverlapCircleAll(transform.position, weapon.meleeRange);
        float damage        = weaponController.GetFinalMeleeDamage();

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerStats playerStats = hit.GetComponent<PlayerStats>();
                if (playerStats != null)
                    playerStats.TakeDamage(damage);

                Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 knockDir = (hit.transform.position - transform.position).normalized;
                    playerRb.AddForce(knockDir * weapon.meleeKnockback, ForceMode2D.Impulse);
                }
            }
        }

        yield return new WaitForSeconds(weapon.meleeAttackDuration);

        // End attack animation
        weaponController.TriggerAttackAnimation(false);

        yield return new WaitForSeconds(weapon.meleeCooldown);

        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        EnemyWeaponController wc = GetComponent<EnemyWeaponController>();
        if (wc == null || wc.EquippedWeapon == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, wc.EquippedWeapon.meleeRange);
    }
}