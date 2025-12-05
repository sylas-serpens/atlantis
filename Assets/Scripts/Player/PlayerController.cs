using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float movementForce;
    public float waterDrag;
    public float strokeTime;

    public Transform light;

    [Header("Shooting")]
    public Projectile2D projectilePrefab;
    public Transform muzzle;
    public int projectileDamage = 1;
    public float projectileSpeed = 14f;
    public float projectileSpreadDegrees = 0f;
    public float recoilImpulse = 0f;

    [Header("Movement Audio")]
    public AudioSource movementAudio;
    public AudioClip movementClip;
    public float movementVolume = 1f;
    public Vector2 movementPitchRange = new Vector2(0.95f, 1.05f);

    [Header("Firing Audio")]
    public AudioSource fireAudioSource;
    public AudioClip fireClip;
    public float fireVolume = 1f;
    public Vector2 firePitchRange = new Vector2(0.95f, 1.05f);

    [Header("Scene Hotkeys (1–5)")]
    public string sceneForKey1;
    public string sceneForKey2;
    public string sceneForKey3;
    public string sceneForKey4;
    public string sceneForKey5;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 currentForce;
    private bool isDead = false;
    private bool isMoving = false;

    private Vector2 moveDirection = Vector2.zero;
    private Vector2 shotDirection = Vector2.right;
    private bool hasShotDirection = false;
    private Vector2 lastFaceDirection = Vector2.right;

    private bool darkLevel;
    private Vector3 lightLocalOffset;

    void Start()
    {
        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr  = GetComponent<SpriteRenderer>();

        rb.linearDamping = waterDrag;
        rb.freezeRotation = true;

        currentForce = Vector2.zero;
        moveDirection = Vector2.zero;
        shotDirection = Vector2.right;
        lastFaceDirection = Vector2.right;

        if (movementAudio == null)
            movementAudio = GetComponent<AudioSource>();

        if (movementAudio != null)
        {
            movementAudio.playOnAwake = false;
            movementAudio.loop = false;
        }

        if (fireAudioSource == null)
        {
            fireAudioSource = gameObject.AddComponent<AudioSource>();
            fireAudioSource.playOnAwake = false;
            fireAudioSource.loop = false;
        }

        // Light / dark-level setup
        darkLevel = (light != null);
        if (darkLevel)
        {
            // store whatever offset you set in the prefab as the "head" offset
            lightLocalOffset = light.localPosition;
        }

        StartCoroutine(strokeUpdate());
    }

    void Update()
    {
        // keep player at fixed Z
        transform.position = new Vector3(transform.position.x, transform.position.y, .43f);
        if (isDead) return;

        // ---------- SCENE HOTKEYS (1–5) ----------
        HandleSceneHotkeys();

        isMoving = false;

        // ---------------- MOVEMENT INPUT ----------------
        if (Input.GetKeyDown(KeyCode.W)) { currentForce.y += 1; isMoving = true; }
        if (Input.GetKeyDown(KeyCode.A)) { currentForce.x -= 1; isMoving = true; }
        if (Input.GetKeyDown(KeyCode.S)) { currentForce.y -= 1; isMoving = true; }
        if (Input.GetKeyDown(KeyCode.D)) { currentForce.x += 1; isMoving = true; }

        if (Input.GetKey(KeyCode.W)) isMoving = true;
        if (Input.GetKey(KeyCode.A)) isMoving = true;
        if (Input.GetKey(KeyCode.S)) isMoving = true;
        if (Input.GetKey(KeyCode.D)) isMoving = true;

        if (Input.GetKeyUp(KeyCode.W)) currentForce.y -= 1;
        if (Input.GetKeyUp(KeyCode.A)) currentForce.x += 1;
        if (Input.GetKeyUp(KeyCode.S)) currentForce.y += 1;
        if (Input.GetKeyUp(KeyCode.D)) currentForce.x -= 1;

        if (!isMoving)
            currentForce = Vector2.zero;

        moveDirection = (currentForce.sqrMagnitude > 1e-3f) ? currentForce.normalized : Vector2.zero;

        // ---------------- LEFT CLICK: LOCK SHOT DIRECTION ----------------
        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main != null)
            {
                Vector3 mouse = Input.mousePosition;
                mouse.z = Camera.main.WorldToScreenPoint(transform.position).z;
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouse);

                Vector2 dir = (mouseWorld - transform.position);
                if (dir.sqrMagnitude > 0.0001f)
                {
                    shotDirection = dir.normalized;
                    hasShotDirection = true;
                }
            }

            anim.SetTrigger("Shoot");
        }

        // ---------------- CHOOSE FACING DIRECTION FOR PLAYER ----------------
        Vector2 faceDir = lastFaceDirection;

        if (hasShotDirection)
            faceDir = shotDirection;
        else if (moveDirection.sqrMagnitude > 0.0001f)
            faceDir = moveDirection;

        if (faceDir.sqrMagnitude > 0.0001f)
            lastFaceDirection = faceDir;

        float angle = Mathf.Atan2(lastFaceDirection.y, lastFaceDirection.x) * Mathf.Rad2Deg;
        bool facingLeft = (angle > 90f || angle < -90f);

        float visualAngle = angle;
        if (facingLeft)
            visualAngle = angle > 0 ? angle - 180f : angle + 180f;

        transform.rotation = Quaternion.Euler(0f, 0f, visualAngle);
        sr.flipX = facingLeft;

        // ---------------- LIGHT: LOCK TO HEAD + AIM AT MOUSE ----------------
        if (darkLevel && light != null && Camera.main != null)
        {
            // 1) keep the light at the head offset
            light.localPosition = lightLocalOffset;

            // 2) aim toward the mouse
            Vector3 mouse = Input.mousePosition;
            mouse.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouse);
            mouseWorld.z = light.position.z;

            Vector2 lightDir = (mouseWorld - light.position);
            if (lightDir.sqrMagnitude > 0.0001f)
            {
                float lightAngle = Mathf.Atan2(lightDir.y, lightDir.x) * Mathf.Rad2Deg;

                // if your light sprite is "pointing up" in the sprite, -90 keeps the cone aligned.
                light.rotation = Quaternion.Euler(0f, 0f, lightAngle - 90f);
            }
        }

        // ---------------- ANIMATIONS ----------------
        bool isSwimming = (isMoving && currentForce.sqrMagnitude > 0.001f);
        anim.SetBool("IsSwimming", isSwimming);
    }

    // Handle numeric hotkeys 1–5 to warp to scenes
    private void HandleSceneHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && !string.IsNullOrEmpty(sceneForKey1))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneForKey1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && !string.IsNullOrEmpty(sceneForKey2))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneForKey2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && !string.IsNullOrEmpty(sceneForKey3))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneForKey3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && !string.IsNullOrEmpty(sceneForKey4))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneForKey4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && !string.IsNullOrEmpty(sceneForKey5))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneForKey5);
        }
    }

    IEnumerator strokeUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(strokeTime);

            if (!isDead && currentForce.sqrMagnitude > 0.001f)
            {
                rb.AddForce(currentForce * movementForce, ForceMode2D.Impulse);
                PlayMovementStrokeSound();
            }
        }
    }

    void PlayMovementStrokeSound()
    {
        if (movementAudio == null || movementClip == null) return;

        movementAudio.pitch = Random.Range(movementPitchRange.x, movementPitchRange.y);
        movementAudio.PlayOneShot(movementClip, movementVolume);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        currentForce = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        anim.ResetTrigger("Shoot");
        anim.SetBool("IsSwimming", false);
        anim.SetTrigger("Die");

        if (movementAudio != null) movementAudio.Stop();
        if (fireAudioSource != null) fireAudioSource.Stop();
    }

    // Called by animation event
    public void Shoot()
    {
        if (fireAudioSource != null && fireClip != null)
        {
            fireAudioSource.pitch = Random.Range(firePitchRange.x, firePitchRange.y);
            fireAudioSource.PlayOneShot(fireClip, fireVolume);
        }

        if (!projectilePrefab) return;

        Vector3 spawnPos = muzzle ? muzzle.position : transform.position;

        Vector2 dir = hasShotDirection ? shotDirection : lastFaceDirection;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        dir.Normalize();

        if (projectileSpreadDegrees > 0f)
        {
            float half = projectileSpreadDegrees * 0.5f;
            float jitter = Random.Range(-half, half);
            dir = Quaternion.Euler(0, 0, jitter) * dir;
        }

        Projectile2D proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.dir    = dir;
        proj.speed  = projectileSpeed;
        proj.damage = projectileDamage;

        hasShotDirection = false;

        if (recoilImpulse > 0f)
            rb.AddForce(-dir * recoilImpulse, ForceMode2D.Impulse);
    }
}
