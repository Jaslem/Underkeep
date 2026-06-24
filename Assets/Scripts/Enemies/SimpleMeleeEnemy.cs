// SimpleMeleeEnemy.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// Used by: Blue Slime, Green Slime, Red Slime, Zombie
/// Chases the player and deals a simple melee hit when close enough.
/// No weapon system — just direct damage.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyStats))]
public class SimpleMeleeEnemy : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 8f;
    public float attackRange    = 1f;

    [Header("Attack")]
    public float attackDamage   = 10f;
    public float attackCooldown = 1f;
    public float knockbackForce = 3f;

    [Header("References")]
    public EnemyAnimatorController enemyAnimator;

    private Rigidbody2D  rb;
    private EnemyStats   enemyStats;
    private Transform    playerTransform;
    private bool         canAttack = true;
    private Vector2      moveDirection;

    private void Awake()
    {
        rb          = GetComponent<Rigidbody2D>();
        enemyStats  = GetComponent<EnemyStats>();

        if (enemyAnimator == null)
            enemyAnimator = GetComponent<EnemyAnimatorController>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void Update()
    {
        if (!enemyStats.IsAlive) return;
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist > detectionRange)
        {
            moveDirection = Vector2.zero;
        }
        else if (dist <= attackRange)
        {
            moveDirection = Vector2.zero;
            if (canAttack)
                StartCoroutine(PerformAttack());
        }
        else
        {
            moveDirection = ((Vector2)(playerTransform.position - transform.position)).normalized;
        }

        if (enemyAnimator != null)
            enemyAnimator.UpdateMovement(moveDirection);
    }

    private void FixedUpdate()
    {
        if (!enemyStats.IsAlive)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveDirection * enemyStats.moveSpeed;
    }

    private IEnumerator PerformAttack()
    {
        canAttack = false;

        PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
        if (playerStats != null)
            playerStats.TakeDamage(attackDamage);

        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 knockDir = (playerTransform.position - transform.position).normalized;
            playerRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}