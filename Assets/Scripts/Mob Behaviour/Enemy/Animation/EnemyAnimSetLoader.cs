using UnityEngine;

public class EnemyAnimSetLoader : AnimLoader
{
	public enum EnemyAnimState
	{
		idle
	}

	[SerializeField] EnemyAnimationSet anims;

	EnemyAnimState state = EnemyAnimState.idle;

	protected override void Awake()
	{
		UpdateSpriteIndex();
		SetCurrentSprite();
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
		}
	}

	public void ChangeState(EnemyAnimState newState)
	{
		state = newState;
		UpdateSpriteIndex();
		ChangeState(spriteIndex, animSpeed);
	}
}