using UnityEngine;

public class MermaidProjectile : MonoBehaviour
{
    public int damage = 1;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryHitPlayer(collision.transform);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        TryHitPlayer(collision.transform);
        Destroy(gameObject);
    }

    void TryHitPlayer(Transform other)
    {
        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamageFromEnemy(damage);
        }
    }
}
