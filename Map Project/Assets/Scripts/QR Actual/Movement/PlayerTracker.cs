using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class PlayerTracker : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;
    public FixedJoystick moveJoystick;

    private Rigidbody rb;

    private Waypoint currentWaypoint;
    private Waypoint targetWaypoint;

    private float rotationX = 0f;
    private Vector2 lastTouchPos;
    private bool isDragging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentWaypoint = FindNearestWaypoint(transform.position);
    }

    void Update()
    {
        HandleCameraLook();

        // Pick target waypoint based on joystick direction
        if (moveJoystick.Direction.magnitude > 0.1f && currentWaypoint != null)
        {
            Vector3 inputDir = GetWorldInputDirection();
            targetWaypoint = FindBestNeighbor(currentWaypoint, inputDir);
        }
    }

    void FixedUpdate()
    {
        if (targetWaypoint == null) return;

        Vector3 dir = (targetWaypoint.Position - transform.position).normalized;
        Vector3 move = dir * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(transform.position + move);

        if (Vector3.Distance(transform.position, targetWaypoint.Position) < 0.5f)
        {
            currentWaypoint = targetWaypoint;
        }
    }

    // --- CAMERA CONTROL ---
    void HandleCameraLook()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    lastTouchPos = touch.position;
                    isDragging = true;
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 delta = touch.deltaPosition;

                        float rotateX = delta.x * mouseSensitivity * 0.1f;
                        float rotateY = delta.y * mouseSensitivity * 0.1f;

                        transform.Rotate(Vector3.up * rotateX);

                        rotationX -= rotateY;
                        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
                        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
                    }
                    break;

                case TouchPhase.Ended:
                    isDragging = false;
                    break;
            }
        }
    }

    // --- HELPER METHODS ---
    Vector3 GetWorldInputDirection()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Flatten to XZ plane
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        return (forward * moveJoystick.Vertical + right * moveJoystick.Horizontal).normalized;
    }

    Waypoint FindNearestWaypoint(Vector3 pos)
    {
        Waypoint[] all = FindObjectsOfType<Waypoint>();
        Waypoint nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var wp in all)
        {
            float d = Vector3.Distance(pos, wp.Position);
            if (d < minDist)
            {
                minDist = d;
                nearest = wp;
            }
        }
        return nearest;
    }

    Waypoint FindBestNeighbor(Waypoint from, Vector3 inputDir)
    {
        Waypoint best = null;
        float maxDot = -Mathf.Infinity;

        foreach (var neighbor in from.neighbors)
        {
            Vector3 toNeighbor = (neighbor.Position - from.Position).normalized;
            float dot = Vector3.Dot(inputDir, toNeighbor);

            if (dot > maxDot)
            {
                maxDot = dot;
                best = neighbor;
            }
        }
        return best;
    }
}
