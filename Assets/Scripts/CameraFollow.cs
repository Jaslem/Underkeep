// CameraFollow.cs
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Assign the Player transform here, or leave blank to find by tag")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("How smoothly the camera follows — lower = snappier, higher = more lag")]
    public float smoothSpeed = 6f;

    [Tooltip("How far the camera shifts toward the mouse cursor (0 = no offset, 1 = full offset)")]
    [Range(0f, 1f)]
    public float mouseInfluence = 0.25f;

    [Tooltip("Maximum distance the camera can offset toward the mouse in world units")]
    public float maxMouseOffset = 3f;

    [Header("Bounds (optional)")]
    [Tooltip("Tick to clamp the camera within a defined area")]
    public bool useBounds = false;
    public float minX, maxX, minY, maxY;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
            else
                Debug.LogWarning("CameraFollow: No player found!");
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Base position — directly on the player
        Vector3 targetPos = target.position;

        // Mouse offset — shift camera slightly toward where the player is aiming
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector2 mouseDir    = (mouseWorldPos - target.position);
        Vector2 clampedOffset = Vector2.ClampMagnitude(mouseDir * mouseInfluence, maxMouseOffset);

        Vector3 desiredPos = new Vector3(
            targetPos.x + clampedOffset.x,
            targetPos.y + clampedOffset.y,
            transform.position.z   // keep camera Z fixed
        );

        // Smooth follow
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

        // Optional bounds clamp
        if (useBounds)
        {
            smoothedPos.x = Mathf.Clamp(smoothedPos.x, minX, maxX);
            smoothedPos.y = Mathf.Clamp(smoothedPos.y, minY, maxY);
        }

        transform.position = smoothedPos;
    }
}