using UnityEngine;

public class MermaidKingProjectile : MonoBehaviour
{
    public int damage = 2;
    public float lifetime = 5f;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.gravityScale = 0f;

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    void HandleHit(GameObject other)
    {
        // Hit player
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamageFromEnemy(damage);

            Destroy(gameObject);
        }
        // Hit ground / environment (optional – adjust layer name)
        else if (other.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
