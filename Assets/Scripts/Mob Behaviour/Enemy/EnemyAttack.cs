using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyAttack : MonoBehaviour
{
	[SerializeField] EnemyAttackGrid attackGrid;
	[SerializeField] float chargeBlinkSpeed = 0.1f;

	int window = 0;
	float windowTimer = 0;
	int imageIndex = 0;

	float chargeTimer = 0;

	Enemy enemy;

	void Awake()
	{
		enemy = GetComponent<Enemy>();
	}

	public void AttackStart()
	{
		InitAttackAnimation();

		// run attack script, if any
		attackGrid.AttackStart(enemy);
	}

	public bool AttackUpdate()
	{
		AttackAnimation();
		if (window >= attackGrid.windows.Length) // true if attack is done
		{
			attackGrid.AttackEnd(enemy);
			return true;
		}

		// run attack script, if any
		attackGrid.AttackUpdate(enemy, window, windowTimer, chargeTimer);
		return false;
	}

	public void AttackInterrupt()
	{
		attackGrid.AttackEnd(enemy);
	}

	void InitAttackAnimation()
	{
		window = 0;
		windowTimer = 0;
		imageIndex = 0;

		if (attackGrid.windows.Length > 0 && attackGrid.windows[0].sprites.Length > 0 && attackGrid.windows[0].sprites != null)
			enemy.animations.UpdateSpriteIndex(attackGrid.windows[0].sprites);
	}

	void AttackAnimation() // true if attack is done
	{
		int numWindows = attackGrid.windows.Length;
		if (window >= numWindows) return;

		int numSprites = Mathf.Max(attackGrid.windows[window].sprites.Length, 1);
		float windowLength = attackGrid.windows[window].windowLength;

		bool shouldUpdateSprite = false;

		float dt = Time.deltaTime;
		windowTimer += dt;

		if (windowTimer >= windowLength / numSprites * (imageIndex + 1)) // handle sprites in window
		{
			++imageIndex;
			shouldUpdateSprite = true;
		}

		if (windowTimer >= windowLength) // attack window finished
		{
			// charge on last sprite in window
			if (attackGrid.windows[window].chargeable && Input.GetMouseButton(0))
			{
				chargeTimer += dt;
				windowTimer = windowLength;

				// yellow charge
				enemy.animations.SetColour(chargeTimer % (chargeBlinkSpeed * 2) < chargeBlinkSpeed ? Color.yellow : Color.white);
			}
			else // advance window
			{
				++window;
				windowTimer = 0;
				imageIndex = 0;
				shouldUpdateSprite = true;
			}
		}

		if (shouldUpdateSprite)
		{
			enemy.animations.SetColour(Color.white);
			if (window < numWindows)
				enemy.animations.UpdateSpriteIndex(attackGrid.windows[window].sprites, imageIndex);
		}
	}

	public void SetAttackGrid(EnemyAttackGrid newAttack)
	{
		attackGrid = newAttack;
	}
}