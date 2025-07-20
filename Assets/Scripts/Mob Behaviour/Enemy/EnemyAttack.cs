using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyAttack : MonoBehaviour
{
	[SerializeField] EnemyAttackGrid attackGrid;
	[SerializeField] float chargeBlinkSpeed = 0.1f;

	int window = 0;
	int imageIndex = 0;
	float attackTimer = 0;

	float chargeTimer = 0;

	Enemy enemy;

	void Awake()
	{
		enemy = GetComponent<Enemy>();
	}

	public bool AttackUpdate()
	{
		int numWindows = attackGrid.windows.Length;
		int numSprites = attackGrid.windows[window].sprites.Length;
		if (window < numWindows && imageIndex < numSprites)
		{
			float dt = Time.deltaTime;
			attackTimer += dt;

			float animSpeed = attackGrid.windows[window].animSpeed;
			if (attackTimer >= animSpeed)
			{
				// charge on last sprite in window
				if (imageIndex + 1 >= numSprites && attackGrid.windows[window].chargeable && Input.GetMouseButton(0))
				{
					chargeTimer += dt;
					attackTimer = animSpeed;

					// yellow charge
					enemy.animations.SetColour(chargeTimer % (chargeBlinkSpeed * 2) < chargeBlinkSpeed ? Color.yellow : Color.white);
				}
				else // next attack sprite
				{
					attackTimer -= animSpeed;
					if (++imageIndex >= numSprites) // attack window finished
					{
						if (++window >= numWindows) return true; // attack finished
						imageIndex = 0;
					}
					enemy.animations.SetColour(Color.white);
					enemy.animations.UpdateSpriteIndex(attackGrid.windows[window].sprites, imageIndex);
				}
			}
			return false;
		}
		return true;
	}

	public void SetAttackGrid(EnemyAttackGrid newAttack)
	{
		attackGrid = newAttack;
	}

	public void InitAttackAnimation()
	{
		window = 0;
		attackTimer = 0;
		imageIndex = 0;

		if (attackGrid.windows.Length > 0 && attackGrid.windows[0].sprites.Length > 0 && attackGrid.windows[0].sprites != null)
			enemy.animations.UpdateSpriteIndex(attackGrid.windows[0].sprites);
	}
}