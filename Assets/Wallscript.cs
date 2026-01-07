using UnityEngine;

// Solid wall: static rigidbody + non-trigger collider to block movement casts.
[RequireComponent(typeof(BoxCollider2D))]
public class Wallscript : MonoBehaviour
{
	private void Awake()
	{
		var box = GetComponent<BoxCollider2D>();
		box.isTrigger = false; // Use solid collision

		var rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
		rb.bodyType = RigidbodyType2D.Static;
		rb.gravityScale = 0f;
		rb.constraints = RigidbodyConstraints2D.FreezeAll;
	}
}
