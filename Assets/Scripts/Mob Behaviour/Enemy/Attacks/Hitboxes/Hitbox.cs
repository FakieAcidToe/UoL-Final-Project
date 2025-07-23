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

	List<Enemy> hitEnemies; // lockout
	float lockoutTimer = 0f;

	protected virtual void Awake()
	{
		hitEnemies = new List<Enemy>();
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
		Enemy enemy = collision.gameObject.GetComponent<Enemy>();
		if (enemy != null && !hitEnemies.Contains(enemy))
			DamageEnemy(enemy);
	}

	protected virtual void DamageEnemy(Enemy _enemy)
	{
		_enemy.TakeDamage(damage); // damage enemy

		Vector2 knockbackDirection;
		switch (knockbackType)
		{
			default:
			case KnockbackTypes.Direction:
				knockbackDirection = direction.normalized;
				break;
			case KnockbackTypes.FromCenter:
				knockbackDirection = (_enemy.transform.position - transform.position).normalized;
				break;
			case KnockbackTypes.FromParent:
				knockbackDirection = (_enemy.transform.position - transform.parent.position).normalized;
				break;
		}
		_enemy.ReceiveKnockback(knockbackDirection * knockback, hitstun, hitpauseTime);
		hitEnemies.Add(_enemy);

		if (owner != null)
			owner.ApplyHitpause(hitpauseTime);

		if (pierce > -1 && --pierce < 0) Destroy(gameObject); // handle piercing
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
					hitEnemies.Clear();
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