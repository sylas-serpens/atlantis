using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GoldPickup : MonoBehaviour
{
    public int amount = 1;

    [Header("Audio")]
    public AudioClip pickupClip;            
    public float volume = 1f;
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Award gold
        PlayerGoldUI gold = other.GetComponent<PlayerGoldUI>();
        if (gold != null)
        {
            gold.AddGold(amount);
        }

        // Play the pickup sound BEFORE destroying the coin
        PlayPickupSound();

        // Destroy the coin
        Destroy(gameObject);
    }

    private void PlayPickupSound()
    {
        if (pickupClip == null)
            return;

        GameObject audioGO = new GameObject("GoldPickupAudio");
        audioGO.transform.position = transform.position;

        AudioSource src = audioGO.AddComponent<AudioSource>();
        src.clip = pickupClip;
        src.volume = volume;
        src.spatialBlend = 0f; 
        src.playOnAwake = false;
        src.loop = false;

        src.pitch = Random.Range(pitchRange.x, pitchRange.y);

        src.Play();

        Destroy(audioGO, pickupClip.length / Mathf.Max(src.pitch, 0.01f));
    }
}
