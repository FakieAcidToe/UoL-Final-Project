using UnityEngine;

public class UIFader : MonoBehaviour
{
	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] bool startFaded = true;

	// previous variables
	float alpha = 1f;
	bool interactable = false;
	bool blocksRaycasts = false;

	Coroutine currentCouroutine;
	bool isFadeOutRunning = false;
	bool isFadeInRunning = false;

	void Awake()
	{
		alpha = canvasGroup.alpha;
		interactable = canvasGroup.interactable;
		blocksRaycasts = canvasGroup.blocksRaycasts;

		if (startFaded)
		{
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
	}

	public float GetCurrentAlpha()
	{
		return canvasGroup.alpha;
	}

	public void FadeOutCoroutine(float fadeDuration = 0.2f)
	{
		// stop fade in
		if (currentCouroutine != null && isFadeInRunning)
		{
			isFadeInRunning = false;
			StopCoroutine(currentCouroutine);
		}

		if (!isFadeOutRunning) // dont interrupt current fade out
		{
			isFadeInRunning = false;
			currentCouroutine = StartCoroutine(FadeOut(fadeDuration));
		}
	}

	public void FadeInCoroutine(float fadeDuration = 0.2f)
	{
		// stop fade out
		if (currentCouroutine != null && isFadeOutRunning)
		{
			isFadeOutRunning = false;
			StopCoroutine(currentCouroutine);
		}

		if (!isFadeInRunning) // dont interrupt current fade in
		{
			isFadeOutRunning = false;
			currentCouroutine = StartCoroutine(FadeIn(fadeDuration));
		}
	}

	System.Collections.IEnumerator FadeOut(float fadeDuration = 0.2f)
	{
		isFadeOutRunning = true;

		float startAlpha = canvasGroup.alpha;
		float time = 0f;

		while (time < fadeDuration)
		{
			canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeDuration);
			time += Time.deltaTime;
			yield return null;
		}

		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;

		isFadeOutRunning = false;
	}

	System.Collections.IEnumerator FadeIn(float fadeDuration = 0.2f)
	{
		isFadeInRunning = true;

		float startAlpha = canvasGroup.alpha;
		float time = 0f;

		while (time < fadeDuration)
		{
			canvasGroup.alpha = Mathf.Lerp(startAlpha, alpha, time / fadeDuration);
			time += Time.deltaTime;
			yield return null;
		}

		canvasGroup.alpha = alpha;
		canvasGroup.interactable = interactable;
		canvasGroup.blocksRaycasts = blocksRaycasts;

		isFadeInRunning = false;
	}
}