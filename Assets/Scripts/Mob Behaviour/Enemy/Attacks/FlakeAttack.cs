using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Flake Attack", menuName = "Attack/Enemy Attacks/Flake")]
public class FlakeAttack : EnemyAttackGrid
{
	[Header("Prefabs")]
	[SerializeField] Projectile projectilePrefab;
	[Header("Hitbox Properties")]
	[SerializeField] float hitboxDistance = 0.2f;
	[SerializeField, Min(0)] float minChargeTime = 0;
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
	}

	// runs when starting an attack
	public override void AttackStart(Enemy self)
	{
		UniqueVariables vars = new UniqueVariables();

		vars.hasAttacked = false;
		vars.chargeTimer = 0;

		Vector2 direction;
		if (self.IsBeingControlledByPlayer())
			direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - self.transform.position);
		else
			direction = (self.target.transform.position - self.transform.position);
		direction.Normalize();
		self.animations.SetFlipX(direction);

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

				if (vars.chargeTimer >= minChargeTime && (self.IsBeingControlledByPlayer() ? !Input.GetMouseButton(0) : vars.chargeTimer > cpuChargeTime))
				{
					self.attack.SetWindow(2);

					if (self.IsBeingControlledByPlayer())
						vars.direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - self.transform.position);
					else
						vars.direction = (self.target.transform.position - self.transform.position);
					vars.direction.Normalize();
					self.animations.SetFlipX(vars.direction);
				}
				break;
			case 3:
				if (!vars.hasAttacked)
				{
					Quaternion directionQuaternion = Quaternion.Euler(0f, 0f, Mathf.Atan2(vars.direction.y, vars.direction.x) * Mathf.Rad2Deg);

					Projectile hbox = Instantiate(projectilePrefab, (Vector2)self.transform.position + vars.direction * hitboxDistance, directionQuaternion, self.transform);
					hbox.SetDirection(vars.direction);
					Physics2D.IgnoreCollision(hbox.GetComponent<Collider2D>(), self.enemyCollider);
					hbox.owner = self;
					vars.hasAttacked = true;
				}
				break;
		}

		varsDict[self] = vars;
	}

	// runs when an attack ends or gets interrupted
	public override void AttackEnd(Enemy self)
	{
		if (!varsDict.ContainsKey(self)) return;
		varsDict.Remove(self);
	}

	// when should the cpu begin its attack?
	public override bool ShouldAttack(Enemy self)
	{
		if (self.state == Enemy.EnemyState.chase && (self.target.transform.position - self.transform.position).magnitude <= chaseDist && self.pathfinding.GetWaypoints() != null && self.pathfinding.GetWaypoints().Count == 0)
			return true;

		return false;
	}
}