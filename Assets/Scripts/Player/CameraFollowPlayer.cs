using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform target;          // Player transform (optional to assign in Inspector)
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float smoothTime = 0.2f;

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // If not set in Inspector, try to find the player automatically
        if (target == null)
        {
            FindPlayer();
        }
    }

    void LateUpdate()
    {
        // If target was destroyed or missing, try to find again
        if (target == null)
        {
            FindPlayer();
            if (target == null)
            {
                Debug.Log("could no find player");
                // No player found this frame; do nothing instead of throwing
                return;
            }
        }

        // Now it's safe to use target.position
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
    }
}
