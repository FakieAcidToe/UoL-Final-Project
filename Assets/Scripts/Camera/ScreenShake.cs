using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
	public static ScreenShake Instance { private set; get; }

	Vector3 originalPosition;
	Coroutine shakeCoroutine;

	void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(this);

		originalPosition = transform.localPosition;
	}

	/// <summary>
	/// Triggers the screen shake effect.
	/// </summary>
	/// <param name="duration">How long the shake should last</param>
	/// <param name="magnitude">How intense the shake is</param>
	public void Shake(float duration, float magnitude)
	{
		if (shakeCoroutine != null)
			StopCoroutine(shakeCoroutine);

		shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
	}

	IEnumerator ShakeCoroutine(float duration, float magnitude)
	{
		float elapsed = 0f;

		while (elapsed < duration)
		{
			float x = Random.Range(-1f, 1f) * magnitude;
			float y = Random.Range(-1f, 1f) * magnitude;

			transform.localPosition = originalPosition + new Vector3(x, y, 0f);

			elapsed += Time.deltaTime;
			yield return null;
		}

		transform.localPosition = originalPosition;
		shakeCoroutine = null;
	}
}