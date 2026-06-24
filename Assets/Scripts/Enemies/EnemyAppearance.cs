// EnemyAppearance.cs
using UnityEngine;

public class EnemyAppearance : MonoBehaviour
{
    [Header("Animator Override Controller")]
    [Tooltip("Drag the override controller for this specific enemy type here")]
    public AnimatorOverrideController animatorOverride;

    private void Start()
    {
        if (animatorOverride == null)
        {
            Debug.LogWarning($"{gameObject.name}: No animator override assigned!");
            return;
        }

        UnityEngine.Animator animator = GetComponent<UnityEngine.Animator>();
        if (animator != null)
            animator.runtimeAnimatorController = animatorOverride;
    }
}