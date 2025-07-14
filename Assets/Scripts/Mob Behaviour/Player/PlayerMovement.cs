using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
	enum PlayerState
	{
		idle,
		run
	}

	[Header("Animation")]
	[SerializeField] PlayerAnimSetLoader animLoader;
	PlayerState state = PlayerState.idle;


	[Header("Movement")]
	[SerializeField] float moveSpeed = 2f;

	Rigidbody2D rb;
	Vector2 movement;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		switch (state)
		{
			default:
			case PlayerState.idle:
			case PlayerState.run:
				movement.x = Input.GetAxisRaw("Horizontal");
				movement.y = Input.GetAxisRaw("Vertical");

				// Normalize diagonal movement
				if (movement.sqrMagnitude > 1) movement.Normalize();

				// set animation if moving
				ChangeState(movement.sqrMagnitude > 0 ? PlayerState.run : PlayerState.idle);
				animLoader.SetFlipX(movement);
				break;
		}
	}

	void FixedUpdate()
	{
		// move the player using physics
		rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
	}

	void ChangeState(PlayerState newState)
	{
		if (state != newState && animLoader != null)
		{
			state = newState;
			switch (state)
			{
				default:
				case PlayerState.idle:
					animLoader.ChangeState(PlayerAnimSetLoader.PlayerAnimState.idle);
					break;
				case PlayerState.run:
					animLoader.ChangeState(PlayerAnimSetLoader.PlayerAnimState.run);
					break;
			}
		}
	}
}