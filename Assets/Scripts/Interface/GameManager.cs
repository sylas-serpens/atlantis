using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadFromLastSave()
    {
        SaveData data = SaveSystem.LoadGame();
        if (data == null)
        {
            Debug.LogWarning("GameManager: No save data to load.");
            return;
        }

        StartCoroutine(LoadAndApply(data));
    }

    private IEnumerator LoadAndApply(SaveData data)
    {
        // 1) Load the saved scene
        Debug.Log("GameManager: Loading scene " + data.sceneName);
        SceneManager.LoadScene(data.sceneName);

        // 2) Wait one frame for the scene to finish
        yield return null;

        // 3) Find the player in the new scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("GameManager: Player not found after scene load.");
            yield break;
        }

        PlayerHealth ph = playerObj.GetComponent<PlayerHealth>();
        if (ph == null)
        {
            Debug.LogWarning("GameManager: PlayerHealth missing on Player.");
            yield break;
        }

        // 4) Apply saved state
        playerObj.transform.position = new Vector3(data.playerX, data.playerY, playerObj.transform.position.z);
        ph.currentHealth = data.playerHealth;

        Debug.Log("GameManager: Applied saved position & health.");
    }
}
