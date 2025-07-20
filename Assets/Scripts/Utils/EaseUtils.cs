using UnityEngine;

public static class EaseUtils
{
	// -------- Sine --------
	public static float EaseInSine(float t) => 1f - Mathf.Cos((t * Mathf.PI) / 2f);
	public static float EaseOutSine(float t) => Mathf.Sin((t * Mathf.PI) / 2f);
	public static float EaseInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;

	// -------- Quad --------
	public static float EaseInQuad(float t) => t * t;
	public static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
	public static float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

	// -------- Cubic --------
	public static float EaseInCubic(float t) => t * t * t;
	public static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
	public static float EaseInOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

	// -------- Quart --------
	public static float EaseInQuart(float t) => t * t * t * t;
	public static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);
	public static float EaseInOutQuart(float t) => t < 0.5f ? 8f * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 4f) / 2f;

	// -------- Quint --------
	public static float EaseInQuint(float t) => t * t * t * t * t;
	public static float EaseOutQuint(float t) => 1f - Mathf.Pow(1f - t, 5f);
	public static float EaseInOutQuint(float t) => t < 0.5f ? 16f * t * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 5f) / 2f;

	// -------- Expo --------
	public static float EaseInExpo(float t) => t == 0f ? 0f : Mathf.Pow(2f, 10f * t - 10f);
	public static float EaseOutExpo(float t) => t == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
	public static float EaseInOutExpo(float t)
	{
		if (t == 0f) return 0f;
		if (t == 1f) return 1f;
		return t < 0.5f
			? Mathf.Pow(2f, 20f * t - 10f) / 2f
			: (2f - Mathf.Pow(2f, -20f * t + 10f)) / 2f;
	}

	// -------- Circ --------
	public static float EaseInCirc(float t) => 1f - Mathf.Sqrt(1f - Mathf.Pow(t, 2f));
	public static float EaseOutCirc(float t) => Mathf.Sqrt(1f - Mathf.Pow(t - 1f, 2f));
	public static float EaseInOutCirc(float t) => t < 0.5f
		? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * t, 2f))) / 2f
		: (Mathf.Sqrt(1f - Mathf.Pow(-2f * t + 2f, 2f)) + 1f) / 2f;

	// -------- Back --------
	public static float EaseInBack(float t)
	{
		const float c1 = 1.70158f;
		return (c1 + 1f) * t * t * t - c1 * t * t;
	}

	public static float EaseOutBack(float t)
	{
		const float c1 = 1.70158f;
		t -= 1f;
		return 1f + (c1 + 1f) * t * t * t + c1 * t * t;
	}

	public static float EaseInOutBack(float t)
	{
		const float c1 = 1.70158f;
		const float c2 = c1 * 1.525f;

		return t < 0.5f
			? (Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f
			: (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
	}

	// -------- Bounce (Out only, plus In & InOut via reverse) --------
	public static float EaseOutBounce(float t)
	{
		const float n1 = 7.5625f;
		const float d1 = 2.75f;

		if (t < 1f / d1)
			return n1 * t * t;
		else if (t < 2f / d1)
		{
			t -= 1.5f / d1;
			return n1 * t * t + 0.75f;
		}
		else if (t < 2.5f / d1)
		{
			t -= 2.25f / d1;
			return n1 * t * t + 0.9375f;
		}
		else
		{
			t -= 2.625f / d1;
			return n1 * t * t + 0.984375f;
		}
	}

	public static float EaseInBounce(float t) => 1f - EaseOutBounce(1f - t);

	public static float EaseInOutBounce(float t) =>
		t < 0.5f
			? (1f - EaseOutBounce(1f - 2f * t)) / 2f
			: (1f + EaseOutBounce(2f * t - 1f)) / 2f;

	// -------- Utility: Interpolated easing between start and end --------
	public static float Interpolate(float t, float start, float end, System.Func<float, float> easingFunc)
	{
		return Mathf.Lerp(start, end, easingFunc(t));
	}
}