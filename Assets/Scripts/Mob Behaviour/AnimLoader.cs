using UnityEngine;

public class AnimLoader : MonoBehaviour
{
	[SerializeField] SpriteRenderer spriteRenderer;

	protected Sprite[] spriteIndex;
	protected int imageIndex = 0;
	protected float animSpeed = 0;
	float animationTimer = 0;

	Rigidbody2D rb;

	protected virtual void Awake()
	{
		SetCurrentSprite();
		rb = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		UpdateImageIndex();
	}

	void UpdateImageIndex()
	{
		if (spriteIndex != null && spriteIndex.Length > 0 && animSpeed > 0)
		{
			animationTimer += Time.deltaTime;
			if (animationTimer > animSpeed)
			{
				animationTimer -= animSpeed;
				imageIndex = (imageIndex + 1) % spriteIndex.Length;
				SetCurrentSprite();
			}
		}
	}

	protected void UpdateSpriteIndex(Sprite[] _spriteIndex, float _animSpeed)
	{
		spriteIndex = _spriteIndex;
		animSpeed = _animSpeed;
	}

	protected void SetCurrentSprite()
	{
		if (spriteRenderer != null && spriteIndex != null && spriteIndex.Length > imageIndex)
			spriteRenderer.sprite = spriteIndex[imageIndex];
	}

	public void ChangeState(Sprite[] _spriteIndex, float _animSpeed)
	{
		animationTimer = 0;
		imageIndex = 0;
		UpdateSpriteIndex(_spriteIndex, _animSpeed);
		SetCurrentSprite();
	}

	public void SetFlipX(Vector2 velocity)
	{
		if (rb != null && Mathf.Abs(velocity.x) > 0f)
			spriteRenderer.flipX = velocity.x < 0;
	}
}