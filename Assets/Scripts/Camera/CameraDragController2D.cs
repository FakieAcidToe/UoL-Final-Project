using UnityEngine;

public class CameraDragController2D : MonoBehaviour
{
	[Header("Zoom Stats")]
	[SerializeField] bool allowZoom = true;
	[SerializeField] float zoomSpeed = 5f;
	[SerializeField] float minZoom = 2f;
	[SerializeField] float maxZoom = 10f;

	Vector3 dragOrigin;

	void LateUpdate()
	{
		HandleDrag();
		if (allowZoom) HandleZoom();
	}

	void HandleDrag()
	{
		if (Input.GetMouseButtonDown(1)) // right click
			SetDragOrigin();

		if (Input.GetMouseButton(1))
		{
			Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
			transform.position += difference;
		}
	}

	public void SetDragOrigin()
	{
		dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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