using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile2D : MonoBehaviour
{
    public float speed = 14f;
    public float lifetime = 2f;
    public int damage = 1;

    [HideInInspector] public Vector2 dir;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void Start()
    {
        // Normalize direction for safety
        if (dir.sqrMagnitude > 0.0001f)
            dir = dir.normalized;
        else
            dir = Vector2.right;

        // Rotate to face direction of travel
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Move in that direction
        rb.linearVelocity = dir * speed;

        // Destroy after some time
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Hit any enemy by tag
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealthBase enemy = collision.GetComponent<EnemyHealthBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        // Optionally, handle walls/terrain here
        // if (collision.CompareTag("Wall")) Destroy(gameObject);
    }
}
