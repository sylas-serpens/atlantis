using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Slider slider;

    [Header("Damage Flash")]
    public Image fillImage;            // assign the Fill image here
    public Color flashColor = Color.red;
    public float flashDuration = 0.2f;

    private bool isFlashing = false;
    private Color normalColor;

    [System.Obsolete]
    void Start()
    {
        // Try to get slider on the same object if not set
        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider != null)
        {
            // Slider always works with 0–1, we feed it a percent
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.value = 1f;
        }
        else
        {
            Debug.LogWarning("HealthBarUI: No Slider found on this object.", this);
        }

        if (fillImage != null)
            normalColor = fillImage.color;
        else
            normalColor = Color.white;

        // Try to find the player immediately
        TryFindPlayer();
    }

    [System.Obsolete]
    void Update()
    {
        // If we lost the reference between scenes, try to find it again
        if (playerHealth == null)
            TryFindPlayer();

        if (playerHealth == null || slider == null)
            return;

        float pct = 0f;
        if (playerHealth.maxHealth > 0)
            pct = (float)playerHealth.currentHealth / playerHealth.maxHealth;

        slider.value = Mathf.Clamp01(pct);
    }

    [System.Obsolete]
    void TryFindPlayer()
    {
        // First try by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        // Fallback: search any PlayerHealth in the scene
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogWarning("HealthBarUI: Could not find PlayerHealth in this scene.", this);
        }
    }

    // Call this from PlayerHealth when damage is taken
    public void FlashDamage()
    {
        if (!isFlashing && fillImage != null)
            StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        isFlashing = true;

        if (fillImage != null)
            fillImage.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        if (fillImage != null)
            fillImage.color = normalColor;

        isFlashing = false;
    }
}
