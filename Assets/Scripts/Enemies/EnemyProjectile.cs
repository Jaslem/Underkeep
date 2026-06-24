// EnemyProjectile.cs
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector2 direction;
    private Rigidbody2D rb;
    private GameObject owner; // the enemy that fired this

    [Header("Settings")]
    public float lifetime = 5f;

    [Tooltip("Adjust this if the bullet sprite isn't pointing the right way.")]
    public float spriteAngleOffset = -45f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float damage, float speed, Vector2 direction, GameObject owner = null)
    {
        this.damage    = damage;
        this.speed     = speed;
        this.direction = direction.normalized;
        this.owner     = owner;

        if (rb != null)
            rb.linearVelocity = this.direction * this.speed;

        // Rotate sprite to face travel direction
        float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffset);

        // Ignore collision with the owner
        if (owner != null)
        {
            Collider2D ownerCol  = owner.GetComponent<Collider2D>();
            Collider2D projectileCol = GetComponent<Collider2D>();
            if (ownerCol != null && projectileCol != null)
                Physics2D.IgnoreCollision(projectileCol, ownerCol);
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
{
    // Ignore everything tagged Enemy — projectiles only hit player and walls
    if (other.CompareTag("Enemy")) return;

    // Ignore the owner specifically as a backup
    if (owner != null && other.gameObject == owner) return;

    if (other.CompareTag("Player"))
    {
        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            if (playerStats.IsInvulnerable) return;
            playerStats.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
    else if (other.CompareTag("Wall"))
    {
        Destroy(gameObject);
    }
}
}