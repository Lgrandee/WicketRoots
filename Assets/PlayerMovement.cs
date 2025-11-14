// 14-11-2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private Vector3 originalScale;

    void Start()
    {
        // Try to find the Rigidbody2D on this GameObject. If it's missing,
        // add one at runtime so the game doesn't throw MissingComponentException.
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            Debug.LogWarning("PlayerMovement: Rigidbody2D was missing on '" + gameObject.name + "'. Added one at runtime.");
        }

        animator = GetComponent<Animator>();
        originalScale = transform.localScale; // Store the original scale
    }

    void Update()
    {
        // Only allow horizontal movement
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = 0; // Prevent vertical movement

        bool isMoving = Mathf.Abs(movement.x) > 0;
        animator.SetBool("isMoving", isMoving);

        if (movement.x != 0)
        {
            // Flip the character horizontally while preserving the original scale
            transform.localScale = new Vector3(originalScale.x * Mathf.Sign(movement.x), originalScale.y, originalScale.z);
        }
    }

    void FixedUpdate()
    {
        // ✅ Defensive: skip movement if Rigidbody2D missing
        if (rb == null)
        {
            Debug.LogWarning("Rigidbody2D is missing on '" + gameObject.name + "' — skipping movement.");
            return;
        }

        // Move only along the x-axis
        rb.MovePosition(new Vector2(rb.position.x + movement.x * moveSpeed * Time.fixedDeltaTime, rb.position.y));
    }
}