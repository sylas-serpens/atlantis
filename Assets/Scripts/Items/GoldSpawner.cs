using UnityEngine;

public class GoldSpawner : MonoBehaviour
{
    public GameObject goldPrefab;
    public int goldCount = 20;
    public Vector2 spawnAreaSize = new Vector2(10, 5);

    void Start()
    {
        SpawnGold();
    }

    void SpawnGold()
    {
        if (goldPrefab == null) return;

        for (int i = 0; i < goldCount; i++)
        {
            Vector2 spawnPos = (Vector2)transform.position +
                               new Vector2(
                                   Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                                   Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2)
                               );

            Instantiate(goldPrefab, spawnPos, Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}
