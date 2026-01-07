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

    private ContactFilter2D contactFilter;
    private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[4];
    private const float skinWidth = 0.01f;

    private Vector2 movement;

    void Start()
    {
        // Get Rigidbody2D (required)
        rb = GetComponent<Rigidbody2D>();

        // Get Animator (optional but used)
        animator = GetComponent<Animator>();

        // Get SpriteRenderer (used for flipping safely)
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Configure contact filter to collide only with solid layers and ignore triggers.
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
    }

    void Update()
    {
        // Only horizontal movement
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = 0;

        bool isMoving = Mathf.Abs(movement.x) != 0;
        if (animator != null)
        {
            animator.SetBool("isMoving", isMoving);
        }

        // Flip safely (no scale change)
        if (movement.x != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = movement.x < 0;
        }
    }

    void FixedUpdate()
    {
        // Desired movement this step
        var move = movement * moveSpeed * Time.fixedDeltaTime;
        var distance = move.magnitude;

        if (distance <= 0f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        var direction = move / distance;

        // Cast to find how far we can go without hitting a solid collider.
        var hitCount = rb.Cast(direction, contactFilter, hitBuffer, distance + skinWidth);
        var allowedDistance = distance;

        for (int i = 0; i < hitCount; i++)
        {
            // Skip if we hit our own collider
            if (hitBuffer[i].collider.attachedRigidbody == rb)
                continue;
                
            // Reserve a small skin so we don't overlap.
            allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0f, hitBuffer[i].distance - skinWidth));
            Debug.Log($"Hit: {hitBuffer[i].collider.name}, Distance: {hitBuffer[i].distance}, Allowed: {allowedDistance}");
        }

        var newPos = rb.position + direction * allowedDistance;
        rb.MovePosition(newPos);
        
        if (hitCount > 0 && allowedDistance == 0)
        {
            Debug.LogWarning("Player blocked by collision!");
        }

        // Zero velocity to prevent residual sliding into the wall.
        rb.linearVelocity = Vector2.zero;
    }
}
