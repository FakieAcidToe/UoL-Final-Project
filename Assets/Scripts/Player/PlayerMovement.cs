using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
	[SerializeField] float moveSpeed = 2f;

	Rigidbody2D rb;
	Vector2 movement;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		movement.x = Input.GetAxisRaw("Horizontal");
		movement.y = Input.GetAxisRaw("Vertical");

		// Normalize diagonal movement
		if (movement.sqrMagnitude > 1) movement.Normalize();
	}

	void FixedUpdate()
	{
		// move the player using physics
		rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
	}
}