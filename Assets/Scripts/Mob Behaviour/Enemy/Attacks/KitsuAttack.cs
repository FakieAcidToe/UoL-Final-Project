using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[CreateAssetMenu(fileName = "Kitsu Attack", menuName = "Attack/Enemy Attacks/Kitsu")]
public class KitsuAttack : EnemyAttackGrid
{
	[SerializeField] SpriteRenderer warningPrefab;
	[SerializeField] Hitbox meleeHitboxPrefab;
	[SerializeField, Min(0)] float warningFadeRate = 2f;
	[SerializeField, Min(0)] float hitboxFadeRate = 5f;
	[SerializeField] float hitboxDistance = 2f;

	bool hasAttacked = false;
	Vector2 direction;
	SpriteRenderer warning;
	Hitbox hbox;

	Quaternion directionQuaternion;

	// runs when starting an attack
	public override void AttackStart(Enemy self)
	{
		hasAttacked = false;
		hbox = null;
		direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - self.transform.position).normalized;
		self.animations.SetFlipX(direction);

		directionQuaternion = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

		warning = Instantiate(warningPrefab, (Vector2)self.transform.position + direction * hitboxDistance, directionQuaternion, self.transform);
	}

	// runs every frame of the attack
	public override void AttackUpdate(Enemy self, int window, float windowTimer, float chargeTimer)
	{
		switch (window)
		{
			case 0:
				if (warning != null)
					warning.color = new Color(warning.color.r, warning.color.g, warning.color.b, warning.color.a - (warningFadeRate * Time.deltaTime));
				break;
			case 1:
			case 2:
				if (!hasAttacked)
				{
					Destroy(warning.gameObject);
					warning = null;

					hbox = Instantiate(meleeHitboxPrefab, (Vector2)self.transform.position + direction * hitboxDistance, directionQuaternion, self.transform);
					hbox.SetDirection(direction);
					hbox.owner = self;
					hasAttacked = true;
				}
				if (hbox != null)
				{
					Color color = hbox.hitboxSprite.color;
					hbox.hitboxSprite.color = new Color(color.r, color.g, color.b, color.a - (hitboxFadeRate * Time.deltaTime));
				}
				break;
		}
	}

	// runs when an attack ends or gets interrupted
	public override void AttackEnd(Enemy self)
	{
		if (hbox != null)
		{
			hbox.Destroy();
			hbox = null;
		}

		if (warning != null)
		{
			Destroy(warning.gameObject);
			warning = null;
		}
	}
}