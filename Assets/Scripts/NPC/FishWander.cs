using UnityEngine;

public class FishWander : MonoBehaviour
{
    public float speed = 2f;
    public float changeDirTime = 2f;

    private Vector2 direction;
    private float timer;

    void Start()
    {
        PickNewDirection();
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);

        timer += Time.deltaTime;
        if (timer >= changeDirTime)
        {
            PickNewDirection();
        }
    }

    void PickNewDirection()
    {
        timer = 0;
        direction = Random.insideUnitCircle.normalized;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamageFromEnemy(1);
        }
    }
}
