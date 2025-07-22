using UnityEngine;

[CreateAssetMenu(fileName = "Kitsu Attack", menuName = "Attack/Enemy Attacks/Kitsu")]
public class KitsuAttack : EnemyAttackGrid
{
	[SerializeField] Hitbox meleeHitbox;
	[SerializeField] float hitboxDistance = 1f;
	bool hasAttacked = false;
	Vector2 direction;

	// runs when starting an attack
	public override void AttackStart(Enemy self)
	{
		hasAttacked = false;
		direction = self.animations.GetFlipX() ? Vector2.left : Vector2.right;
	}

	// runs every frame of the attack
	public override void AttackUpdate(Enemy self, int window, float windowTimer, float chargeTimer)
	{
		if (window == 1 && !hasAttacked)
		{
			Hitbox hbox = Instantiate(meleeHitbox, (Vector2)self.transform.position + direction * hitboxDistance, Quaternion.identity, self.transform);
			hbox.SetDirection(direction);
			hasAttacked = true;
		}
	}

	// runs when an attack ends
	public override void AttackEnd(Enemy self) { }
}