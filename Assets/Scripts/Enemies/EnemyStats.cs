// EnemyStats.cs
using UnityEngine;
using UnityEngine.Events;

public class EnemyStats : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth    = 50f;
    public float moveSpeed    = 3f;
    public float meleeDamage  = 10f;
    public float rangedDamage = 8f;

    public float CurrentHealth { get; private set; }
    public bool  IsAlive       => CurrentHealth > 0f;

    public UnityEvent<float, float> OnHealthChanged = new UnityEvent<float, float>();
    public UnityEvent               OnDied          = new UnityEvent();

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0f, maxHealth);
        OnHealthChanged.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0f, maxHealth);
        OnHealthChanged.Invoke(CurrentHealth, maxHealth);
    }

    private void Die()
    {
        OnDied.Invoke();
        Destroy(gameObject);
    }
}