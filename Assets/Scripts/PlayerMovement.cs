using UnityEngine;

/// <summary>
/// Simple, generic 2D step-based player movement for prototypes.
/// - Uses WASD / arrow keys by default
/// - Moves in 4 directions on a grid (1 unit steps)
/// - Optional Animator + SpriteRenderer for facing/animations
/// 
/// Customize by:
/// - Changing key bindings
/// - Changing step size and move delay
/// - Hooking into the movement events
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveStepSize = 1f;      // How far each move step is
    [SerializeField] private float moveDelay = 0.15f;      // Time between move steps
    [SerializeField] private bool useGridMovement = true;  // If false, uses continuous movement

    [Header("Input Keys")]
    [SerializeField] private KeyCode moveLeftKey  = KeyCode.A;
    [SerializeField] private KeyCode moveRightKey = KeyCode.D;
    [SerializeField] private KeyCode moveUpKey    = KeyCode.W;
    [SerializeField] private KeyCode moveDownKey  = KeyCode.S;

    [Header("Optional Visuals")]
    [SerializeField] private Animator animator;            // Optional
    [SerializeField] private SpriteRenderer spriteRenderer; // Optional
    [SerializeField] private string idleAnimationName = "PlayerIdle";
    [SerializeField] private string moveAnimationName = "PlayerMove";

    private Rigidbody2D rb;
    private bool isMoving;
    private bool faceLeft;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Auto-grab components if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Read raw input each frame
        Vector2 input = GetInputDirection();

        if (useGridMovement)
        {
            HandleGridMovement(input);
        }
        else
        {
            HandleContinuousMovement(input);
        }

        // Visuals
        UpdateFacing(input);
        UpdateAnimations(input);
    }

    /// <summary>
    /// Returns a normalized direction from the current key input.
    /// </summary>
    private Vector2 GetInputDirection()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(moveLeftKey))  x -= 1f;
        if (Input.GetKey(moveRightKey)) x += 1f;
        if (Input.GetKey(moveUpKey))    y += 1f;
        if (Input.GetKey(moveDownKey))  y -= 1f;

        Vector2 dir = new Vector2(x, y);

        // Enforce 4-directional movement (no diagonals) if desired
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            dir.y = 0f;
        else
            dir.x = 0f;

        return dir.normalized;
    }

    #region Movement Modes

    private void HandleGridMovement(Vector2 input)
    {
        if (isMoving || input == Vector2.zero)
            return;

        Vector3 targetPos = transform.position + (Vector3)(input * moveStepSize);
        StartCoroutine(MoveStep(targetPos));
    }

    private System.Collections.IEnumerator MoveStep(Vector3 targetPos)
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < moveDelay)
        {
            float t = elapsed / moveDelay;
            rb.MovePosition(Vector3.Lerp(startPos, targetPos, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(targetPos);
        isMoving = false;
    }

    private void HandleContinuousMovement(Vector2 input)
    {
        if (input == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = input * (moveStepSize / moveDelay); // reuse vars as "speed"
    }

    #endregion

    #region Visuals

    private void UpdateFacing(Vector2 input)
    {
        if (spriteRenderer == null)
            return;

        if (input.x < 0f)
            faceLeft = true;
        else if (input.x > 0f)
            faceLeft = false;

        spriteRenderer.flipX = faceLeft;
    }

    private void UpdateAnimations(Vector2 input)
    {
        if (animator == null)
            return;

        bool isMovingNow = input != Vector2.zero;

        if (isMovingNow && !string.IsNullOrEmpty(moveAnimationName))
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(moveAnimationName))
                animator.Play(moveAnimationName);
        }
        else if (!string.IsNullOrEmpty(idleAnimationName))
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(idleAnimationName))
                animator.Play(idleAnimationName);
        }
    }

    #endregion

    #region Public API (for easy tweaking/extension)

    public void SetMoveStepSize(float value) => moveStepSize = value;
    public void SetMoveDelay(float value) => moveDelay = value;
    public void SetGridMovement(bool enabled) => useGridMovement = enabled;

    public void SetInputKeys(KeyCode left, KeyCode right, KeyCode up, KeyCode down)
    {
        moveLeftKey = left;
        moveRightKey = right;
        moveUpKey = up;
        moveDownKey = down;
    }

    #endregion
}