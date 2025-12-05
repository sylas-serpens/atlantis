using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class Dialogue : MonoBehaviour
{
    public enum Speaker { A, B }

    [Header("UI")]
    public TextMeshProUGUI dialogueText;
    public Image leftPortrait;
    public Image rightPortrait;
    public Image fadeImage;

    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    public Speaker[] speakers;
    public float textSpeed = 0.03f;

    [Header("Scene Transition")]
    public bool loadSceneOnEnd = false;
    public string nextSceneName;
    public float fadeDuration = 1f;

    [Header("Typing Audio")]
    public AudioClip typeClip;
    public float typeVolume = 1f;
    public Vector2 typePitchRange = new Vector2(0.95f, 1.05f);
    public float minTimeBetweenTypeSounds = 0.02f;

    private int index = 0;
    private bool isTyping = false;
    private Coroutine typingCo;

    private AudioSource audioSource;
    private float lastTypeSoundTime = -999f;

    void Start()
    {
        // Basic validation
        if (!dialogueText || !leftPortrait || !rightPortrait)
        {
            Debug.LogError("Dialogue: Missing UI references, disabling.");
            enabled = false;
            return;
        }

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("Dialogue: No lines, disabling.");
            gameObject.SetActive(false);
            return;
        }

        if (speakers == null || speakers.Length != dialogueLines.Length)
        {
            Debug.LogError("Dialogue: speakers length must match dialogueLines length.");
            enabled = false;
            return;
        }

        // Fade image start alpha
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        // Audio setup
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && typeClip != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        dialogueText.text = "";
        PlayCurrentLine();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
                FinishTyping();
            else
                NextLine();
        }
    }

    void PlayCurrentLine()
    {
        bool aTalking = speakers[index] == Speaker.A;

        leftPortrait.enabled  = aTalking;
        rightPortrait.enabled = !aTalking;

        if (typingCo != null)
            StopCoroutine(typingCo);

        typingCo = StartCoroutine(Type(dialogueLines[index]));
    }

    IEnumerator Type(string s)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in s)
        {
            dialogueText.text += c;

            // Play type sound (with rate limiting)
            if (typeClip != null && audioSource != null)
            {
                if (Time.time - lastTypeSoundTime >= minTimeBetweenTypeSounds)
                {
                    audioSource.pitch = Random.Range(typePitchRange.x, typePitchRange.y);
                    audioSource.PlayOneShot(typeClip, typeVolume);
                    lastTypeSoundTime = Time.time;
                }
            }

            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        typingCo = null;
    }

    void FinishTyping()
    {
        if (typingCo != null)
            StopCoroutine(typingCo);

        typingCo = null;
        isTyping = false;
        dialogueText.text = dialogueLines[index];
    }

    void NextLine()
    {
        index++;

        if (index < dialogueLines.Length)
        {
            PlayCurrentLine();
        }
        else
        {
            // All lines done
            dialogueText.text = "";
            leftPortrait.enabled  = false;
            rightPortrait.enabled = false;

            if (loadSceneOnEnd && !string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log("Dialogue: Finished, loading scene: " + nextSceneName);
                StartCoroutine(FadeAndLoad());
            }
            else
            {
                Debug.Log("Dialogue: Finished, deactivating dialogue object.");
                gameObject.SetActive(false);
            }
        }
    }

    IEnumerator FadeAndLoad()
    {
        if (fadeImage != null)
        {
            float t = 0f;
            Color c = fadeImage.color;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / fadeDuration);
                c.a = a;
                fadeImage.color = c;
                yield return null;
            }
        }

        // Make sure scene name is correct + in Build Settings
        SceneManager.LoadScene(nextSceneName);
    }
}
