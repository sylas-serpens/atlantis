using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Seam-proof parallax background with automatic tile sizing,
/// micro-overlap to hide float seams, and optional vertical parallax.
/// Place 3 or more child sprites side-by-side under this GameObject.
/// </summary>
[ExecuteAlways]
public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax")]
    [Range(0f, 1f)] public float parallaxSpeed = 0.3f; // 0 = locked to camera, 1 = world-fixed
    public bool parallaxVertical = true;

    [Header("Tiling")]
    [Tooltip("World width of ONE tile. Leave 0 to auto-detect from SpriteRenderer.bounds.")]
    public float backgroundSize = 0f;

    [Tooltip("Extra camera cushion before recycling. Leave 0 to auto-compute from camera.")]
    public float viewZone = 0f;

    // --- Internals ---
    private const float EPS = 0.01f; // tiny overlap to hide seams
    private Transform cam;
    private readonly List<Transform> layers = new List<Transform>();
    private int leftIndex;
    private int rightIndex;
    private float lastCamX, lastCamY;

    void Start() => Init();

#if UNITY_EDITOR
    void OnValidate()
    {
        // Keep layout correct even in Edit Mode
        if (!Application.isPlaying && isActiveAndEnabled)
            Init();
    }
#endif

    private void Init()
    {
        cam = Camera.main ? Camera.main.transform : null;
        if (cam == null) return;

        lastCamX = cam.position.x;
        lastCamY = cam.position.y;

        // Collect & sort children left→right
        layers.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            if (t.gameObject.activeInHierarchy) layers.Add(t);
        }
        if (layers.Count == 0) return;
        layers.Sort((a, b) => a.position.x.CompareTo(b.position.x));

        leftIndex  = 0;
        rightIndex = layers.Count - 1;

        // Auto-detect true world width of one tile
        if (backgroundSize <= 0f)
        {
            var sr = layers[0].GetComponent<SpriteRenderer>();
            if (sr) backgroundSize = sr.bounds.size.x;
            else if (layers.Count > 1)
                backgroundSize = Mathf.Abs(layers[1].position.x - layers[0].position.x);
            if (backgroundSize <= 0f) backgroundSize = 1f;
        }

        // Auto-compute viewZone (camera half-width + buffer)
        if (viewZone <= 0f)
        {
            var c = Camera.main;
            float camHalfWidth = c.orthographicSize * c.aspect;
            viewZone = camHalfWidth + 0.5f;
        }
    }

    void Update()
    {
        if (cam == null || layers.Count == 0) return;

        Parallax();

        // Use while (not if) to handle fast movement / skipping tiles
        while (cam.position.x < layers[leftIndex].position.x + viewZone)
            ScrollLeft();
        while (cam.position.x > layers[rightIndex].position.x - viewZone)
            ScrollRight();
    }

    private void Parallax()
    {
        float dx = cam.position.x - lastCamX;
        transform.position += Vector3.right * (dx * parallaxSpeed);
        lastCamX = cam.position.x;

        if (parallaxVertical)
        {
            float dy = cam.position.y - lastCamY;
            transform.position += Vector3.up * (dy * parallaxSpeed);
            lastCamY = cam.position.y;
        }
    }

    private void ScrollLeft()
    {
        float y = layers[leftIndex].position.y;
        float z = layers[leftIndex].position.z;

        layers[rightIndex].position = new Vector3(
            layers[leftIndex].position.x - backgroundSize + EPS,
            y, z
        );

        leftIndex = rightIndex;
        rightIndex = (rightIndex - 1 + layers.Count) % layers.Count;
    }

    private void ScrollRight()
    {
        float y = layers[rightIndex].position.y;
        float z = layers[rightIndex].position.z;

        layers[leftIndex].position = new Vector3(
            layers[rightIndex].position.x + backgroundSize - EPS,
            y, z
        );

        rightIndex = leftIndex;
        leftIndex = (leftIndex + 1) % layers.Count;
    }

#if UNITY_EDITOR
    // Optional helper: right-click component → "Auto Layout Children"
    // Lays out 3 tiles perfectly as [-width, 0, +width].
    [ContextMenu("Auto Layout Children")]
    private void AutoLayoutChildren()
    {
        if (layers.Count == 0)
        {
            for (int i = 0; i < transform.childCount; i++)
                layers.Add(transform.GetChild(i));
        }
        layers.Sort((a, b) => a.position.x.CompareTo(b.position.x));

        float w = backgroundSize;
        if (w <= 0f)
        {
            var sr = layers[0].GetComponent<SpriteRenderer>();
            if (sr) w = sr.bounds.size.x;
        }
        if (w <= 0f) w = 1f;

        if (layers.Count >= 3)
        {
            float y = layers[1].position.y;
            float z = layers[1].position.z;
            layers[0].position = new Vector3(-w, y, z);
            layers[1].position = new Vector3( 0f, y, z);
            layers[2].position = new Vector3( w, y, z);
        }
    }
#endif
}
