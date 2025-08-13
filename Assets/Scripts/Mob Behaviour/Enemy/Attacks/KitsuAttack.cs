using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Kitsu Attack", menuName = "Attack/Enemy Attacks/Kitsu")]
public class KitsuAttack : EnemyAttackGrid
{
	[Header("Prefabs")]
	[SerializeField] SpriteRenderer warningPrefab;
	[SerializeField] Hitbox meleeHitboxPrefab;
	[Header("Fade Timings")]
	[SerializeField, Min(0)] float warningFadeRate = 2f;
	[SerializeField, Min(0)] float hitboxFadeRate = 5f;
	[Header("Audio")]
	[SerializeField] AudioClip warningSfx;
	[SerializeField] AudioClip attackSfx;
	[Header("Hitbox Spawn Location")]
	[SerializeField, Min(0)] int numHitboxes = 3;
	[SerializeField] float hitboxDistance = 2f;
	[SerializeField, Min(0)] float hitboxSpread = 0.5f;
	[Header("CPU Properties")]
	[SerializeField] float attackDistance = 2.5f;
	[SerializeField, Min(0)] float chargeTime = 1f;

	// EVERY enemy of the same species share the same script and variables. we need to handle this.
	Dictionary<Enemy, UniqueVariables> varsDict = new Dictionary<Enemy, UniqueVariables>();
	struct UniqueVariables
	{
		public bool hasAttacked;
		public Vector2 direction;
		public List<SpriteRenderer> warnings;
		public List<Hitbox> hboxs;
	}

	// runs when starting an attack
	public override void AttackStart(Enemy self)
	{
		UniqueVariables vars = new UniqueVariables();

		vars.hasAttacked = false;

		// selects attack direction
		if (self.IsBeingControlledByPlayer())
			vars.direction = (Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - self.transform.position); // direction towards mouse position
		else
			vars.direction = (self.target.transform.position - self.transform.position); // cpu targets player position
		vars.direction.Normalize();
		self.animations.SetFlipX(vars.direction);

		vars.hboxs = new List<Hitbox>();
		vars.warnings = new List<SpriteRenderer>();
		// spawn warnings in pattern
		for (int i = 0; i < numHitboxes; ++i)
		{
			vars.warnings.Add(
				Instantiate(
					warningPrefab,
					(Vector2)self.transform.position + vars.direction * hitboxDistance + ((i - numHitboxes / 2f + 0.5f) * hitboxSpread * new Vector2(vars.direction.y, -vars.direction.x)),
					Quaternion.Euler(0f, 0f, Mathf.Atan2(vars.direction.y, vars.direction.x) * Mathf.Rad2Deg),
					self.transform
				)
			);
		}

		self.PlaySFX(warningSfx);

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
			case 0: // startup
				// fade warning opacity
				foreach (SpriteRenderer warning in vars.warnings)
					warning.color = new Color(warning.color.r, warning.color.g, warning.color.b, warning.color.a - (warningFadeRate * Time.deltaTime));
				break;
			case 1: // swipe
			case 2: // endlag
				if (!vars.hasAttacked)
				{
					// spawn hitboxes at warning locations
					foreach (SpriteRenderer warning in vars.warnings)
					{
						Hitbox hbox = Instantiate(meleeHitboxPrefab, warning.transform.position, warning.transform.rotation, self.transform);
						hbox.SetDirection(vars.direction);
						hbox.owner = self;
						vars.hboxs.Add(hbox);

						Destroy(warning.gameObject);
					}

					self.PlaySFX(attackSfx);

					vars.warnings.Clear();
					vars.hasAttacked = true;
				}
				// fade hitbox opacity
				foreach (Hitbox hbox in vars.hboxs)
					if (hbox != null)
					{
						Color color = hbox.hitboxSprite.color;
						hbox.hitboxSprite.color = new Color(color.r, color.g, color.b, color.a - (hitboxFadeRate * Time.deltaTime));
					}
				break;
		}

		varsDict[self] = vars;
	}

	// runs when an attack ends or gets interrupted
	public override void AttackEnd(Enemy self)
	{
		if (!varsDict.ContainsKey(self)) return;
		UniqueVariables vars = varsDict[self];

		foreach (Hitbox hbox in vars.hboxs)
			if (hbox != null)
				Destroy(hbox.gameObject);
		vars.hboxs.Clear();

		foreach (SpriteRenderer warning in vars.warnings)
			if (warning != null)
				Destroy(warning.gameObject);
		vars.warnings.Clear();

		varsDict.Remove(self);
	}

	// when should the cpu begin its attack?
	public override bool ShouldAttack(Enemy self)
	{
		if (self.state == Enemy.EnemyState.chase && (self.target.transform.position - self.transform.position).magnitude <= attackDistance)
			return true;

		return false;
	}

	// when should the cpu hold a charge during an attack?
	public override bool ShouldCPUChargeAttack(Enemy self, int window, float windowTimer, float chargeTimer)
	{
		return chargeTimer < chargeTime;
	}
}