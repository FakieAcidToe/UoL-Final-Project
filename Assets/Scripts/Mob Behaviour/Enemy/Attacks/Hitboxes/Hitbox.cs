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

	enum HitProjectileBehaviour
	{
		DontHit,
		Destroy,
		Reflect
	}

	[Header("Hitbox References")]
	public SpriteRenderer hitboxSprite;
	public Enemy owner;

	[Header("Base Hitbox Properties")]
	[SerializeField, Tooltip("How long the hitbox lasts"), Min(0)]
	float lifetime = 2f;
	[SerializeField, Tooltip("Damage amount to deal on hit")]
	int damage = 1;
	[SerializeField, Tooltip("Damage increase per level")]
	int damageScaling = 1;
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
	[SerializeField, Tooltip("Time when hitbox becomes not active. 0f = Don't disable hitbox"), Min(0)]
	float hitboxDelayEnd = 0f;
	[SerializeField, Tooltip("How many enemies the hitbox can hit before dying, -1 = infinite pierce"), Min(-1)]
	int pierce = 0;
	[SerializeField, Tooltip("How long hitpause lasts on hit"), Min(0)]
	float hitpauseTime = 0.06f;
	[SerializeField, Tooltip("How long Screenshake lasts on hit\nRecommended same duration as hitpause"), Min(0)]
	float screenshakeDuration = 0.06f;
	[SerializeField, Tooltip("How powerful the Screenshake feels\nRecommended half of knockback strength"), Min(0)]
	float screenshakeMagnitude = 0.25f;
	[SerializeField, Tooltip("If this hitbox can hit pressure plate items (usually decorations)")]
	bool canHitPressurePlates = true;
	[SerializeField, Tooltip("What happens when this hitbox hits an opposing projectile?")]
	HitProjectileBehaviour hitProjectileBehaviour = HitProjectileBehaviour.DontHit;
	[SerializeField]
	AudioClip sfx;

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

	public virtual void SetDirection(Vector2 _direction)
	{
		direction = _direction.normalized;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		CollisionDetected(collision.gameObject);
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		CollisionDetected(collision.gameObject);
	}

	protected virtual void CollisionDetected(GameObject collisionGO)
	{
		PlayerMovement player = collisionGO.GetComponent<PlayerMovement>();
		if (player != null && (!player.IsInvince() || player.IsDead()) && !hitObjects.Contains(player.gameObject))
			DamagePlayer(player);

		Enemy enemy = collisionGO.GetComponent<Enemy>();
		if (enemy != null && !enemy.IsInvince() && enemy != owner && (owner == null || owner.IsBeingControlledByPlayer() || enemy.IsBeingControlledByPlayer()) && !hitObjects.Contains(enemy.gameObject))
			DamageEnemy(enemy);

		Projectile proj = collisionGO.GetComponent<Projectile>();
		if (proj != null && proj.owner != owner && (owner == null || owner.IsBeingControlledByPlayer() || proj.owner == null || proj.owner.IsBeingControlledByPlayer()) && !hitObjects.Contains(proj.gameObject))
			HitProjectile(proj);
	}

	protected virtual void DamagePlayer(PlayerMovement _player)
	{
		if (owner != null)
		{
			float multipliedHitpauseTime = hitpauseTime * SaveManager.Instance.CurrentSaveData.feedbackDuration;

			Vector2 knockbackDirection = GetKnockbackDirection(_player.gameObject);
			_player.ReceiveKnockback(knockbackDirection * knockback, hitstun, multipliedHitpauseTime);
			hitObjects.Add(_player.gameObject);

			owner.ApplyHitpause(multipliedHitpauseTime);

			int damageAmount = damage + damageScaling * (owner.level - 1);
			if (owner.IsBeingControlledByPlayer())
				damageAmount = Mathf.FloorToInt(damageAmount * owner.playerStats.attackMult);
			else
				damageAmount = Mathf.FloorToInt(damageAmount * owner.playerStats.reveiveAttackMult);
			_player.TakeDamage(damageAmount);

			// damage numbers
			if (damageAmount > 0)
				DamageNumberSpawner.Instance.SpawnDamageNumbers(damageAmount, Vector3.Lerp(transform.position, _player.transform.position, 0.5f));

			// invince
			_player.SetInvince();

			// screenshake
			ScreenShake.Instance.Shake(
				screenshakeDuration * SaveManager.Instance.CurrentSaveData.feedbackDuration,
				screenshakeMagnitude * SaveManager.Instance.CurrentSaveData.screenshake);

			// sfx
			_player.PlaySFX(sfx);

			if (pierce > -1 && --pierce < 0) Destroy(); // handle piercing
		}
	}

	protected virtual void DamageEnemy(Enemy _enemy)
	{
		float multipliedHitpauseTime = hitpauseTime * SaveManager.Instance.CurrentSaveData.feedbackDuration;

		// knockback and hitpause
		Vector2 knockbackDirection = GetKnockbackDirection(_enemy.gameObject);
		_enemy.ReceiveKnockback(knockbackDirection * knockback, hitstun, multipliedHitpauseTime);
		hitObjects.Add(_enemy.gameObject);

		if (owner != null && !(this is Projectile))
			owner.ApplyHitpause(multipliedHitpauseTime);

		// hp damage
		int damageAmount = damage + damageScaling * (owner == null ? 0 : owner.level - 1);
		if (owner != null)
		{
			if (owner.IsBeingControlledByPlayer())
				damageAmount = Mathf.FloorToInt(damageAmount * owner.playerStats.attackMult);
			else
				damageAmount = Mathf.FloorToInt(damageAmount * owner.playerStats.reveiveAttackMult);
		}
		PlayerMovement player = _enemy.GetControllingPlayer();
		_enemy.TakeDamage(damageAmount);

		// damage numbers
		if (damageAmount > 0)
			DamageNumberSpawner.Instance.SpawnDamageNumbers(damageAmount, Vector3.Lerp(transform.position, _enemy.transform.position, 0.5f));

		if (player != null && !_enemy.IsBeingControlledByPlayer()) // if player was knocked out
		{
			player.ReceiveKnockback(knockbackDirection * knockback, hitstun, multipliedHitpauseTime);
			player.SetInvince();
			hitObjects.Add(player.gameObject);
		}

		// invince
		if (_enemy != null && _enemy.IsBeingControlledByPlayer())
			_enemy.SetInvince();

		//if (owner != null) owner.OnDealDamage(_enemy);

		// screenshake
		ScreenShake.Instance.Shake(
			screenshakeDuration * SaveManager.Instance.CurrentSaveData.feedbackDuration,
			screenshakeMagnitude * SaveManager.Instance.CurrentSaveData.screenshake);

		// sfx
		_enemy.PlaySFX(sfx);

		if (pierce > -1 && --pierce < 0) Destroy(); // handle piercing
	}

	protected virtual void HitProjectile(Projectile _proj)
	{
		switch (hitProjectileBehaviour)
		{
			default:
			case HitProjectileBehaviour.DontHit:
				return;
			case HitProjectileBehaviour.Destroy:
				_proj.Destroy();
				break;
			case HitProjectileBehaviour.Reflect:
				Vector2 knockbackDirection = GetKnockbackDirection(_proj.gameObject);
				_proj.SetDirection(knockbackDirection);
				_proj.lifetimeTimer = 0f;
				_proj.owner = owner; // steal ownership of the projectile
				_proj.transform.SetParent(owner == null ? null : owner.transform, true);
				_proj.hitObjects.Clear();
				break;
		}

		hitObjects.Add(_proj.gameObject);
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
		if (owner == null || owner.hitpause <= 0)
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

			if (hitboxDelayEnd > 0 && lifetimeTimer >= hitboxDelayEnd)
				hitboxCollider.enabled = false;
			else if (lifetimeTimer >= hitboxDelay)
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

	public bool CanHitPressurePlates()
	{
		return canHitPressurePlates;
	}
}