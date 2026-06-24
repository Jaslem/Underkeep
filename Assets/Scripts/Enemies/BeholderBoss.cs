// BeholderBoss.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyStats))]
public class BeholderBoss : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed      = 3f;
    public float hoverAmplitude = 0.5f;
    public float hoverSpeed     = 2f;

    [Header("Chomp Attack")]
    public float chompRange     = 2f;
    public float chompDamage    = 25f;
    public float chompCooldown  = 1.5f;
    public float chompKnockback = 5f;

    [Header("Projectile Attack")]
    public GameObject projectilePrefab;
    public float projectileSpeed    = 10f;
    public float projectileDamage   = 12f;
    public float projectileFireRate = 2f;
    public int   projectileBurstCount = 3;

    [Header("Phase 2")]
    [Range(0f, 1f)]
    public float phase2Threshold = 0.5f;

    [Header("Animator")]
    public UnityEngine.Animator bossAnimator;

    private static readonly int HashIsMoving   = UnityEngine.Animator.StringToHash("IsMoving");
    private static readonly int HashIsChomp    = UnityEngine.Animator.StringToHash("IsChomp");
    private static readonly int HashIsShooting = UnityEngine.Animator.StringToHash("IsShooting");

    private Rigidbody2D rb;
    private EnemyStats  enemyStats;
    private Transform   playerTransform;

    private bool  isPhase2   = false;
    private bool  canChomp   = true;
    private bool  canShoot   = true;
    private float hoverTimer = 0f;

    private void Awake()
    {
        rb         = GetComponent<Rigidbody2D>();
        enemyStats = GetComponent<EnemyStats>();

        if (bossAnimator == null)
            bossAnimator = GetComponent<UnityEngine.Animator>();
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

        if (!isPhase2 && enemyStats.CurrentHealth / enemyStats.maxHealth <= phase2Threshold)
            EnterPhase2();

        hoverTimer += Time.deltaTime * hoverSpeed;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distToPlayer <= chompRange)
        {
            if (canChomp)
                StartCoroutine(PerformChomp());
        }
        else
        {
            if (canShoot)
                StartCoroutine(PerformProjectileAttack());
        }
    }

    private void FixedUpdate()
    {
        if (!enemyStats.IsAlive)
        {
            rb.linearVelocity = Vector2.zero;
            if (bossAnimator != null)
                bossAnimator.SetBool(HashIsMoving, false);
            return;
        }

        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist <= chompRange)
        {
            rb.linearVelocity = Vector2.zero;
            if (bossAnimator != null)
                bossAnimator.SetBool(HashIsMoving, false);
        }
        else
        {
            Vector2 dir      = ((Vector2)(playerTransform.position - transform.position)).normalized;
            Vector2 hoverVec = new Vector2(0f, Mathf.Sin(hoverTimer) * hoverAmplitude * 0.1f);
            rb.linearVelocity = (dir * moveSpeed) + hoverVec;

            if (bossAnimator != null)
                bossAnimator.SetBool(HashIsMoving, true);
        }
    }

    // -------------------------------------------------------
    // Chomp
    // -------------------------------------------------------

    private IEnumerator PerformChomp()
    {
        canChomp = false;

        if (bossAnimator != null)
            bossAnimator.SetBool(HashIsChomp, true);

        yield return new WaitForSeconds(0.2f);

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= chompRange)
        {
            PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
            if (playerStats != null)
                playerStats.TakeDamage(chompDamage);

            Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockDir = (playerTransform.position - transform.position).normalized;
                playerRb.AddForce(knockDir * chompKnockback, ForceMode2D.Impulse);
            }
        }

        yield return new WaitForSeconds(0.3f);

        if (bossAnimator != null)
            bossAnimator.SetBool(HashIsChomp, false);

        yield return new WaitForSeconds(isPhase2 ? chompCooldown * 0.6f : chompCooldown);

        canChomp = true;
    }

    // -------------------------------------------------------
    // Projectile
    // -------------------------------------------------------

    private IEnumerator PerformProjectileAttack()
    {
        canShoot = false;

        if (bossAnimator != null)
            bossAnimator.SetBool(HashIsShooting, true);

        int burstCount = isPhase2 ? projectileBurstCount + 2 : projectileBurstCount;

        for (int i = 0; i < burstCount; i++)
        {
            if (playerTransform == null) break;

            Vector2 dir         = ((Vector2)(playerTransform.position - transform.position)).normalized;
            float spreadAngle   = (i - burstCount / 2f) * 15f;
            float rad           = spreadAngle * Mathf.Deg2Rad;
            Vector2 spread      = new Vector2(
                dir.x * Mathf.Cos(rad) - dir.y * Mathf.Sin(rad),
                dir.x * Mathf.Sin(rad) + dir.y * Mathf.Cos(rad)
            );

            if (projectilePrefab != null)
            {
                Vector3 spawnPos = transform.position + (Vector3)(spread * 2.5f);
                GameObject go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                EnemyProjectile proj = go.GetComponent<EnemyProjectile>();
                if (proj != null)
                    proj.Initialize(projectileDamage, projectileSpeed, spread, gameObject);
            }

            yield return new WaitForSeconds(0.15f);
        }

        if (bossAnimator != null)
            bossAnimator.SetBool(HashIsShooting, false);

        yield return new WaitForSeconds(isPhase2 ? projectileFireRate * 0.6f : projectileFireRate);

        canShoot = true;
    }

    // -------------------------------------------------------
    // Phase 2
    // -------------------------------------------------------

    private void EnterPhase2()
    {
        isPhase2   = true;
        moveSpeed *= 1.4f;
        Debug.Log("[BeholderBoss] Entered Phase 2!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chompRange);
    }
}