using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Flake Attack", menuName = "Attack/Enemy Attacks/Flake")]
public class FlakeAttack : EnemyAttackGrid
{
	[Header("Prefabs")]
	[SerializeField] Projectile projectilePrefab;
	[SerializeField] SpriteRenderer icicleSpritePrefab;
	[Header("Hitbox Properties")]
	[SerializeField] float hitboxDistance = 0.2f;
	[SerializeField, Min(0)] float minChargeTime = 0;
	[SerializeField, Min(0)] float chargeTimePer = 0.5f;
	[SerializeField, Min(0)] int chargeMax = 5;
	[SerializeField, Min(0)] float spinRadius = 0.65f;
	[SerializeField] float spinSpeed = 5f;
	[SerializeField] Vector2 iciclePositionOffset = new Vector2(-1, 0);
	[SerializeField, Min(0)] float icicleFadeInSpeed = 4f;
	[SerializeField] float icicleRotationOffset = 30f;
	[SerializeField] float icicleSpreadAngle = 10f;
	[Header("Audio")]
	[SerializeField] AudioClip chargeSfx;
	[SerializeField] AudioClip attackSfx;
	[Header("CPU Properties")]
	[SerializeField, Min(0)] float chaseDist = 10f;
	[SerializeField, Min(0)] float cpuChargeTime = 0.3f;

	// EVERY enemy of the same species share the same script and variables. we need to handle this.
	Dictionary<Enemy, UniqueVariables> varsDict = new Dictionary<Enemy, UniqueVariables>();
	struct UniqueVariables
	{
		public bool hasAttacked;
		public Vector2 direction;
		public float chargeTimer;
		public List<SpriteRenderer> icicleSprites;
	}

	// runs when starting an attack
	public override void AttackStart(Enemy self)
	{
		UniqueVariables vars = new UniqueVariables();

		vars.hasAttacked = false;
		vars.chargeTimer = 0;

		Vector2 direction;
		if (self.IsBeingControlledByPlayer())
			direction = (Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - self.transform.position);
		else
			direction = (self.target.transform.position - self.transform.position);
		direction.Normalize();
		self.animations.SetFlipX(direction);

		// spawn icicle indicator
		vars.icicleSprites = new List<SpriteRenderer>
		{
			SpawnSprite(self, vars)
		};
		self.PlaySFX(chargeSfx);

		if (varsDict.ContainsKey(self)) varsDict[self] = vars;
		else varsDict.Add(self, vars);
	}

	// runs every frame of the attack
	public override void AttackUpdate(Enemy self, int window, float windowTimer, float chargeTimer)
	{
		if (!varsDict.ContainsKey(self)) return;
		UniqueVariables vars = varsDict[self];

		switch (window)
		{
			case 1:
				vars.chargeTimer += Time.deltaTime;

				if (vars.chargeTimer >= minChargeTime && (self.IsBeingControlledByPlayer() ? !self.controls.Gameplay.Attack.IsPressed() : vars.chargeTimer > cpuChargeTime))
				{
					self.attack.SetWindow(2);
					for (int i = vars.icicleSprites.Count - 1; i >= 0; --i)
						Destroy(vars.icicleSprites[i].gameObject);

						if (self.IsBeingControlledByPlayer())
						vars.direction = (Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - self.transform.position);
					else
						vars.direction = (self.target.transform.position - self.transform.position);
					vars.direction.Normalize();
					self.animations.SetFlipX(vars.direction);
					break;
				}
				else if (vars.icicleSprites.Count < chargeMax && vars.chargeTimer >= chargeTimePer * vars.icicleSprites.Count)
				{
					// spawn icicle indicator
					vars.icicleSprites.Add(SpawnSprite(self, vars));
					self.PlaySFX(chargeSfx);
				}

				for (int i = 0; i < vars.icicleSprites.Count; ++i)
				{
					SpriteRenderer icicle = vars.icicleSprites[i];

					// update icicle positions
					SpinIcicleSprite(icicle, i, vars.chargeTimer, self.animations.GetFlipX() ? -1 : 1);

					// also update alpha
					icicle.color = new Color(icicle.color.r, icicle.color.g, icicle.color.b, icicle.color.a + icicleFadeInSpeed * Time.deltaTime);
				}
				break;
			case 3:
				if (!vars.hasAttacked)
				{
					for (int i = vars.icicleSprites.Count - 1; i >= 0; --i)
					{
						Vector2 dir = RotateVector(vars.direction, icicleSpreadAngle * (i - vars.icicleSprites.Count / 2f + 0.5f));

						Projectile hbox = Instantiate(
							projectilePrefab,
							(Vector2)self.transform.position + dir * hitboxDistance,
							Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg),
							self.transform);
						hbox.SetDirection(dir);
						Physics2D.IgnoreCollision(hbox.GetComponent<Collider2D>(), self.enemyCollider);
						hbox.owner = self;
					}
					vars.icicleSprites.Clear();
					self.PlaySFX(attackSfx);
					vars.hasAttacked = true;
				}
				break;
		}

		varsDict[self] = vars;
	}

	SpriteRenderer SpawnSprite(Enemy self, UniqueVariables vars)
	{
		SpriteRenderer icicle = Instantiate(icicleSpritePrefab,
			(Vector2)self.transform.position,
			Quaternion.identity,
			self.transform);
		icicle.color = new Color(icicle.color.r, icicle.color.g, icicle.color.b, 0);
		return icicle;
	}

	void SpinIcicleSprite(SpriteRenderer icicle, int index, float timer, int facingDirection)
	{
		// evenly spaced angle around the circle
		float angle = index * Mathf.PI * 2f / chargeMax + timer * spinSpeed * facingDirection;

		// set position and rotation
		icicle.transform.localPosition = iciclePositionOffset * facingDirection + new Vector2(Mathf.Cos(angle) * spinRadius, Mathf.Sin(angle) * spinRadius);
		icicle.transform.localRotation = Quaternion.Euler(0f, 0f, (angle + icicleRotationOffset * facingDirection) * Mathf.Rad2Deg);
	}

	Vector2 RotateVector(Vector2 dir, float angleDegrees)
	{
		float angleRad = angleDegrees * Mathf.Deg2Rad;
		float cos = Mathf.Cos(angleRad);
		float sin = Mathf.Sin(angleRad);

		return new Vector2(
			dir.x * cos - dir.y * sin,
			dir.x * sin + dir.y * cos
		);
	}

	// runs when an attack ends or gets interrupted
	public override void AttackEnd(Enemy self)
	{
		if (!varsDict.ContainsKey(self)) return;
		UniqueVariables vars = varsDict[self];

		for (int i = vars.icicleSprites.Count - 1; i >= 0; --i)
			if (vars.icicleSprites[i] != null)
				Destroy(vars.icicleSprites[i].gameObject);
		vars.icicleSprites.Clear();

		varsDict.Remove(self);
	}

	// when should the cpu begin its attack?
	public override bool ShouldAttack(Enemy self)
	{
		if (self.state == Enemy.EnemyState.chase &&
			(self.target.transform.position - self.transform.position).magnitude <= chaseDist &&
			(self.pathfinding.GetWaypoints() == null || self.pathfinding.GetWaypoints().Count <= 1))
			return true;

		return false;
	}
}