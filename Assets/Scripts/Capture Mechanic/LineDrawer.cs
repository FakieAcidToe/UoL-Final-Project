using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineDrawer : MonoBehaviour
{
	[SerializeField, Tooltip("How long each point stays visible (in seconds)")]
	float pointLifetime = 0.5f;

	LineRenderer lineRenderer;
	Camera mainCam;

	public List<Vector3> points { get; private set; }
	List<float> pointTimestamps;

	public bool hasUpdatedPointsThisFrame { get; private set; }

	void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		mainCam = Camera.main;

		lineRenderer.positionCount = 0;
		lineRenderer.useWorldSpace = true;

		points = new List<Vector3>();
		pointTimestamps = new List<float>();

		hasUpdatedPointsThisFrame = false;
	}

	void Update()
	{
		float currentTime = Time.time;
		hasUpdatedPointsThisFrame = false;

		// delete old points
		while (pointTimestamps.Count > 0 && currentTime - pointTimestamps[0] > pointLifetime)
		{
			pointTimestamps.RemoveAt(0);
			points.RemoveAt(0);
			hasUpdatedPointsThisFrame = true;
		}

		if (Input.GetMouseButtonDown(0))
		{
			points.Clear();
			pointTimestamps.Clear();
			lineRenderer.positionCount = 0;
		}

		if (Input.GetMouseButton(0))
		{
			Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
			mousePos.z = 0f;

			if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], mousePos) > 0.1f)
			{
				points.Add(mousePos);
				pointTimestamps.Add(currentTime);
				hasUpdatedPointsThisFrame = true;
			}
		}

		// update line renderer if points were removed
		if (hasUpdatedPointsThisFrame)
		{
			lineRenderer.positionCount = points.Count;
			lineRenderer.SetPositions(points.ToArray());
		}
	}
}