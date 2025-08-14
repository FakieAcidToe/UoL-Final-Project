using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AfterImage : MonoBehaviour
{
	[SerializeField] float fadeSpeed = 2f;

	public SpriteRenderer spriteRenderer { private set; get; }
	Color color;

	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void Start()
	{
		color = spriteRenderer.color;
	}

	void Update()
	{
		color.a -= fadeSpeed * Time.deltaTime;
		spriteRenderer.color = color;

		if (color.a <= 0)
			Destroy(gameObject);
	}
}