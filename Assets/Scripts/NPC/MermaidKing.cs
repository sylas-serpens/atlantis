using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class MermaidKing : EnemyHealthBase
{
    public Transform player;

    [Header("Movement")]
    public float swimSpeed = 2.5f;
    public float chaseSpeed = 3.5f;
    public float retreatSpeed = 5f;

    [Header("Patrol")]
    public float patrolDistance = 4f;

    [Header("Aggro")]
    public float aggroRange = 8f;

    [Header("Melee Attack")]
    public int meleeDamage = 1;
    public float meleeDamageRadius = 2f;
    public float meleeWindup = 0.3f;
    public float meleePostAttackPause = 0.3f;
    public float retreatDuration = 1.5f;

    [Header("Special Attack")]
    public GameObject specialProjectilePrefab;
    public float specialProjectileSpeed = 10f;
    public float specialCooldown = 10f;
    public Vector2 specialOffset = new Vector2(0.8f, 0.2f);

    [Header("Audio")]
    public AudioClip meleeClip;   
    public float meleeVolume = 1f;
    public Vector2 meleePitchRange = new Vector2(0.95f, 1.05f);

    public AudioClip specialClip; 
    public float specialVolume = 1f;
    public Vector2 specialPitchRange = new Vector2(0.95f, 1.05f);

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private AudioSource audioSource;

    private Vector2 startPosition;
    private int patrolDirection = 1;
    private float specialTimer = 0f;

    private Coroutine currentRoutine;

    private enum State { Patrolling, Chasing, MeleeAttacking, Retreating, Dead }
    private State state = State.Patrolling;

    private bool isCastingSpecial = false;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }

    protected override void Start()
    {
        base.Start();

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

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

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            if (player == null) return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (isCastingSpecial)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetFloat("Speed", 0f);
            UpdateFacing();
            return;
        }

        bool canSwitch = (state != State.MeleeAttacking && state != State.Retreating);
        if (canSwitch)
        {
            if (distToPlayer <= aggroRange)
                state = State.Chasing;
            else
                state = State.Patrolling;
        }

        if (state == State.Patrolling)
        {
            HandlePatrol();
        }
        else if (state == State.Chasing)
        {
            HandleChase();
        }

        if (distToPlayer <= aggroRange && state != State.Dead)
        {
            specialTimer += Time.deltaTime;
            if (specialTimer >= specialCooldown &&
                state != State.MeleeAttacking &&
                state != State.Retreating)
            {
                StartSpecial();
            }
        }
        else
        {
            specialTimer = Mathf.Min(specialTimer, specialCooldown * 0.5f);
        }

        anim.SetFloat("Speed", rb.linearVelocity.magnitude);
        UpdateFacing();
    }

    // ---------------------------
    // MOVEMENT
    // ---------------------------

    void HandlePatrol()
    {
        float leftBound  = startPosition.x - patrolDistance;
        float rightBound = startPosition.x + patrolDistance;

        if (patrolDirection > 0 && transform.position.x >= rightBound)
            patrolDirection = -1;
        else if (patrolDirection < 0 && transform.position.x <= leftBound)
            patrolDirection = 1;

        rb.linearVelocity = new Vector2(patrolDirection * swimSpeed, 0f);
    }

    void HandleChase()
    {
        Vector2 toPlayer = (player.position - transform.position);
        rb.linearVelocity = toPlayer.normalized * chaseSpeed;
    }

    // ---------------------------
    // MELEE + RETREAT
    // ---------------------------

    void StartNewRoutine(IEnumerator routine)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(routine);
    }

    IEnumerator MeleeThenRetreat()
    {
        state = State.MeleeAttacking;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Melee");

        if (audioSource != null && meleeClip != null)
        {
            audioSource.pitch = Random.Range(meleePitchRange.x, meleePitchRange.y);
            audioSource.PlayOneShot(meleeClip, meleeVolume);
        }

        yield return new WaitForSeconds(meleeWindup);

        TryMeleeDamage();

        yield return new WaitForSeconds(meleePostAttackPause);

        state = State.Retreating;

        Vector2 awayDir = (transform.position - player.position).normalized;
        if (awayDir.sqrMagnitude < 0.001f)
            awayDir = Vector2.left;

        float t = 0f;
        while (t < retreatDuration && !isDead)
        {
            rb.linearVelocity = awayDir * retreatSpeed;
            t += Time.deltaTime;
            yield return null;
        }

        if (!isDead)
            state = State.Chasing;

        currentRoutine = null;
    }

    void TryMeleeDamage()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= meleeDamageRadius)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamageFromEnemy(meleeDamage);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleMeleeContact(collision.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleMeleeContact(other);
    }

    void HandleMeleeContact(Collider2D col)
    {
        if (isDead || state == State.Dead) return;
        if (isCastingSpecial) return;

        if (col.CompareTag("Player"))
        {
            if (state != State.MeleeAttacking && state != State.Retreating)
            {
                StartNewRoutine(MeleeThenRetreat());
            }
        }
    }

    // ---------------------------
    // SPECIAL
    // ---------------------------

    void StartSpecial()
    {
        isCastingSpecial = true;
        specialTimer = 0f;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Special");
    }

    // Called from Special animation event
    public void FireSpecialVolley()
    {
        if (player == null || specialProjectilePrefab == null)
        {
            isCastingSpecial = false;
            return;
        }

        if (audioSource != null && specialClip != null)
        {
            audioSource.pitch = Random.Range(specialPitchRange.x, specialPitchRange.y);
            audioSource.PlayOneShot(specialClip, specialVolume);
        }

        int facingDir = sr.flipX ? -1 : 1;
        Vector3 basePos = transform.position +
                          new Vector3(specialOffset.x * facingDir, specialOffset.y, 0f);

        float[] angleOffsets = new float[] { 0f, 10f, -10f, 20f, -20f };

        foreach (float angleOffset in angleOffsets)
        {
            GameObject proj = Instantiate(specialProjectilePrefab, basePos, Quaternion.identity);

            Collider2D projCol = proj.GetComponentInChildren<Collider2D>();
            if (projCol != null)
            {
                Collider2D[] myCols = GetComponentsInChildren<Collider2D>();
                foreach (var c in myCols)
                    Physics2D.IgnoreCollision(projCol, c);
            }

            Vector2 dir = (player.position - basePos).normalized;
            dir = Quaternion.Euler(0, 0, angleOffset) * dir;

            Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.gravityScale = 0f;
                projRb.linearVelocity = Vector2.zero;
                projRb.linearVelocity = dir * specialProjectileSpeed;
            }

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            proj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        isCastingSpecial = false;
    }

    // ---------------------------
    // FACING
    // ---------------------------

    void UpdateFacing()
    {
        if (player != null &&
            (state == State.Chasing || state == State.MeleeAttacking || state == State.Retreating || isCastingSpecial))
        {
            sr.flipX = (player.position.x < transform.position.x);
        }
        else
        {
            Vector2 vel = rb.linearVelocity;
            if (vel.sqrMagnitude > 0.0001f)
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
        {
            Die();
        }
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

        if (anim != null)
            anim.SetTrigger("Die");

        Destroy(gameObject, 3f);
    }
}
