using UnityEngine;

public class PlayerAnimations : MobAnimation
{
	public enum PlayerAnimState
	{
		idle,
		run
	}

	[SerializeField] PlayerAnimationSet anims;

	PlayerAnimState state = PlayerAnimState.idle;

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
			case PlayerAnimState.idle:
				UpdateSpriteIndex(anims.idle, _animSpeed: anims.idleSpeed);
				break;
			case PlayerAnimState.run:
				UpdateSpriteIndex(anims.run, _animSpeed: anims.runSpeed);
				break;
		}
	}

	public void ChangeState(PlayerAnimState newState)
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
}