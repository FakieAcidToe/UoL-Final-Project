using UnityEngine;

public class AfterImageSpawner : MonoBehaviour
{
	[SerializeField] AfterImage afterImagePrefab;
	[SerializeField] float spawnInterval = 0.05f;

	float timer;
	[SerializeField] SpriteRenderer spriteRenderer;

	void OnEnable()
	{
		timer = spawnInterval;
	}

	void Update()
	{
		timer += Time.deltaTime;

		if (timer >= spawnInterval)
		{
			SpawnAfterImage();
			timer = 0;
		}
	}

	void SpawnAfterImage()
	{
		AfterImage afterImage = Instantiate(afterImagePrefab, spriteRenderer.transform.position, spriteRenderer.transform.rotation);
		afterImage.transform.localScale = transform.lossyScale;

		if (spriteRenderer != null && afterImage.spriteRenderer != null)
		{
			afterImage.spriteRenderer.sprite = spriteRenderer.sprite;
			afterImage.spriteRenderer.flipX = spriteRenderer.flipX;
			afterImage.spriteRenderer.color = spriteRenderer.color;
		}
	}
}