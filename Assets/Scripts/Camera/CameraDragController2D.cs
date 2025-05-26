using UnityEngine;

public class CameraDragController2D : MonoBehaviour
{
	[SerializeField] float zoomSpeed = 5f;
	[SerializeField] float minZoom = 2f;
	[SerializeField] float maxZoom = 10f;

	private Vector3 dragOrigin;

	void Update()
	{
		HandleDrag();
		HandleZoom();
	}

	void HandleDrag()
	{
		if (Input.GetMouseButtonDown(0)) // Left click
			dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		if (Input.GetMouseButton(0))
		{
			Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
			transform.position += difference;
		}
	}

	void HandleZoom()
	{
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		Camera cam = Camera.main;
		if (cam.orthographic)
		{
			cam.orthographicSize -= scroll * zoomSpeed;
			cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
		}
	}
}