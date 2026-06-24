// EnemyAI.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyWeaponController))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("How close the player must be before the enemy starts chasing")]
    public float detectionRange = 8f;

    [Tooltip("How close the enemy gets before stopping and attacking (melee)")]
    public float meleeStopRange = 1.2f;

    [Tooltip("How far the enemy stays from the player when using a ranged weapon")]
    public float rangedStopRange = 5f;

    [Header("References")]
    [Tooltip("Leave blank — found automatically at runtime")]
    public Transform playerTransform;

    // Components
    private Rigidbody2D       rb;
    private EnemyStats        enemyStats;
    private EnemyWeaponController weaponController;
    private EnemyMeleeAttack  meleeAttack;
    private EnemyRangedAttack rangedAttack;
    private EnemyAnimatorController enemyAnimator;

    // State
    private enum AIState { Idle, Chase, Attack }
    private AIState state = AIState.Idle;
    private Vector2 moveDirection;

    private void Awake()
    {
        rb               = GetComponent<Rigidbody2D>();
        enemyStats       = GetComponent<EnemyStats>();
        weaponController = GetComponent<EnemyWeaponController>();
        meleeAttack      = GetComponent<EnemyMeleeAttack>();
        rangedAttack     = GetComponent<EnemyRangedAttack>();
        enemyAnimator    = GetComponent<EnemyAnimatorController>();
    }

    private void Start()
    {
        // Find the player automatically by tag
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            playerTransform = playerGO.transform;
        else
            Debug.LogWarning($"{gameObject.name}: Could not find Player tag in scene!");
    }

    private void Update()
    {
        if (!enemyStats.IsAlive) return;
        if (playerTransform == null) return;

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        UpdateState(distToPlayer);
        HandleState(distToPlayer);

        // Always aim weapon at player
        weaponController.AimAtTarget(playerTransform.position);
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

    // -------------------------------------------------------
    // State Machine
    // -------------------------------------------------------

    private void UpdateState(float distToPlayer)
    {
        float stopRange = weaponController.IsMelee ? meleeStopRange : rangedStopRange;

        if (distToPlayer > detectionRange)
            state = AIState.Idle;
        else if (distToPlayer <= stopRange)
            state = AIState.Attack;
        else
            state = AIState.Chase;
    }

    private void HandleState(float distToPlayer)
    {
        switch (state)
        {
            case AIState.Idle:
                moveDirection = Vector2.zero;
                break;

            case AIState.Chase:
                // Move toward the player
                moveDirection = ((Vector2)(playerTransform.position - transform.position)).normalized;
                break;

            case AIState.Attack:
                moveDirection = Vector2.zero;

                // Delegate to the correct attack component
                if (weaponController.IsMelee && meleeAttack != null)
                    meleeAttack.TryAttack(playerTransform);
                else if (weaponController.IsRanged && rangedAttack != null)
                    rangedAttack.TryAttack(playerTransform.position);
                break;
        }

        // Update animator with move direction
        if (enemyAnimator != null)
            enemyAnimator.UpdateMovement(moveDirection);
    }

    // -------------------------------------------------------
    // Gizmos
    // -------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeStopRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangedStopRange);
    }


}