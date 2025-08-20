using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bomb Attack", menuName = "Attack/Enemy Attacks/Bomb")]
public class BombAttack : EnemyAttackGrid
{
	[Header("Prefabs")]
	[SerializeField] Hitbox explosPrefab;
	[Header("Audio")]
	[SerializeField] AudioClip explosSfx;
	[Header("Attack Properties")]
	[SerializeField, Min(0)] float minChargeTime = 0.3f;
	[SerializeField, Min(0)] int selfDamage = 2;
	[Header("CPU Properties")]
	[SerializeField] float attackDistance = 2.5f;
	[SerializeField, Min(0)] float cpuChargeTime = 0.8f;

	// EVERY enemy of the same species share the same script and variables. we need to handle this.
	Dictionary<Enemy, UniqueVariables> varsDict = new Dictionary<Enemy, UniqueVariables>();
	struct UniqueVariables
	{
		public bool hasAttacked;
		public float chargeTimer;
	}

	// runs when starting an attack
	public override void AttackStart(Enemy self)
	{
		UniqueVariables vars = new UniqueVariables();

		vars.hasAttacked = false;
		vars.chargeTimer = 0;

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
			case 0: // charge
				vars.chargeTimer += Time.deltaTime;

				// change window out of the loop
				if (vars.chargeTimer >= minChargeTime && (self.IsBeingControlledByPlayer() ? !self.controls.Gameplay.Attack.IsPressed() : vars.chargeTimer > cpuChargeTime))
					self.attack.SetWindow(1);
				break;
			case 1: // explos
				if (!vars.hasAttacked)
				{
					// spawn hitbox
					Hitbox hbox = Instantiate(explosPrefab, self.transform.position, Quaternion.identity);
					hbox.owner = self;

					if (self.IsBeingControlledByPlayer())
						self.TakeDamage(selfDamage);
		
					self.PlaySFX(explosSfx);

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
		//UniqueVariables vars = varsDict[self];

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
		return false;
	}
}