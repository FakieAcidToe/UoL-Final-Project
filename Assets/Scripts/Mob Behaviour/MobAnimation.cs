using UnityEngine;

public class MobAnimation : MonoBehaviour
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

	public void UpdateSpriteIndex(Sprite[] _spriteIndex, int _imageIndex = 0, float _animSpeed = 0)
	{
		spriteIndex = _spriteIndex;
		imageIndex = _imageIndex;
		animSpeed = _animSpeed;
		animationTimer = 0;
		SetCurrentSprite();
	}

	protected void SetCurrentSprite()
	{
		if (spriteRenderer != null && spriteIndex != null && spriteIndex.Length > imageIndex)
			spriteRenderer.sprite = spriteIndex[imageIndex];
	}

	public void SetFlipX(Vector2 velocity)
	{
		if (rb != null && Mathf.Abs(velocity.x) > 0f)
			spriteRenderer.flipX = velocity.x < 0;
	}

	public void SetColour(Color _colour)
	{
		spriteRenderer.color = _colour;
	}
}