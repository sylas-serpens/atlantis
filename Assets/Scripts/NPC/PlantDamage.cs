using UnityEngine;

public class PlantDamage : MonoBehaviour
{
    public int damage = 1;                    
    public bool damageOnce = false;           
    public bool destroyOnHit = false;         

    private bool hasDamaged = false;

    // --- For solid collisions ---
    void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamage(collision.transform);
    }

    // --- For trigger-based hazards ---
    void OnTriggerEnter2D(Collider2D collision)
    {
        TryDamage(collision.transform);
    }

    private void TryDamage(Transform other)
    {
        if (hasDamaged && damageOnce)
            return;

        // See if the collided object has PlayerHealth
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamageFromEnemy(damage);
            hasDamaged = true;

            if (destroyOnHit)
                Destroy(gameObject);
        }
    }
}
