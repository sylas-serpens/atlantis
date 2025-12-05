using UnityEngine;
using UnityEngine.SceneManagement;

public class StageTeleporter : MonoBehaviour
{
    [Header("Destination")]
    public string sceneToLoad;                   // EXACT name from Build Settings
    public string receiverName = "TeleportReceiver";

    [Header("Input")]
    public KeyCode interactKey = KeyCode.Space;

    [Header("UI Prompt")]
    public GameObject promptBox;                 // Assign a UI panel / worldspace box here

    // Static so we can carry the player between scenes
    private static Transform playerToMove;
    private static string receiverToUse;

    private bool playerInZone = false;
    private Transform currentPlayer;

    void Start()
    {
        // Make sure the prompt starts hidden
        if (promptBox != null)
            promptBox.SetActive(false);
    }

    void Update()
    {
        if (playerInZone && currentPlayer != null && Input.GetKeyDown(interactKey))
        {
            BeginTeleport();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            currentPlayer = other.transform;

            if (promptBox != null)
                promptBox.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform == currentPlayer)
        {
            playerInZone = false;
            currentPlayer = null;

            if (promptBox != null)
                promptBox.SetActive(false);
        }
    }

    void BeginTeleport()
    {
        if (currentPlayer == null) return;

        // Hide prompt when we actually teleport
        if (promptBox != null)
            promptBox.SetActive(false);

        playerToMove  = currentPlayer;
        receiverToUse = receiverName;

        DontDestroyOnLoad(playerToMove.gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneToLoad);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (playerToMove == null)
            return;

        GameObject receiverObj = GameObject.Find(receiverToUse);
        if (receiverObj != null)
        {
            Vector3 spawnPos = receiverObj.transform.position;
            spawnPos.z = playerToMove.position.z; // keep player Z
            playerToMove.position = spawnPos;
        }
        else
        {
            Debug.LogWarning("StageTeleporter: Receiver object '" + receiverToUse +
                             "' not found in scene '" + scene.name + "'");
        }

        SceneManager.MoveGameObjectToScene(playerToMove.gameObject, scene);

        playerToMove  = null;
        receiverToUse = null;
    }
}
