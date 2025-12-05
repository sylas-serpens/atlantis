using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Crab : EnemyHealthBase
{
    [Header("Movement")]
    public float walkSpeed = 2f;
    public int direction = -1; // -1 = left, 1 = right

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundRayLength = 0.3f;

    [Header("Damage")]
    public int touchDamage = 1;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip biteClip;
    public float biteVolume = 1f;
    public Vector2 bitePitchRange = new Vector2(0.95f, 1.05f);

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D col;

    private bool isDying = false;
    private bool isBiting = false;

    protected override void Start()
    {
        base.Start();

        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
        col  = GetComponent<Collider2D>();

        rb.linearVelocity = Vector2.zero;

        // Auto-assign audio source if none provided
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        if (groundLayer == 0)
            groundLayer = ~(1 << gameObject.layer);
    }

    void Update()
    {
        if (isDead || isDying) return;
        if (isBiting) return;

        Patrol();
        UpdateVisuals();
    }

    // ---------------------------
    // PATROL MOVEMENT
    // ---------------------------
    void Patrol()
    {
        Bounds b = col.bounds;
        float frontOffsetX = b.extents.x * 0.9f;
        float frontX = b.center.x + direction * frontOffsetX;

        Vector2 origin = new Vector2(frontX, b.min.y - 0.05f);

        RaycastHit2D groundHit = Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundLayer);

        if (!groundHit)
        {
            FlipDirection();

            frontX = b.center.x + direction * frontOffsetX;
            origin = new Vector2(frontX, b.min.y - 0.05f);
            groundHit = Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundLayer);

            if (!groundHit)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }
        }

        rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
    }

    void FlipDirection()
    {
        direction *= -1;
    }

    // ---------------------------
    // COLLISION → ATTACK
    // ---------------------------
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || isDying) return;

        if (collision.collider.CompareTag("Player"))
        {
            StartCoroutine(BiteAttack(collision.collider));
            return;
        }

        if (((1 << collision.collider.gameObject.layer) & groundLayer) != 0)
        {
            if (collision.contacts.Length > 0)
            {
                Vector2 normal = collision.contacts[0].normal;
                if (Mathf.Abs(normal.x) > 0.5f)
                    FlipDirection();
            }
        }
    }

    // ---------------------------
    // BITE ATTACK
    // ---------------------------
    IEnumerator BiteAttack(Collider2D player)
    {
        isBiting = true;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        anim.SetTrigger("Bite");

        // --- Play bite SFX ---
        if (audioSource != null && biteClip != null)
        {
            audioSource.pitch = Random.Range(bitePitchRange.x, bitePitchRange.y);
            audioSource.PlayOneShot(biteClip, biteVolume);
        }

        yield return new WaitForSeconds(0.25f);

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamageFromEnemy(touchDamage);

        yield return new WaitForSeconds(0.2f);

        isBiting = false;
    }

    // ---------------------------
    // VISUALS
    // ---------------------------
    void UpdateVisuals()
    {
        if (direction < 0)
            sr.flipX = true;
        else if (direction > 0)
            sr.flipX = false;

        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        transform.rotation = Quaternion.identity;
    }

    // ---------------------------
    // HEALTH / DEATH
    // ---------------------------
    public override void TakeDamage(int amount)
    {
        if (isDead || isDying) return;

        currentHealth -= amount;
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator HitFlash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    protected override void Die()
    {
        if (isDead || isDying) return;
        isDead = true;
        isDying = true;

        rb.linearVelocity = Vector2.zero;

        Collider2D c = GetComponent<Collider2D>();
        if (c != null) c.enabled = false;

        anim.SetTrigger("Die");

        Destroy(gameObject, 2f);
    }
}
