// PlayerMovement.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("Dodge Roll Settings")]
    public float dodgeRollDistance = 4f;
    public float dodgeRollDuration = 0.2f;
    public float dodgeRollCooldown = 0.8f;

    [Header("I-Frame Settings")]
    public float baseIFrameDuration = 0.15f;

    // Components
    private Rigidbody2D rb;
    private UnityEngine.Animator animator;

    // Movement
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;

    // Dodge roll state
    private bool    isRolling          = false;
    private bool    isRollInvulnerable = false;
    private float   rollTimer          = 0f;
    private float   rollCooldownTimer  = 0f;
    private float   iFrameTimer        = 0f;
    private Vector2 rollDirection;

    // Post-roll lock
    private float       facingLockTimer    = 0f;
    private const float FacingLockDuration = 0.9f;

    // Expose for other scripts
    public bool IsRolling        => isRolling;
    public bool IsDodgeAnimating => isRolling || facingLockTimer > 0f;

    // Animator parameter hashes
    private static readonly int HashMoveX     = UnityEngine.Animator.StringToHash("MoveX");
    private static readonly int HashMoveY     = UnityEngine.Animator.StringToHash("MoveY");
    private static readonly int HashIsMoving  = UnityEngine.Animator.StringToHash("IsMoving");
    private static readonly int HashIsRolling = UnityEngine.Animator.StringToHash("IsRolling");

    private void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        animator = GetComponent<UnityEngine.Animator>();

        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (!playerStats.IsAlive) return;

        if (!isRolling && facingLockTimer <= 0f)
        {
            ReadMoveInput();
            HandleDodgeRoll();
        }

        UpdateRollTimer();

        if (facingLockTimer > 0f)
            facingLockTimer -= Time.deltaTime;

        if (!isRolling && facingLockTimer <= 0f)
            HandleMouseFacing();

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (!playerStats.IsAlive)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isRolling) return;

        if (facingLockTimer > 0f)
        {
            rb.linearVelocity = Vector2.Lerp(
                rb.linearVelocity,
                Vector2.zero,
                8f * Time.fixedDeltaTime
            );
            return;
        }

        rb.linearVelocity = moveInput * playerStats.MoveSpeed;
    }

    // -------------------------------------------------------
    // Input
    // -------------------------------------------------------

    private void ReadMoveInput()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (moveInput != Vector2.zero)
            lastMoveDirection = moveInput;
    }

    // -------------------------------------------------------
    // Mouse Facing
    // -------------------------------------------------------

    private void HandleMouseFacing()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        bool mouseIsLeft = mouseWorldPos.x < transform.position.x;
        Vector3 scale    = transform.localScale;
        scale.x          = mouseIsLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    // -------------------------------------------------------
    // Animator
    // -------------------------------------------------------

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetBool(HashIsMoving,  moveInput != Vector2.zero);
        animator.SetBool(HashIsRolling, isRolling);

        // During roll or post-roll lock freeze the aim direction
        if (isRolling || facingLockTimer > 0f) return;

        // Drive blend tree from mouse direction
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 aimDir = ((Vector2)(mouseWorldPos - transform.position)).normalized;

        // Snap to 4 directions
        Vector2 snapped;
        if (Mathf.Abs(aimDir.x) > Mathf.Abs(aimDir.y))
            snapped = new Vector2(Mathf.Sign(aimDir.x), 0f);
        else
            snapped = new Vector2(0f, Mathf.Sign(aimDir.y));

        animator.SetFloat(HashMoveX, snapped.x);
        animator.SetFloat(HashMoveY, snapped.y);
    }

    // -------------------------------------------------------
    // Dodge Roll
    // -------------------------------------------------------

    private void HandleDodgeRoll()
    {
        if (rollCooldownTimer > 0f) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift))
            StartRoll();
    }

    private void StartRoll()
    {
        isRolling         = true;
        rollTimer         = dodgeRollDuration;
        rollCooldownTimer = dodgeRollCooldown;
        rollDirection     = moveInput != Vector2.zero ? moveInput : lastMoveDirection;

        float rollSpeed = (dodgeRollDistance / dodgeRollDuration) * playerStats.MobilityMultiplier;
        rb.linearVelocity = rollDirection * rollSpeed;

        float iFrameDuration = baseIFrameDuration * playerStats.DodgeIFrameMultiplier;
        iFrameTimer        = iFrameDuration;
        isRollInvulnerable = true;
        playerStats.SetInvulnerable(true);

        if (animator != null)
        {
            // Snap roll direction and freeze MoveX/MoveY to it
            Vector2 snapped;
            if (Mathf.Abs(rollDirection.x) > Mathf.Abs(rollDirection.y))
                snapped = new Vector2(Mathf.Sign(rollDirection.x), 0f);
            else
                snapped = new Vector2(0f, Mathf.Sign(rollDirection.y));

            animator.SetFloat(HashMoveX, snapped.x);
            animator.SetFloat(HashMoveY, snapped.y);
            animator.SetBool(HashIsRolling, true);

            // Force sprite to face roll direction
            Vector3 scale = transform.localScale;
            if (Mathf.Abs(rollDirection.x) > Mathf.Abs(rollDirection.y))
                scale.x = rollDirection.x < 0f
                    ? -Mathf.Abs(scale.x)
                    :  Mathf.Abs(scale.x);
            else
                scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void UpdateRollTimer()
    {
        if (isRollInvulnerable)
        {
            iFrameTimer -= Time.deltaTime;
            if (iFrameTimer <= 0f)
            {
                isRollInvulnerable = false;
                if (!AbilityGrantingInvulnerability())
                    playerStats.SetInvulnerable(false);
            }
        }

        if (rollCooldownTimer > 0f)
            rollCooldownTimer -= Time.deltaTime;

        if (!isRolling) return;

        rollTimer -= Time.deltaTime;
        if (rollTimer <= 0f)
        {
            isRolling       = false;
            facingLockTimer = FacingLockDuration;

            if (animator != null)
                animator.SetBool(HashIsRolling, false);
        }
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private bool AbilityGrantingInvulnerability()
    {
        PlayerAbilityController ac = GetComponent<PlayerAbilityController>();
        if (ac == null) return false;
        PlayerAbility active = ac.GetActiveAbility();
        return active != null && active.IsActive && active is Ability_DivineIntervention;
    }
}