// GiantSlimeBoss.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyStats))]
public class GiantSlimeBoss : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Projectile Attack")]
    public GameObject projectilePrefab;
    public float projectileSpeed    = 8f;
    public float projectileDamage   = 15f;
    public int   projectileCount    = 8;
    public float fireRate           = 2f;
    public float projectileLifetime = 4f;

    [Header("Phase 2")]
    [Range(0f, 1f)]
    public float phase2Threshold        = 0.5f;
    public int   phase2ExtraProjectiles = 4;
    public float phase2FireRate         = 1.2f;

    [Header("Animator")]
    public UnityEngine.Animator bossAnimator;

    private static readonly int HashIsMoving    = UnityEngine.Animator.StringToHash("IsMoving");
    private static readonly int HashIsAttacking = UnityEngine.Animator.StringToHash("IsAttacking");

    private Rigidbody2D rb;
    private EnemyStats  enemyStats;
    private Transform   playerTransform;
    private bool        isPhase2 = false;

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

        StartCoroutine(ShootLoop());
    }

    private void Update()
    {
        if (!enemyStats.IsAlive) return;

        if (!isPhase2 && enemyStats.CurrentHealth / enemyStats.maxHealth <= phase2Threshold)
            EnterPhase2();
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

        Vector2 dir = ((Vector2)(playerTransform.position - transform.position)).normalized;
        rb.linearVelocity = dir * moveSpeed;

        if (bossAnimator != null)
            bossAnimator.SetBool(HashIsMoving, rb.linearVelocity.magnitude > 0.1f);
    }

    // -------------------------------------------------------
    // Shooting
    // -------------------------------------------------------

    private IEnumerator ShootLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(isPhase2 ? phase2FireRate : fireRate);
            if (!enemyStats.IsAlive) yield break;
            yield return StartCoroutine(ShootVolley());
        }
    }

    private IEnumerator ShootVolley()
    {
        if (bossAnimator != null)
            bossAnimator.SetBool(HashIsAttacking, true);

        int count = isPhase2
            ? projectileCount + phase2ExtraProjectiles
            : projectileCount;

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            if (projectilePrefab != null)
            {
                Vector3 spawnPos = transform.position + (Vector3)(dir * 2.5f);
                GameObject go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                EnemyProjectile proj = go.GetComponent<EnemyProjectile>();
                if (proj != null)
                    proj.Initialize(projectileDamage, projectileSpeed, dir, gameObject);
            }
        }

        yield return new WaitForSeconds(0.3f);

        if (bossAnimator != null)
            bossAnimator.SetBool(HashIsAttacking, false);
    }

    // -------------------------------------------------------
    // Phase 2
    // -------------------------------------------------------

    private void EnterPhase2()
    {
        isPhase2   = true;
        moveSpeed *= 1.5f;
        Debug.Log("[GiantSlimeBoss] Entered Phase 2!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}