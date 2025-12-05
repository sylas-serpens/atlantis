using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class Squid : EnemyHealthBase
{
    public Transform player;

    [Header("Movement")]
    public float swimSpeed = 3f;
    public float retreatSpeed = 4.5f;
    public float biteRange = 1.5f;
    public float retreatDuration = 2f;

    [Header("Aggro")]
    public float aggroRange = 8f;

    [Header("Patrol")]
    public float patrolDistance = 3f;

    [Header("Damage")]
    public int biteDamage = 1;
    public float biteDamageRadius = 2.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip biteClip;
    public float biteVolume = 1f;
    public Vector2 bitePitchRange = new Vector2(0.95f, 1.05f);

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Coroutine currentRoutine;

    private Vector2 startPosition;
    private int patrolDirection = 1;

    private enum State { Patrolling, Chasing, Biting, Retreating, Dead }
    private State state = State.Patrolling;

    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr  = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        startPosition = transform.position;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    void Update()
    {
        if (isDead || state == State.Dead) return;
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        bool canSwitchForAggro = (state != State.Biting && state != State.Retreating);

        if (canSwitchForAggro)
        {
            if (distToPlayer <= aggroRange)
            {
                if (state != State.Chasing)
                    state = State.Chasing;
            }
            else
            {
                if (state != State.Patrolling)
                    state = State.Patrolling;
            }
        }

        if (state == State.Chasing)
        {
            Vector2 toPlayer = (player.position - transform.position);
            rb.linearVelocity = toPlayer.normalized * swimSpeed;
        }
        else if (state == State.Patrolling)
        {
            float leftBound = startPosition.x - patrolDistance;
            float rightBound = startPosition.x + patrolDistance;

            if (patrolDirection > 0 && transform.position.x >= rightBound)
                patrolDirection = -1;
            else if (patrolDirection < 0 && transform.position.x <= leftBound)
                patrolDirection = 1;

            rb.linearVelocity = new Vector2(patrolDirection * swimSpeed, 0f);
        }

        anim.SetFloat("Speed", rb.linearVelocity.magnitude);

        UpdateFacingFromVelocity();
    }

    void StartNewRoutine(IEnumerator routine)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(routine);
    }

    IEnumerator BiteThenRetreat()
    {
        state = State.Biting;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetTrigger("Bite");

        if (audioSource != null && biteClip != null)
        {
            audioSource.pitch = Random.Range(bitePitchRange.x, bitePitchRange.y);
            audioSource.PlayOneShot(biteClip, biteVolume);
        }

        yield return new WaitForSeconds(0.25f);

        TryDamagePlayer(biteDamage);

        state = State.Retreating;

        Vector2 awayDir = (transform.position - player.position).normalized;
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

    void TryDamagePlayer(int amount)
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= biteDamageRadius)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                Debug.Log("Squid: Bite hit player, applying damage " + amount);
                health.TakeDamageFromEnemy(amount);
            }
        }
        else
        {
            Debug.Log("Squid: Bite attempted but player was out of radius. Dist = " + dist);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || state == State.Dead) return;

        if (player != null && collision.transform == player)
        {
            if (state == State.Chasing)
            {
                Debug.Log("Squid: Collision while chasing – starting bite routine");
                StartNewRoutine(BiteThenRetreat());
            }
        }
    }

    void UpdateFacingFromVelocity()
    {
        Vector2 vel = rb.linearVelocity;

        if (vel.sqrMagnitude < 0.0001f)
            return;

        if (vel.x < -0.01f)
            sr.flipX = true;
        else if (vel.x > 0.01f)
            sr.flipX = false;

        transform.rotation = Quaternion.identity;
    }

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

        Destroy(gameObject, 2f);
    }
}
