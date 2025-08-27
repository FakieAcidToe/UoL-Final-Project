using UnityEngine;

public class SineHover : MonoBehaviour
{
	[SerializeField, Tooltip("Amplitude and direction of the effect.")]
	Vector2 hoverAmplitude = new Vector2(0f, 0.5f);

	[SerializeField, Tooltip("Frequency of the motion.")]
	float hoverFrequency = 1f;

	Vector3 startPosition;

	void Start()
	{
		startPosition = transform.localPosition;
	}

	void Update()
	{
		float sinValue = Mathf.Sin(Time.time * hoverFrequency * 2f * Mathf.PI);
		transform.localPosition = startPosition + new Vector3(hoverAmplitude.x * sinValue, hoverAmplitude.y * sinValue, 0f);
	}
}