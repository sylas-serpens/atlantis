using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish Settings")]
    public GameObject[] fishPrefabs;   // all fish prefabs go here
    public int fishCount = 10;         // how many fish to spawn total
    public Vector2 spawnAreaSize = new Vector2(30f, 10f); // Width/Height of area

    void Start()
    {
        for (int i = 0; i < fishCount; i++)
        {
            SpawnFish();
        }
    }

    void SpawnFish()
    {
        if (fishPrefabs.Length == 0)
        {
            Debug.LogWarning("FishSpawner: No fish prefabs assigned!");
            return;
        }

        GameObject fishToSpawn = fishPrefabs[Random.Range(0, fishPrefabs.Length)];

        Vector3 pos = transform.position +
                      new Vector3(
                          Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                          Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
                          0f
                      );

        Instantiate(fishToSpawn, pos, Quaternion.identity);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Gizmos.DrawCube(transform.position, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 1));
    }
}
