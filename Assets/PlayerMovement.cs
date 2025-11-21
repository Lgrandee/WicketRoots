// 14-11-2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;

    void Start()
    {
        // Get Rigidbody2D (required)
        rb = GetComponent<Rigidbody2D>();

        // Get Animator (optional but used)
        animator = GetComponent<Animator>();

        // Get SpriteRenderer (used for flipping safely)
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Only horizontal movement
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = 0;

        bool isMoving = Mathf.Abs(movement.x) != 0;
        animator.SetBool("isMoving", isMoving);

        // Flip safely (no scale change)
        if (movement.x != 0)
        {
            spriteRenderer.flipX = movement.x < 0;
        }
    }

    void FixedUpdate()
    {
        // Move using Rigidbody2D physics
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
