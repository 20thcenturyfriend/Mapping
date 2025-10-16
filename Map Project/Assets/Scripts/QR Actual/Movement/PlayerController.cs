using UnityEngine;
using UnityEngine.EventSystems; 


[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 2f;

    public Transform cameraTransform;

    public FixedJoystick moveJoystick;

    private Rigidbody rb;
    private bool isGrounded = true;
    private float rotationX = 0f;

    Vector2 lastTouchPos;
    bool isDragging = false;

    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f; 


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //Swipe to rotate camera pov
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            //Ignore if touch is on UI stuff (like joystick)
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
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;
            switch (touch.phase)
            {
                //Double tap to jump
                case TouchPhase.Began:
                    float timeSinceLastTap = Time.time - lastTapTime;
                    if (timeSinceLastTap <= doubleTapThreshold && isGrounded)
                    {
                        Jump(); 
                    }
                    lastTapTime = Time.time;
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

    public void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    void FixedUpdate()
    {
        float moveX = moveJoystick.Horizontal;
        float moveZ = moveJoystick.Vertical;
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 targetVelocity = move * moveSpeed;
        Vector3 velocity = rb.velocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;
        rb.velocity = velocity;
    }
    void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }
    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
    void DetectArrowNodeUnderPlayer()
    {
        TargetNode closestNode = null;
        float threshold = 1.5f;
        float closestDist = Mathf.Infinity;
        foreach (var node in FindObjectsOfType<TargetNode>())
        {
            float dist = Vector3.Distance(transform.position, node.transform.position);
            if (dist < threshold && dist < closestDist)
            {
                closestDist = dist;
                closestNode = node;
            }
        }
    }
}
