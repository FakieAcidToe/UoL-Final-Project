using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chess Attack", menuName = "Attack/Enemy Attacks/Chesspiece")]
public class ChessAttack : EnemyAttackGrid
{
	[Header("Prefabs")]
	[SerializeField] Hitbox hitboxPrefab;
	[Header("Movement")]
	[SerializeField, Min(0)] float dashSpeed = 6f;
	[SerializeField, Min(0)] float cpuDashSpeed = 3f;
	[SerializeField, Min(0)] float dashTime = 1f;
	[Header("CPU Properties")]
	[SerializeField, Min(0)] float attackDistance = 4f;
	[SerializeField, Min(0)] float unchargeDistance = 0.8f;
	[SerializeField, Min(0)] float chargeMinTime = 0.3f;
	[SerializeField, Min(0)] float chargeMaxTime = 1.2f;

	// EVERY enemy of the same species share the same script and variables. we need to handle this.
	Dictionary<Enemy, UniqueVariables> varsDict = new Dictionary<Enemy, UniqueVariables>();
	struct UniqueVariables
	{
		public bool hasAttacked;
		public Vector2 direction;
		public Hitbox hbox;
	}

	// runs when starting an attack
	public override void AttackStart(Enemy self)
	{
		UniqueVariables vars = new UniqueVariables();

		vars.hasAttacked = false;

		// selects attack direction
		if (self.IsBeingControlledByPlayer())
			vars.direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - self.transform.position); // direction towards mouse position
		else
			vars.direction = (self.target.transform.position - self.transform.position); // cpu targets player position
		vars.direction.Normalize();
		self.animations.SetFlipX(vars.direction);

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
			case 1: // jump
			case 2: // jump hold
				if (chargeTimer < dashTime)
					self.SetMovement(vars.direction * (self.IsBeingControlledByPlayer() ? dashSpeed : cpuDashSpeed));
				break;
			case 4: // hit + endlag
				if (!vars.hasAttacked)
				{
					// spawn hitbox
					Hitbox hbox = Instantiate(hitboxPrefab, self.transform.position, Quaternion.identity, self.transform);
					hbox.SetDirection(vars.direction);
					hbox.owner = self;
					vars.hbox = hbox;

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
		UniqueVariables vars = varsDict[self];

		if (vars.hbox != null)
			Destroy(vars.hbox.gameObject);
		vars.hbox = null;

		varsDict.Remove(self);
	}

	// when should the cpu begin its attack?
	public override bool ShouldAttack(Enemy self)
	{
		if (self.state == Enemy.EnemyState.chase &&
			(self.target.transform.position - self.transform.position).magnitude <= attackDistance &&
			(self.pathfinding.GetWaypoints() == null || self.pathfinding.GetWaypoints().Count <= 1))
			return true;

		return false;
	}

	// when should the cpu hold a charge during an attack?
	public override bool ShouldCPUChargeAttack(Enemy self, int window, float windowTimer, float chargeTimer)
	{
		return chargeTimer < chargeMaxTime && ((self.target.transform.position - self.transform.position).magnitude > unchargeDistance || chargeTimer < chargeMinTime);
	}
}