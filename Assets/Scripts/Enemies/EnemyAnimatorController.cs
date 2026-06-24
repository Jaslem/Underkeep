// EnemyAnimatorController.cs
using UnityEngine;

public class EnemyAnimatorController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SpriteRenderer on the enemy body")]
    public SpriteRenderer spriteRenderer;

    private UnityEngine.Animator animator;

    private static readonly int HashIsMoving = UnityEngine.Animator.StringToHash("IsMoving");

    private void Awake()
    {
        animator = GetComponent<UnityEngine.Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpdateMovement(Vector2 moveDirection)
    {
        bool isMoving = moveDirection != Vector2.zero;

        if (animator != null)
            animator.SetBool(HashIsMoving, isMoving);

        // Flip sprite based on horizontal movement
        if (spriteRenderer != null && moveDirection.x != 0f)
            spriteRenderer.flipX = moveDirection.x < 0f;
    }
}