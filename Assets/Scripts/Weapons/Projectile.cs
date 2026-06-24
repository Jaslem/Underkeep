// Projectile.cs
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector2 direction;
    private Rigidbody2D rb;

    [Header("Settings")]
    public float lifetime = 5f;

    [Tooltip("Adjust this if the bullet sprite isn't pointing the right way. " +
             "If the sprite points upper-right at 45 degrees, set this to -45")]
    public float spriteAngleOffset = -45f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float damage, float speed, Vector2 direction)
    {
        this.damage    = damage;
        this.speed     = speed;
        this.direction = direction.normalized;

        if (rb != null)
            rb.linearVelocity = this.direction * this.speed;

        // Rotate the sprite to face the travel direction
        float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffset);

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyStats enemy = other.GetComponent<EnemyStats>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}