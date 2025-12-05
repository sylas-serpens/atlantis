using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class UIButtonSFX : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler,
    ISelectHandler,
    ISubmitHandler
{
    [Header("Hover / Select")]
    public AudioClip hoverClip;
    public float hoverVolume = 1f;
    public Vector2 hoverPitchRange = new Vector2(0.95f, 1.05f);

    [Header("Click / Submit")]
    public AudioClip clickClip;
    public float clickVolume = 1f;
    public Vector2 clickPitchRange = new Vector2(0.95f, 1.05f);

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void PlayClip(AudioClip clip, float volume, Vector2 pitchRange)
    {
        if (clip == null || audioSource == null) return;

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(clip, volume);
    }

    // Mouse moves over button
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayClip(hoverClip, hoverVolume, hoverPitchRange);
    }

    // Mouse clicks button
    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClip(clickClip, clickVolume, clickPitchRange);
    }

    // Keyboard / gamepad moves selection onto this button
    public void OnSelect(BaseEventData eventData)
    {
        PlayClip(hoverClip, hoverVolume, hoverPitchRange);
    }

    // Keyboard / gamepad “submit” (Enter / A button)
    public void OnSubmit(BaseEventData eventData)
    {
        PlayClip(clickClip, clickVolume, clickPitchRange);
    }
}
