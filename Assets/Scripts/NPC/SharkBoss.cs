using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class SharkBoss : EnemyHealthBase
{
    public Transform player;

    [Header("Movement")]
    public float swimSpeed = 3f;
    public float retreatSpeed = 4.5f;
    public float biteRange = 1.5f;
    public float retreatDuration = 2f;

    [Header("Dash Attack")]
    public float dashSpeed = 10f;
    public float dashDuration = 1f;
    public float timeBetweenDashes = 20f;
    public float dashWindupTime = 0.7f;

    [Header("Damage")]
    public int biteDamage = 1;
    public int dashDamage = 2;
    public float biteDamageRadius = 2.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip biteClip;       // assign "Monster Bite"
    public float biteVolume = 1f;
    public Vector2 bitePitchRange = new Vector2(0.95f, 1.05f);

    public AudioClip specialClip;    // assign "45_Charge_05"
    public float specialVolume = 1f;
    public Vector2 specialPitchRange = new Vector2(0.95f, 1.05f);

    [Header("Scene On Death")]
    public bool loadSceneOnDeath = true;
    public string sceneToLoad;          // name of scene to load when shark dies
    public float deathSceneDelay = 2f;  // delay so death anim can play

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private float dashTimer;
    private Coroutine currentRoutine;

    private enum State { Chasing, Biting, Retreating, Dashing, Dead }
    private State state = State.Chasing;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.clip = null;
    }

    protected override void Start()
    {
        base.Start();

        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = Vector2.zero;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        dashTimer = timeBetweenDashes;
    }

    void Update()
    {
        if (isDead || state == State.Dead) return;
        if (player == null) return;

        if (state == State.Chasing || state == State.Retreating)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f && state == State.Chasing)
            {
                StartNewRoutine(DashAttack());
            }
        }

        if (state == State.Chasing)
        {
            Vector2 toPlayer = (player.position - transform.position);
            rb.linearVelocity = toPlayer.normalized * swimSpeed;
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

    // ---------------------------
    // BITE → DAMAGE ONCE → RETREAT
    // ---------------------------
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
        {
            state = State.Chasing;
        }

        currentRoutine = null;
    }

    // ---------------------------
    // DASH ATTACK (SPECIAL)
    // ---------------------------
    IEnumerator DashAttack()
    {
        state = State.Dashing;
        dashTimer = timeBetweenDashes;
        rb.linearVelocity = Vector2.zero;

        if (player == null)
        {
            state = State.Chasing;
            yield break;
        }

        Vector3 targetPos = player.position;
        Vector2 dashDir = (targetPos - transform.position).normalized;

        float windup = dashWindupTime;
        while (windup > 0f && !isDead)
        {
            rb.linearVelocity = Vector2.zero;
            windup -= Time.deltaTime;
            yield return null;
        }

        if (isDead)
        {
            currentRoutine = null;
            yield break;
        }

        if (anim != null)
            anim.SetTrigger("Special");

        if (audioSource != null && specialClip != null)
        {
            audioSource.pitch = Random.Range(specialPitchRange.x, specialPitchRange.y);
            audioSource.PlayOneShot(specialClip, specialVolume);
        }

        float t = 0f;
        while (t < dashDuration && !isDead)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            t += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        if (!isDead)
            state = State.Chasing;

        currentRoutine = null;
    }

    // ---------------------------
    // DAMAGE TO PLAYER (bite)
    // ---------------------------
    void TryDamagePlayer(int amount)
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= biteDamageRadius)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                Debug.Log("SharkBoss: Bite hit player, applying damage " + amount);
                health.TakeDamageFromEnemy(amount);
            }
        }
        else
        {
            Debug.Log("SharkBoss: Bite attempted but player was out of radius. Dist = " + dist);
        }
    }

    // ---------------------------
    // COLLISION HANDLING
    // ---------------------------
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || state == State.Dead) return;

        if (player != null && collision.transform == player)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();

            if (state == State.Dashing && health != null)
            {
                Debug.Log("SharkBoss: Dash collision with player, applying dash damage " + dashDamage);
                health.TakeDamageFromEnemy(dashDamage);
            }
            else if (state == State.Chasing)
            {
                Debug.Log("SharkBoss: Collision while chasing – starting bite routine");
                StartNewRoutine(BiteThenRetreat());
            }
        }
    }

    // ---------------------------
    // FACING (MIRROR LEFT/RIGHT)
    // ---------------------------
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

        if (loadSceneOnDeath && !string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneAfterDelay());
        }
        else
        {
            // fallback: just destroy shark if no scene set
            Destroy(gameObject, deathSceneDelay);
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(deathSceneDelay);
        SceneManager.LoadScene(sceneToLoad);
    }
}
