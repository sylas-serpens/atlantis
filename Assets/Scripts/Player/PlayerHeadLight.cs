using UnityEngine;

public class PlayerHeadLight : MonoBehaviour
{
    [Header("References")]
    public Transform playerHead;   

    [Header("Offset")]
    public Vector3 localOffset = Vector3.zero; 

    [Header("Rotation")]
    public float rotationOffsetDegrees = -90f; 

    void LateUpdate()
    {
        if (playerHead == null) return;
        if (Camera.main == null) return;

        // 1) FOLLOW PLAYER HEAD POSITION
        Vector3 targetPos = playerHead.position + playerHead.TransformVector(localOffset);

        // keep original Z so it stays in correct layer depth
        targetPos.z = transform.position.z;
        transform.position = targetPos;

        // 2) AIM TOWARD MOUSE
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = targetPos.z;

        Vector2 dir = (mouseWorld - targetPos);
        if (dir.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffsetDegrees);
    }
}
