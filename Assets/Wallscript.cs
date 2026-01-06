using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Wallscript : MonoBehaviour
{
    [Header("Layers")]
    [Tooltip("Layer index of the player (e.g., 10). Ensure Physics2D matrix allows collision with this wall's layer.")]
    public int playerLayer = 10;

    [Tooltip("Optional: force this GameObject to a specific layer for collision matrix consistency. -1 leaves it unchanged.")]
    public int forceWallLayer = -1;

    BoxCollider2D _collider;

    void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();

        // Ensure the wall uses a solid collider so dynamic Rigidbody2D objects cannot pass through.
        _collider.isTrigger = false;

        // Optionally force the wall onto a specific layer so the Physics2D matrix applies.
        if (forceWallLayer >= 0 && forceWallLayer < 32)
        {
            gameObject.layer = forceWallLayer;
        }

        // Make sure collisions between the wall's layer and the player layer are NOT ignored.
        Physics2D.IgnoreLayerCollision(gameObject.layer, playerLayer, false);

        // Mark as static for performance if no Rigidbody2D is attached.
        if (!TryGetComponent<Rigidbody2D>(out _))
        {
            gameObject.isStatic = true;
        }
    }

    void OnValidate()
    {
        // Keep collider reference updated in edit mode and enforce non-trigger.
        if (!_collider) _collider = GetComponent<BoxCollider2D>();
        if (_collider) _collider.isTrigger = false;

        // Catch obvious layer mistakes while editing.
        if (forceWallLayer >= 0 && forceWallLayer < 32)
        {
            gameObject.layer = forceWallLayer;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Simple debug aid: logs when something hits this wall.
        Debug.Log($"Wall collision with {collision.collider.name} on layer {collision.collider.gameObject.layer}");
    }
}
