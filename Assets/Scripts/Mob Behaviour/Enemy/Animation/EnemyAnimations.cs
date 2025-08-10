using UnityEngine;

public class EnemyAnimations : MobAnimation
{
	public enum EnemyAnimState
	{
		idle,
		run,
		spare,
		hurt,
		die,
		custom
	}

	[SerializeField] EnemyAnimationSet anims;

	EnemyAnimState state = EnemyAnimState.idle;
	Enemy enemy;

	protected override void Awake()
	{
		UpdateSpriteIndex();
		enemy = gameObject.GetComponent<Enemy>();
		base.Awake();
	}
	protected override void Update()
	{
		base.Update();

		if (state == EnemyAnimState.die) // fade out
		{
			Color color = spriteRenderer.color;
			spriteRenderer.color = new Color(color.r, color.g, color.b, color.a - (anims.dieFadeSpeed * Time.deltaTime));
			if (spriteRenderer.color.a <= 0)
				enemy.Die();
		}
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
			case EnemyAnimState.die:
				UpdateSpriteIndex(anims.hurt, _animSpeed: anims.hurtSpeed);
				break;
			case EnemyAnimState.custom:
				break;
		}
	}

	public void SetAnimations(EnemyAnimationSet newAnims)
	{
		anims = newAnims;

		UpdateSpriteIndex();
		shadowRenderer.transform.localScale = new Vector3(anims.shadow.x, anims.shadow.y, 1);
	}

	public EnemyAnimationSet GetAnimations()
	{
		return anims;
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