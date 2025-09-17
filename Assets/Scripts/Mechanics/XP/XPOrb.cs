using System.Collections;
using UnityEngine;

public class XPOrb : MonoBehaviour
{
	public enum XPOrbType
	{
		small = 1,
		medium = 5,
		big = 10,
		huge = 50,
		max = 200,
	}

	[SerializeField] XPOrbType xpAmount = XPOrbType.small;
	[SerializeField, Min(0)] float collectionTime = 0.5f;
	[SerializeField] float randomAmount = 0.2f;

	Coroutine collectionCoroutine;
	[SerializeField] SpriteRenderer sprite;
	[SerializeField, Min(0)] float bounceTime = 0.3f;
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
		collectionTime += Random.value * randomAmount;
		while (currentTime < collectionTime)
		{
			if (!collector.canCollect)
			{
				collectionCoroutine = null;
				yield break;
			}

			currentTime += Time.deltaTime;
			transform.position = new Vector2(
				EaseUtils.Interpolate(currentTime / collectionTime, startPos.x, collector.transform.position.x, EaseUtils.EaseInSine),
				EaseUtils.Interpolate(currentTime / collectionTime, startPos.y, collector.transform.position.y, EaseUtils.EaseInSine));
			yield return null;
		}
		collector.OnCollectOrb.Invoke(GetXpWorth());
		Destroy(gameObject);
	}

	public int GetXpWorth()
	{
		return (int)xpAmount;
	}

	public void SetXpWorth(XPOrbType type)
	{
		xpAmount = type;

		switch (xpAmount)
		{
			default:
			case XPOrbType.small:
				transform.localScale = Vector3.one;
				if (sprite != null) sprite.color = new Color(156f / 255, 250f / 255, 96f / 255);
				break;
			case XPOrbType.medium:
				transform.localScale = new Vector3(1.2f, 1.2f, 1);
				if (sprite != null) sprite.color = new Color(212f / 255, 250f / 255, 96f / 255);
				break;
			case XPOrbType.big:
				transform.localScale = new Vector3(1.4f, 1.4f, 1);
				if (sprite != null) sprite.color = new Color(250f / 255, 190f / 255, 96f / 255);
				break;
			case XPOrbType.huge:
				transform.localScale = new Vector3(1.7f, 1.7f, 1);
				if (sprite != null) sprite.color = new Color(250f / 255, 96f / 255, 159f / 255);
				break;
			case XPOrbType.max:
				transform.localScale = new Vector3(2, 2, 1);
				if (sprite != null) sprite.color = new Color(174f / 255, 96f / 255, 250f / 255);
				break;
		}
	}

	void OnValidate()
	{
		SetXpWorth(xpAmount);
	}
}
