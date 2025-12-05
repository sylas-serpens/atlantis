using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Drain Settings")]
    public bool drainHealthOverTime = true;
    public float drainRate = 1f;

    [Header("Audio")]
    public AudioClip hurtClip;
    public float hurtVolume = 1f;
    public Vector2 hurtPitchRange = new Vector2(0.95f, 1.05f);

    private float drainTimer = 0f;
    private AudioSource audioSource;

    // We store gold so it persists through death / reload
    private int goldAtSceneStart;

    void Start()
    {
        currentHealth = maxHealth;

        // Cache gold value at scene start
        PlayerGoldUI goldUI = FindFirstObjectByType<PlayerGoldUI>();
        if (goldUI != null)
            goldAtSceneStart = goldUI.currentGold;

        // AudioSource setup
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Update()
    {
        if (drainHealthOverTime)
        {
            drainTimer += Time.deltaTime;

            if (drainTimer >= drainRate)
            {
                drainTimer = 0f;
                TakeDamageOverTime(1);
            }
        }
    }

    // ========= PUBLIC DAMAGE APIS =========

    public void TakeDamageFromEnemy(int amount)
    {
        ApplyDamage(amount, showFlash: true);
    }

    public void TakeDamageOverTime(int amount)
    {
        ApplyDamage(amount, showFlash: false);
    }

    // ========= INTERNAL DAMAGE LOGIC =========

    private void ApplyDamage(int amount, bool showFlash)
    {
        currentHealth -= amount;

        if (showFlash)
        {
            // Flash UI
            HealthBarUI ui = FindFirstObjectByType<HealthBarUI>();
            if (ui != null)
                ui.FlashDamage();

            // Play damage sound
            PlayHurtSound();
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void PlayHurtSound()
    {
        if (hurtClip == null || audioSource == null) return;

        audioSource.pitch = Random.Range(hurtPitchRange.x, hurtPitchRange.y);
        audioSource.PlayOneShot(hurtClip, hurtVolume);
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    private void Die()
    {
        Debug.Log("Player Died.");

        // Restore player's gold to what they had when entering the scene
        PlayerGoldUI goldUI = FindFirstObjectByType<PlayerGoldUI>();
        if (goldUI != null)
            goldUI.currentGold = goldAtSceneStart;

        // Reload the scene
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }
}
