using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineDrawer : MonoBehaviour
{
	LineRenderer lineRenderer;
	List<Vector3> points = new List<Vector3>();
	Camera mainCam;

	void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		mainCam = Camera.main;

		lineRenderer.positionCount = 0;
		lineRenderer.useWorldSpace = true;
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			points.Clear();
			lineRenderer.positionCount = 0;
		}

		if (Input.GetMouseButton(0))
		{
			Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
			mousePos.z = 0f;

			if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], mousePos) > 0.1f)
			{
				points.Add(mousePos);
				lineRenderer.positionCount = points.Count;
				lineRenderer.SetPositions(points.ToArray());
			}
		}
	}
}
