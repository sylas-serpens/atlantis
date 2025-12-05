using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button saveGameButton;
    public Button loadGameButton;
    public Button mainMenuButton;

    private bool isPaused = false;

    private Transform player;
    private PlayerHealth playerHealth;

    void Start()
    {
        // Find player + health
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
        else
        {
            Debug.LogWarning("PauseMenu: No Player tagged 'Player' found.");
        }

        // Hook up button listeners here (so they ALWAYS point at this instance)
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (saveGameButton != null)
            saveGameButton.onClick.AddListener(OnClick_SaveGame);

        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(OnClick_LoadGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnClick_ReturnToMainMenu);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        else
            Debug.LogWarning("PauseMenu: pauseMenuPanel is not assigned.");

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        Debug.Log("PauseMenu: Resume() called");

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    void Pause()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    // SAVE
    public void OnClick_SaveGame()
    {
        Debug.Log("PauseMenu: OnClick_SaveGame() called");

        if (player == null || playerHealth == null)
        {
            Debug.LogWarning("PauseMenu: Cannot save – player or PlayerHealth missing.");
            return;
        }

        SaveData data = new SaveData();
        data.sceneName    = SceneManager.GetActiveScene().name;
        data.playerX      = player.position.x;
        data.playerY      = player.position.y;
        data.playerHealth = playerHealth.currentHealth;

        SaveSystem.SaveGame(data);
        Debug.Log("PauseMenu: Game Saved.");
    }

    // LOAD (use GameManager)
    public void OnClick_LoadGame()
    {
        Debug.Log("PauseMenu: OnClick_LoadGame() called");

        if (!SaveSystem.HasSave())
        {
            Debug.LogWarning("PauseMenu: No save to load.");
            return;
        }

        Time.timeScale = 1f;
        isPaused = false;

        if (GameManager.Instance != null)
        {
            Debug.Log("PauseMenu: Requesting GameManager to load save.");
            GameManager.Instance.LoadFromLastSave();
        }
        else
        {
            Debug.LogWarning("PauseMenu: No GameManager.Instance found.");
        }
    }

    public void OnClick_ReturnToMainMenu()
    {
        Debug.Log("PauseMenu: OnClick_ReturnToMainMenu() called");

        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
}
