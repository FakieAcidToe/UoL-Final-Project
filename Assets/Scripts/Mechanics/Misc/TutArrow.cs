using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TutArrow : MonoBehaviour
{
	[SerializeField, Min(0)] float lerpSpeed = 8f;
	[SerializeField] float bounceLength = 0.5f;
	[SerializeField, Min(0)] float bounceSpeed = 2f;
	Vector2 position = Vector2.zero;
	float currentAngle = 0;
	float currentAlpha = 0;
	float timer = 0;
	SpriteRenderer spriteRenderer;

	void Awake()
	{
		position = new Vector2(transform.position.x, transform.position.y);
		currentAngle = transform.eulerAngles.z;
		spriteRenderer = GetComponent<SpriteRenderer>();
		currentAlpha = spriteRenderer.color.a;
	}

	void Update()
	{
		timer += Time.deltaTime;
		float bounceDist = bounceLength * Mathf.Sin(timer * bounceSpeed);
		float lerpAmt = 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime);

		transform.position = new Vector3(
			Mathf.Lerp(transform.position.x, position.x + bounceDist * -Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad), lerpAmt),
			Mathf.Lerp(transform.position.y, position.y + bounceDist * Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad), lerpAmt),
			transform.position.z);
		transform.eulerAngles = new Vector3(
			transform.eulerAngles.x,
			transform.eulerAngles.y,
			Mathf.LerpAngle(transform.eulerAngles.z, currentAngle, lerpAmt));
		spriteRenderer.color = new Color(
			spriteRenderer.color.r,
			spriteRenderer.color.g,
			spriteRenderer.color.b,
			Mathf.LerpAngle(spriteRenderer.color.a, currentAlpha, lerpAmt));
	}

	public void SetPosition(Vector2 _newPos, bool _setPos = false)
	{
		position = _newPos;
		if (_setPos)
			transform.position = new Vector3(position.x, position.y, transform.position.z);
	}

	public void SetRotation(float _newAngle, bool _setRot = false)
	{
		currentAngle = _newAngle;
		if (_setRot)
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, currentAngle);
	}

	public void SetOpacity(float _newAlpha, bool _setAlpha = false)
	{
		currentAlpha = _newAlpha;
		if (_setAlpha)
			spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, currentAlpha);
	}

	public float GetOpacity()
	{
		return spriteRenderer.color.a;
	}
}