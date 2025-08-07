using UnityEngine;
using UnityEngine.InputSystem;

public class CameraDragController2D : MonoBehaviour
{
	[Header("Zoom Stats")]
	[SerializeField] bool allowZoom = true;
	[SerializeField] float zoomSpeed = 5f;
	[SerializeField] float minZoom = 2f;
	[SerializeField] float maxZoom = 10f;

	Vector3 dragOrigin;
	PlayerInputActions controls;

	void Awake()
	{
		controls = new PlayerInputActions();
	}

	void OnEnable()
	{
		controls.Gameplay.Enable();
	}

	void OnDisable()
	{
		controls.Gameplay.Disable();
	}

	void LateUpdate()
	{
		HandleDrag();
		if (allowZoom) HandleZoom();
	}

	void HandleDrag()
	{
		if (controls.Gameplay.DragMap.WasPressedThisFrame()) // right click
			SetDragOrigin();

		if (controls.Gameplay.DragMap.IsPressed())
		{
			Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			transform.position += difference;
		}
	}

	public void SetDragOrigin()
	{
		dragOrigin = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
	}

	void HandleZoom()
	{
		float scroll = controls.Gameplay.ZoomMap.ReadValue<Vector2>().y;
		Camera cam = Camera.main;
		if (cam.orthographic)
		{
			cam.orthographicSize -= scroll * zoomSpeed * Time.deltaTime;
			cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
		}
	}
}