using UnityEngine;

public class EnemyAnimations : MobAnimation
{
	public enum EnemyAnimState
	{
		idle,
		run,
		spare,
		hurt,
		custom
	}

	[SerializeField] EnemyAnimationSet anims;

	EnemyAnimState state = EnemyAnimState.idle;

	protected override void Awake()
	{
		UpdateSpriteIndex();
		base.Awake();
	}

	void UpdateSpriteIndex()
	{
		if (anims == null) return;

		SetColour(Color.white);

		switch (state)
		{
			default:
			case EnemyAnimState.idle:
				UpdateSpriteIndex(anims.idle, _animSpeed: anims.idleSpeed);
				break;
			case EnemyAnimState.run:
				UpdateSpriteIndex(anims.run, _animSpeed: anims.runSpeed);
				break;
			case EnemyAnimState.spare:
				UpdateSpriteIndex(anims.sparable, _animSpeed: anims.sparableSpeed);
				break;
			case EnemyAnimState.hurt:
				UpdateSpriteIndex(anims.hurt, _animSpeed: anims.hurtSpeed);
				break;
			case EnemyAnimState.custom:
				break;
		}
	}

	public void SetAnimations(EnemyAnimationSet newAnims)
	{
		anims = newAnims;
	}

	public void ChangeState(EnemyAnimState newState)
	{
		state = newState;
		UpdateSpriteIndex();
	}

	public new void SetFlipX(Vector2 velocity)
	{
		if (anims.isFacingRight)
			base.SetFlipX(velocity);
		else
			base.SetFlipX(velocity*Vector2.left);
	}

	public new bool GetFlipX()
	{
		return anims.isFacingRight ? base.GetFlipX() : !base.GetFlipX();
	}
}