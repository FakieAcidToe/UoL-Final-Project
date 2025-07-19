using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerAnimations))]
public class PlayerMovement : MonoBehaviour
{
	enum PlayerState
	{
		idle,
		run
	}

	PlayerState state = PlayerState.idle;
	PlayerAnimations playerAnimation;

	[Header("Movement")]
	[SerializeField] float moveSpeed = 2f;

	Rigidbody2D rb;
	Vector2 movement;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		playerAnimation = GetComponent<PlayerAnimations>();
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
				playerAnimation.SetFlipX(movement);
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
		if (state != newState && playerAnimation != null)
		{
			state = newState;
			switch (state)
			{
				default:
				case PlayerState.idle:
					playerAnimation.ChangeState(PlayerAnimations.PlayerAnimState.idle);
					break;
				case PlayerState.run:
					playerAnimation.ChangeState(PlayerAnimations.PlayerAnimState.run);
					break;
			}
		}
	}
}