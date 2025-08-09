using System.Collections;
using UnityEngine;

public class XPOrb : MonoBehaviour
{
	[SerializeField] int xpAmount = 1;
	[SerializeField] float collectionTime = 0.5f;

	Coroutine collectionCoroutine;
	[SerializeField] SpriteRenderer sprite;
	[SerializeField] float bounceTime = 0.3f;
	[SerializeField] float bounceHeight = 0.5f;
	[SerializeField] float randomSpawnAmount = 0.5f;

	void Awake()
	{
		transform.position = (Vector2)transform.position + Random.insideUnitCircle * randomSpawnAmount;
		StartCoroutine(BounceAnimation());
	}

	IEnumerator BounceAnimation()
	{
		float currentTime = 0;
		float originalHeight = sprite.transform.localPosition.y;
		while (currentTime < bounceTime)
		{
			currentTime += Time.deltaTime;
			sprite.transform.localPosition = new Vector2(
				sprite.transform.localPosition.x,
				EaseUtils.Interpolate(currentTime / bounceTime, bounceHeight, originalHeight, EaseUtils.EaseInBounce));
			yield return null;
		}
		sprite.transform.localPosition = new Vector2(sprite.transform.localPosition.x, originalHeight);
	}

	public void Collect(XPCollector collector)
	{
		if (collectionCoroutine == null)
			collectionCoroutine = StartCoroutine(CollectCoroutine(collector));
	}

	IEnumerator CollectCoroutine(XPCollector collector)
	{
		float currentTime = 0;
		Vector2 startPos = transform.position;
		while (currentTime < collectionTime)
		{
			if (!collector.canCollect) yield break;

			currentTime += Time.deltaTime;
			transform.position = new Vector2(
				EaseUtils.Interpolate(currentTime / collectionTime, startPos.x, collector.transform.position.x, EaseUtils.EaseInSine),
				EaseUtils.Interpolate(currentTime / collectionTime, startPos.y, collector.transform.position.y, EaseUtils.EaseInSine));
			yield return null;
		}
		collector.OnCollectOrb.Invoke(xpAmount);
		Destroy(gameObject);
	}
}
