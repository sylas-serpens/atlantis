using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneTriggerInteraction : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad;

    [Header("UI Settings")]
    public GameObject interactionBox;   
    public TMP_Text interactionText;   
    [TextArea]
    public string message = "Press SPACE to continue";

    private bool playerInRange = false;

    void Start()
    {
        if (interactionBox != null)
            interactionBox.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (interactionBox != null)
            {
                interactionText.text = message;
                interactionBox.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactionBox != null)
                interactionBox.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
