using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    private float minX, maxX, minY, maxY;

    void Start()
    {
        // Find all CameraWall objects and use their bounds
        GameObject[] walls = GameObject.FindGameObjectsWithTag("CameraWall");
        
        if (walls.Length > 0)
        {
            minX = Mathf.Infinity;
            maxX = -Mathf.Infinity;
            minY = Mathf.Infinity;
            maxY = -Mathf.Infinity;

            foreach (GameObject wall in walls)
            {
                BoxCollider2D box = wall.GetComponent<BoxCollider2D>();
                if (box != null)
                {
                    Bounds bounds = box.bounds;
                    minX = Mathf.Min(minX, bounds.center.x);
                    maxX = Mathf.Max(maxX, bounds.center.x);
                    minY = Mathf.Min(minY, bounds.center.y);
                    maxY = Mathf.Max(maxY, bounds.center.y);
                }
            }
            Debug.Log($"Camera boundaries: X({minX} to {maxX}), Y({minY} to {maxY})");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        Camera cam = GetComponent<Camera>();
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float clampedX = Mathf.Clamp(smoothedPosition.x, minX + halfWidth, maxX - halfWidth);
        float clampedY = Mathf.Clamp(smoothedPosition.y, minY + halfHeight, maxY - halfHeight);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}
