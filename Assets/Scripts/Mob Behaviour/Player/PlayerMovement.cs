using UnityEngine;
using static Enemy;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerAnimations))]
public class PlayerMovement : MonoBehaviour
{
	enum PlayerState
	{
		idle,
		run,
		hurt
	}

	PlayerState state = PlayerState.idle;
	PlayerAnimations playerAnimation;
	PlayerInputActions controls;

	[Header("Movement")]
	[SerializeField] float moveSpeed = 2f;

	[Header("HP")]
	[SerializeField] int maxHp = 20;
	int hp;
	[HideInInspector] public HealthbarUI healthbar;

	Rigidbody2D rb;

	// movement
	Vector2 movement;
	float hitstun = 0; // time left in hitstun (cant move)
	public float hitpause { private set; get; }
	Vector2 knockback; // knockback to apply after hitpause

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		playerAnimation = GetComponent<PlayerAnimations>();
		controls = new PlayerInputActions();

		hp = maxHp;
	}

	void OnEnable()
	{
		controls.Gameplay.Enable();
	}

	void OnDisable()
	{
		controls.Gameplay.Disable();
	}

	void Start()
	{
		healthbar.SetHealth(hp, false);
		healthbar.SetMaxHealth(hp, false);
	}

	void Update()
	{
		if (hitpause > 0)
		{
			hitpause -= Time.deltaTime;
			if (hitpause <= 0)
			{
				hitpause = 0;
				if (knockback != Vector2.zero)
				{
					rb.AddForce(knockback, ForceMode2D.Impulse);
					knockback = Vector2.zero;
				}
			}
		}

		if (hitpause <= 0)
		{
			switch (state)
			{
				case PlayerState.idle:
				case PlayerState.run:
					movement = controls.Gameplay.Move.ReadValue<Vector2>();

					// Normalize diagonal movement
					if (movement.sqrMagnitude > 1) movement.Normalize();

					// set animation if moving
					ChangeState(movement.sqrMagnitude > 0 ? PlayerState.run : PlayerState.idle);
					playerAnimation.SetFlipX(movement);
					break;
				case PlayerState.hurt:
					movement = Vector2.zero;
					hitstun -= Time.deltaTime;

					if (hitstun <= 0)
					{
						ChangeState(PlayerState.idle);
						hitstun = 0;
					}
					break;
			}
		}
	}

	void FixedUpdate()
	{
		if (state != PlayerState.hurt)
		{
			// move the player using physics
			rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
		}
	}

	public void TakeDamage(int damage)
	{
		hp = Mathf.Clamp(hp - damage, 0, maxHp);
		healthbar.SetHealth(hp);
	}

	public void ReceiveKnockback(Vector2 _force, float _hitstun, float _hitpause)
	{
		hitstun = _hitstun;
		if (hitstun > 0)
		{
			ChangeState(PlayerState.hurt);
			playerAnimation.SetFlipX(_force * -1);
		}

		knockback = _force;
		if (_hitpause <= 0)
			rb.AddForce(knockback, ForceMode2D.Impulse);
		else
			ApplyHitpause(_hitpause);
	}

	public void ApplyHitpause(float _hitpause)
	{
		hitpause = Mathf.Max(_hitpause, hitpause);
		rb.velocity = Vector2.zero;
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
				case PlayerState.hurt:
					playerAnimation.ChangeState(PlayerAnimations.PlayerAnimState.hurt);
					break;
			}
		}
	}
}