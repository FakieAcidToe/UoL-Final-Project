using UnityEngine;

public class EnemyAnimSetLoader : AnimLoader
{
	public enum EnemyAnimState
	{
		idle,
		run,
		spare
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

		switch (state)
		{
			default:
			case EnemyAnimState.idle:
				UpdateSpriteIndex(anims.idle, anims.idleSpeed);
				break;
			case EnemyAnimState.run:
				UpdateSpriteIndex(anims.run, anims.runSpeed);
				break;
			case EnemyAnimState.spare:
				UpdateSpriteIndex(anims.sparable, anims.sparableSpeed);
				break;
		}
	}

	public void ChangeState(EnemyAnimState newState)
	{
		state = newState;
		UpdateSpriteIndex();
		ChangeState(spriteIndex, animSpeed);
	}

	public new void SetFlipX(Vector2 velocity)
	{
		if (anims.isFacingRight)
			base.SetFlipX(velocity);
		else
			base.SetFlipX(velocity*Vector2.left);
	}
}