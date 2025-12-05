using UnityEngine;

public class PlayerAutoSpawner : MonoBehaviour
{
    [Header("Player Spawn")]
    public GameObject playerPrefab;   
    public Transform spawnPoint;    

    void Awake()
    {
        // Do we already have a PlayerHealth in the scene?
        PlayerHealth existingPlayer = FindFirstObjectByType<PlayerHealth>();

        if (existingPlayer != null)
        {
            // A player already exists (e.g., normal levels where you placed him in the scene)
            return;
        }

        // No player present → spawn one
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerAutoSpawner: No playerPrefab assigned.", this);
            return;
        }

        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        if (spawnPoint != null)
        {
            spawnPos = spawnPoint.position;
            spawnRot = spawnPoint.rotation;
        }

        Instantiate(playerPrefab, spawnPos, spawnRot);
    }
}
