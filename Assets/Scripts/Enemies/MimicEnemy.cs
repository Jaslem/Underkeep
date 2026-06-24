// MimicEnemy.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// Looks like a chest until the player enters its trigger zone.
/// Then reveals itself and chases the player as a melee enemy.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyStats))]
public class MimicEnemy : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("The chest sprite shown before the mimic reveals itself")]
    public Sprite chestSprite;
    [Tooltip("The mimic sprite shown after revealing")]
    public Sprite mimicSprite;

    [Header("Reveal Settings")]
    [Tooltip("Radius within which the player triggers the reveal")]
    public float revealRadius  = 3f;
    [Tooltip("Delay before the mimic starts chasing after revealing")]
    public float revealDelay   = 0.5f;

    [Header("Attack")]
    public float attackRange    = 1f;
    public float attackDamage   = 15f;
    public float attackCooldown = 1f;
    public float knockbackForce = 4f;

    [Header("References")]
    public EnemyAnimatorController enemyAnimator;

    private Rigidbody2D  rb;
    private EnemyStats   enemyStats;
    private SpriteRenderer spriteRenderer;
    private Transform    playerTransform;

    private bool    isRevealed = false;
    private bool    canAttack  = true;
    private Vector2 moveDirection;

    private void Awake()
    {
        rb             = GetComponent<Rigidbody2D>();
        enemyStats     = GetComponent<EnemyStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (enemyAnimator == null)
            enemyAnimator = GetComponent<EnemyAnimatorController>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Start as chest
        if (spriteRenderer != null && chestSprite != null)
            spriteRenderer.sprite = chestSprite;
    }

    private void Update()
    {
        if (!enemyStats.IsAlive) return;
        if (playerTransform == null) return;

        if (!isRevealed)
        {
            // Check if player is close enough to trigger reveal
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist <= revealRadius)
                StartCoroutine(Reveal());

            return;
        }

        // After reveal — chase and attack like a simple melee enemy
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distToPlayer <= attackRange)
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
        if (!enemyStats.IsAlive || !isRevealed)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveDirection * enemyStats.moveSpeed;
    }

    private IEnumerator Reveal()
    {
        isRevealed = true;

        // Swap to mimic sprite
        if (spriteRenderer != null && mimicSprite != null)
            spriteRenderer.sprite = mimicSprite;

        Debug.Log($"[Mimic] Revealed!");

        // Brief pause before chasing
        yield return new WaitForSeconds(revealDelay);
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, revealRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}