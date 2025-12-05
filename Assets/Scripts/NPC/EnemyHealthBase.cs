using UnityEngine;

public class EnemyHealthBase : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 20;

    protected int currentHealth;
    protected bool isDead = false;
    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        Destroy(gameObject);
    }
}
