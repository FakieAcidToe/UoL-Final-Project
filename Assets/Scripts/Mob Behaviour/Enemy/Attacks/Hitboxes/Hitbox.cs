using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour
{
	enum KnockbackTypes
	{
		Direction,
		FromCenter,
		FromParent
	}

	[Header("Hitbox References")]
	public SpriteRenderer hitboxSprite;
	public Enemy owner;

	[Header("Base Hitbox Properties")]
	[SerializeField, Tooltip("How long the hitbox lasts"), Min(0)]
	float lifetime = 2f;
	[SerializeField, Tooltip("Damage amount to deal on hit")]
	int damage = 1;
	[SerializeField, Tooltip("What direction to knock enemies")]
	KnockbackTypes knockbackType = KnockbackTypes.Direction;
	[SerializeField, Tooltip("Knockback impulse strength applied on hit")]
	float knockback = 0.5f;
	[SerializeField, Tooltip("Hitstun duration applied on hit")]
	float hitstun = 0.05f;
	[SerializeField, Tooltip("How often hitEnemies list should be cleared to reenable them to be hit again\nNegative = Don't reenable"), Min(-1)]
	float hitboxLockout = -1f;
	[SerializeField, Tooltip("Time before hitbox becomes active (for melee hitbox startup animation sprite)"), Min(0)]
	float hitboxDelay = 0f;
	[SerializeField, Tooltip("How many enemies the hitbox can hit before dying, -1 = infinite pierce"), Min(-1)]
	int pierce = 0;
	[SerializeField, Tooltip("How long hitpause lasts on hit"), Min(0)]
	float hitpauseTime = 0.06f;

	protected Vector2 direction = Vector2.zero;
	float lifetimeTimer = 0f;

	Collider2D hitboxCollider;

	List<GameObject> hitObjects; // lockout
	float lockoutTimer = 0f;

	protected virtual void Awake()
	{
		hitObjects = new List<GameObject>();
		hitboxCollider = GetComponent<Collider2D>();
		if (hitboxDelay > 0) hitboxCollider.enabled = false;
	}

	public Vector2 GetDirection()
	{
		return direction;
	}

	public void SetDirection(Vector2 _direction)
	{
		direction = _direction.normalized;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
		if (player != null && !hitObjects.Contains(player.gameObject))
			DamagePlayer(player);

		Enemy enemy = collision.gameObject.GetComponent<Enemy>();
		if (enemy != null && enemy != owner && (owner.IsBeingControlledByPlayer() || enemy.IsBeingControlledByPlayer()) && !hitObjects.Contains(enemy.gameObject))
			DamageEnemy(enemy);
	}

	protected virtual void DamagePlayer(PlayerMovement _player)
	{
		Vector2 knockbackDirection = GetKnockbackDirection(_player.gameObject);
		_player.ReceiveKnockback(knockbackDirection * knockback, hitstun, hitpauseTime);
		hitObjects.Add(_player.gameObject);

		if (owner != null)
			owner.ApplyHitpause(hitpauseTime);

		_player.TakeDamage(damage);

		if (pierce > -1 && --pierce < 0) Destroy(gameObject); // handle piercing
	}

	protected virtual void DamageEnemy(Enemy _enemy)
	{
		// knockback and hitpause
		Vector2 knockbackDirection = GetKnockbackDirection(_enemy.gameObject);
		_enemy.ReceiveKnockback(knockbackDirection * knockback, hitstun, hitpauseTime);
		hitObjects.Add(_enemy.gameObject);

		if (owner != null)
			owner.ApplyHitpause(hitpauseTime);

		// hp damage
		PlayerMovement player = _enemy.GetControllingPlayer();
		_enemy.TakeDamage(damage);

		if (player != null && !_enemy.IsBeingControlledByPlayer()) // if player was knocked out
		{
			player.ReceiveKnockback(knockbackDirection * knockback, hitstun, hitpauseTime);
			hitObjects.Add(player.gameObject);
		}

		if (pierce > -1 && --pierce < 0) Destroy(gameObject); // handle piercing
	}

	Vector2 GetKnockbackDirection(GameObject gameObject)
	{
		switch (knockbackType)
		{
			default:
			case KnockbackTypes.Direction:
				return direction.normalized;
			case KnockbackTypes.FromCenter:
				return (gameObject.transform.position - transform.position).normalized;
			case KnockbackTypes.FromParent:
				return (gameObject.transform.position - transform.parent.position).normalized;
		}
	}

	void Update()
	{
		if (owner.hitpause <= 0)
		{
			lifetimeTimer += Time.deltaTime;

			if (hitboxLockout >= 0) // clear lockout
			{
				lockoutTimer += Time.deltaTime;
				if (lockoutTimer >= hitboxLockout)
				{
					lockoutTimer = 0;
					hitObjects.Clear();
				}
			}

			if (lifetimeTimer >= hitboxDelay)
				hitboxCollider.enabled = true;
		}
	}

	void LateUpdate()
	{
		if (lifetimeTimer >= lifetime) Destroy();
	}

	public void Destroy()
	{
		Destroy(gameObject);
	}
}