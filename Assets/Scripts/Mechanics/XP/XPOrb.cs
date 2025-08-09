using System.Collections;
using UnityEngine;

public class XPOrb : MonoBehaviour
{
	[SerializeField] int xpAmount = 1;
	[SerializeField] float collectionTime = 0.5f;

	Coroutine collectionCoroutine;

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
