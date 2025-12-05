using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class MermaidRanged : EnemyHealthBase
{
    public Transform player;

    [Header("Movement")]
    public float swimSpeed = 3f;

    [Header("Aggro & Patrol")]
    public float aggroRange = 8f;
    public float patrolDistance = 3f;

    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 6f;
    public float fireCooldown = 1.5f;
    public Vector2 fireOffset = new Vector2(0.5f, 0f);

    [Header("Audio")]
    public AudioClip poisonClip;        // 46_Poison_01
    public float poisonVolume = 1f;
    public Vector2 poisonPitchRange = new Vector2(0.95f, 1.05f);
    private AudioSource audioSource;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 startPosition;
    private int patrolDirection = 1;
    private float nextFireTime = 0f;

    private enum State { Patrolling, Attacking, Dead }
    private State state = State.Patrolling;

    protected override void Start()
    {
        base.Start();

        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        startPosition = transform.position;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (isDead || state == State.Dead) return;
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        state = (distToPlayer <= aggroRange) ? State.Attacking : State.Patrolling;

        if (state == State.Patrolling)
        {
            HandlePatrolMovement();
        }
        else if (state == State.Attacking)
        {
            HandleAttack();
        }

        anim.SetFloat("Speed", rb.linearVelocity.magnitude);
        UpdateFacing();
    }

    // ---------------------------
    // PATROL
    // ---------------------------
    void HandlePatrolMovement()
    {
        float leftBound  = startPosition.x - patrolDistance;
        float rightBound = startPosition.x + patrolDistance;

        if (patrolDirection > 0 && transform.position.x >= rightBound)
            patrolDirection = -1;
        else if (patrolDirection < 0 && transform.position.x <= leftBound)
            patrolDirection = 1;

        rb.linearVelocity = new Vector2(patrolDirection * swimSpeed, 0f);
    }

    // ---------------------------
    // ATTACK
    // ---------------------------
    void HandleAttack()
    {
        rb.linearVelocity = Vector2.zero;

        if (Time.time >= nextFireTime)
        {
            FireProjectile();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void FireProjectile()
    {
        if (player == null || projectilePrefab == null) return;

        // Trigger animation
        anim.SetTrigger("Attack");

        // PLAY POISON SFX
        if (audioSource != null && poisonClip != null)
        {
            audioSource.pitch = Random.Range(poisonPitchRange.x, poisonPitchRange.y);
            audioSource.PlayOneShot(poisonClip, poisonVolume);
        }

        // spawn position based on facing + offset
        int facingDir = sr.flipX ? -1 : 1;
        Vector3 spawnPos = transform.position +
                           new Vector3(fireOffset.x * facingDir, fireOffset.y, 0f);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // ignore collision with mermaid + her children
        Collider2D projCol = proj.GetComponent<Collider2D>();
        if (projCol != null)
        {
            Collider2D[] myCols = GetComponentsInChildren<Collider2D>();
            foreach (var c in myCols)
                Physics2D.IgnoreCollision(projCol, c);
        }

        Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            Vector2 dir = (player.position - spawnPos).normalized;
            projRb.linearVelocity = dir * projectileSpeed;
        }
    }

    // ---------------------------
    // FACING
    // ---------------------------
    void UpdateFacing()
    {
        if (state == State.Attacking && player != null)
        {
            sr.flipX = (player.position.x < transform.position.x);
        }
        else
        {
            Vector2 vel = rb.linearVelocity;
            if (vel.sqrMagnitude > 0.001f)
            {
                if (vel.x < -0.01f) sr.flipX = true;
                else if (vel.x > 0.01f) sr.flipX = false;
            }
        }

        transform.rotation = Quaternion.identity;
    }

    // ---------------------------
    // HEALTH / DEATH
    // ---------------------------
    public override void TakeDamage(int amount)
    {
        if (isDead) return;

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
        if (isDead) return;
        isDead = true;
        state = State.Dead;

        rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        anim.SetTrigger("Death");

        Destroy(gameObject, 2f);
    }
}
