using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineCircleDrawer : MonoBehaviour
{
	[SerializeField, Min(0)] float radius = 1f;
	[SerializeField, Min(0)] float timeToDraw = 1f;
	[SerializeField, Min(1)] int interpolationsPerFrame = 2;

	[SerializeField, Tooltip("How long each point stays visible (in seconds)")]
	float pointLifetime = 0.25f;
	[SerializeField] GameObject wand;

	LineRenderer lineRenderer;

	public List<Vector3> points { get; private set; }
	List<float> pointTimestamps;

	Coroutine currentCoroutine;

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();

		lineRenderer.positionCount = 0;
		lineRenderer.useWorldSpace = true;

		points = new List<Vector3>();
		pointTimestamps = new List<float>();
	}

	public void DrawCircle()
	{
		if (currentCoroutine != null) StopCoroutine(currentCoroutine);
		currentCoroutine = StartCoroutine(DrawCircleCoroutine());
	}

	IEnumerator DrawCircleCoroutine()
	{
		ResetPoints();
		wand.SetActive(true);

		float lengthToDraw = Random.Range(360f, 450f);
		if (Random.value < 0.5f) lengthToDraw *= -1; // random dir
		float randomAngle = Random.Range(0, Mathf.Deg2Rad * lengthToDraw);

		float timer = 0;
		float timerLast = 0;
		while (timer < timeToDraw)
		{
			timerLast = timer;
			timer += Time.deltaTime;
			Vector3 pos = Vector3.zero;

			for (int i = 0; i < interpolationsPerFrame; ++i)
			{
				float angle = Mathf.Deg2Rad * (EaseUtils.Interpolate(Mathf.Lerp(timerLast, timer, (float)(i + 1) / interpolationsPerFrame) / timeToDraw, 0, lengthToDraw, EaseUtils.EaseInOutCubic)) + randomAngle;
				float x = Mathf.Cos(angle) * radius;
				float y = Mathf.Sin(angle) * radius;
				pos = new Vector3(x + transform.position.x, y + transform.position.y, 0);
				points.Add(pos);
				pointTimestamps.Add(Time.time);
			}

			UpdatePoints();
			wand.transform.position = pos;

			yield return null;
		}
		yield return new WaitForSeconds(0.2f);
		wand.SetActive(false);
	}

	void Update()
	{
		if (Time.timeScale > 0)
		{
			while (pointTimestamps.Count > 0 && Time.time - pointTimestamps[0] > pointLifetime)
			{
				pointTimestamps.RemoveAt(0);
				points.RemoveAt(0);
				UpdatePoints();
			}
		}
		else
			ResetPoints();
	}

	void UpdatePoints()
	{
		lineRenderer.positionCount = points.Count;
		lineRenderer.SetPositions(points.ToArray());
	}

	public void ResetPoints()
	{
		points.Clear();
		pointTimestamps.Clear();
		lineRenderer.positionCount = 0;
	}
}