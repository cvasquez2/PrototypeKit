using UnityEngine;

/// <summary>
/// Handles player spaceship movement using physics forces
/// Supports forward/backward, strafe, pitch, yaw, and roll controls
/// </summary>
public class PlayerMovementSPACE : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float speedMultAngle = 0.5f;
    [SerializeField] private float speedRollMultAngle = 0.05f;

    private Rigidbody spaceshipRB;
    private Transform spaceshipTransform;

    // Cached vectors to avoid allocations each frame
    private Vector3 forwardDirection;
    private Vector3 rightDirection;
    private Vector3 rightAxis;
    private Vector3 upAxis;
    private Vector3 forwardAxis;

    /// <summary>
    /// Initialize movement system - lock cursor and cache references
    /// </summary>
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        spaceshipRB = GetComponent<Rigidbody>();
        spaceshipTransform = transform;

        // Cache axis vectors for rotation
        rightAxis = spaceshipTransform.right;
        upAxis = spaceshipTransform.up;
        forwardAxis = spaceshipTransform.forward;
    }

    /// <summary>
    /// Update movement and rotation using physics forces
    /// Called in FixedUpdate for consistent physics behavior
    /// </summary>
    void FixedUpdate()
    {
        // Cache transform directions once per frame
        forwardDirection = spaceshipTransform.TransformDirection(Vector3.forward);
        rightDirection = spaceshipTransform.TransformDirection(Vector3.right);
        rightAxis = spaceshipTransform.right;
        upAxis = spaceshipTransform.up;
        forwardAxis = spaceshipTransform.forward;

        // Get input axes
        float verticalMove = Input.GetAxis("Vertical");
        float horizontalMove = Input.GetAxis("Horizontal");
        float rollInput = Input.GetAxis("Roll");
        float mouseInputX = Input.GetAxis("Mouse X");
        float mouseInputY = Input.GetAxis("Mouse Y");

        // Apply forward/backward movement forces
        if (Mathf.Abs(verticalMove) > 0.01f)
        {
            spaceshipRB.AddForce(forwardDirection * verticalMove * speedMultiplier, ForceMode.VelocityChange);
        }

        // Apply left/right strafe forces
        if (Mathf.Abs(horizontalMove) > 0.01f)
        {
            spaceshipRB.AddForce(rightDirection * horizontalMove * speedMultiplier, ForceMode.VelocityChange);
        }

        // Apply rotation torques based on mouse input
        // Pitch (up/down rotation)
        if (Mathf.Abs(mouseInputY) > 0.01f)
        {
            spaceshipRB.AddTorque(rightAxis * speedMultAngle * mouseInputY * -1f, ForceMode.VelocityChange);
        }

        // Yaw (left/right rotation)
        if (Mathf.Abs(mouseInputX) > 0.01f)
        {
            spaceshipRB.AddTorque(upAxis * speedMultAngle * mouseInputX, ForceMode.VelocityChange);
        }

        // Roll (banking rotation)
        if (Mathf.Abs(rollInput) > 0.01f)
        {
            spaceshipRB.AddTorque(forwardAxis * speedRollMultAngle * rollInput, ForceMode.VelocityChange);
        }
    }
}
