using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Kitsu Attack", menuName = "Attack/Enemy Attacks/Kitsu")]
public class KitsuAttack : EnemyAttackGrid
{
	[Header("Prefabs")]
	[SerializeField] SpriteRenderer warningPrefab;
	[SerializeField] Hitbox meleeHitboxPrefab;
	[Header("Fade Timings")]
	[SerializeField, Min(0)] float warningFadeRate = 2f;
	[SerializeField, Min(0)] float hitboxFadeRate = 5f;
	[Header("Hitbox Spawn Location")]
	[SerializeField] float hitboxDistance = 2f;
	[Header("CPU Properties")]
	[SerializeField] float attackDistance = 2.5f;
	[SerializeField, Min(0)] float chargeTime = 1f;

	// EVERY enemy of the same species share the same script and variables. we need to handle this.
	Dictionary<Enemy, UniqueVariables> varsDict = new Dictionary<Enemy, UniqueVariables>();
	struct UniqueVariables
	{
		public bool hasAttacked;
		public Vector2 direction;
		public SpriteRenderer warning;
		public Hitbox hbox;
		public Quaternion directionQuaternion;
	}

	// runs when starting an attack
	public override void AttackStart(Enemy self)
	{
		UniqueVariables vars = new UniqueVariables();

		vars.hasAttacked = false;

		if (self.IsBeingControlledByPlayer())
			vars.direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - self.transform.position);
		else
			vars.direction = (self.target.transform.position - self.transform.position);
		vars.direction.Normalize();
		self.animations.SetFlipX(vars.direction);
		vars.directionQuaternion = Quaternion.Euler(0f, 0f, Mathf.Atan2(vars.direction.y, vars.direction.x) * Mathf.Rad2Deg);

		vars.warning = Instantiate(warningPrefab, (Vector2)self.transform.position + vars.direction * hitboxDistance, vars.directionQuaternion, self.transform);

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
			case 0:
				if (vars.warning != null)
					vars.warning.color = new Color(vars.warning.color.r, vars.warning.color.g, vars.warning.color.b, vars.warning.color.a - (warningFadeRate * Time.deltaTime));
				break;
			case 1:
			case 2:
				if (!vars.hasAttacked)
				{
					Destroy(vars.warning.gameObject);
					vars.warning = null;

					vars.hbox = Instantiate(meleeHitboxPrefab, (Vector2)self.transform.position + vars.direction * hitboxDistance, vars.directionQuaternion, self.transform);
					vars.hbox.SetDirection(vars.direction);
					vars.hbox.owner = self;
					vars.hasAttacked = true;
				}
				if (vars.hbox != null)
				{
					Color color = vars.hbox.hitboxSprite.color;
					vars.hbox.hitboxSprite.color = new Color(color.r, color.g, color.b, color.a - (hitboxFadeRate * Time.deltaTime));
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

		if (vars.hbox != null)
		{
			vars.hbox.Destroy();
			vars.hbox = null;
		}

		if (vars.warning != null)
		{
			Destroy(vars.warning.gameObject);
			vars.warning = null;
		}

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