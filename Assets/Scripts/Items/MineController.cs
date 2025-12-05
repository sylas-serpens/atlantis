using System.Collections;
using UnityEngine;

public class MineController : MonoBehaviour
{
    public GameObject explosion;
    public int damage = 1;

    [Header("Audio")]
    public AudioClip explosionClip;        
    public float explosionVolume = 1f;
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Damage player
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamageFromEnemy(damage);
        }

        // Spawn explosion visual
        if (explosion != null)
        {
            Instantiate(explosion, transform.position, transform.rotation);
        }

        // Play explosion sound on a temporary audio object
        if (explosionClip != null)
        {
            GameObject audioGO = new GameObject("MineExplosionAudio");
            audioGO.transform.position = transform.position;

            AudioSource src = audioGO.AddComponent<AudioSource>();
            src.clip = explosionClip;
            src.volume = explosionVolume;
            src.spatialBlend = 0f;
            src.playOnAwake = false;
            src.loop = false;

            src.pitch = Random.Range(pitchRange.x, pitchRange.y);

            src.Play();
            Destroy(audioGO, explosionClip.length / Mathf.Max(src.pitch, 0.01f));
        }

        // Destroy the mine itself
        Destroy(gameObject);
    }
}
