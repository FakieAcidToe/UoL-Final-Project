using UnityEngine;

[CreateAssetMenu(fileName = "Kitsu Attack", menuName = "Attack/Enemy Attacks/Kitsu")]
public class KitsuAttack : EnemyAttackGrid
{
	// runs when starting an attack
	public override void AttackStart(Enemy self) { }

	// runs every frame of the attack
	public override void AttackUpdate(Enemy self, int window, float windowTimer, float chargeTimer) { }
}