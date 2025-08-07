using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class LineDrawer : MonoBehaviour
{
	public static LineDrawer Instance { get; private set; }

	[SerializeField, Tooltip("How long each point stays visible (in seconds)")]
	float pointLifetime = 0.5f;

	LineRenderer lineRenderer;
	Camera mainCam;
	bool hasCameraDrag = false;

	CursorManager cursorManager;
	PlayerInputActions controls;

	public List<Vector3> points { get; private set; }
	List<float> pointTimestamps;

	public bool hasUpdatedPointsThisFrame { get; private set; }

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}

		Instance = this;
		lineRenderer = GetComponent<LineRenderer>();
		cursorManager = GetComponent<CursorManager>();
		mainCam = Camera.main;

		controls = KeybindLoader.GetNewInputActions();

		lineRenderer.positionCount = 0;
		lineRenderer.useWorldSpace = true;

		points = new List<Vector3>();
		pointTimestamps = new List<float>();

		hasUpdatedPointsThisFrame = false;
	}

	void OnEnable()
	{
		controls.Gameplay.Enable();
	}

	void OnDisable()
	{
		controls.Gameplay.Disable();
	}

	void Start()
	{
		if (mainCam.GetComponent<CameraDragController2D>() || mainCam.GetComponentInParent<CameraDragController2D>()) hasCameraDrag = true;
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

		if (controls.Gameplay.Attack.WasPressedThisFrame() || (hasCameraDrag && controls.Gameplay.DragMap.IsPressed()))
			ResetPoints();

		if (controls.Gameplay.Attack.IsPressed())
		{
			Vector3 mousePos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			mousePos.z = 0f;

			if (points.Count == 0 || Vector2.Distance(points[points.Count - 1], mousePos) > 0.1f)
			{
				points.Add(mousePos);
				pointTimestamps.Add(currentTime);
				hasUpdatedPointsThisFrame = true;
			}

			if (cursorManager != null) cursorManager.SetCustomCursor();
		}
		else
		{
			if (cursorManager != null) cursorManager.SetDefaultCursor();
		}

		// update line renderer if points were removed
		if (hasUpdatedPointsThisFrame)
		{
			lineRenderer.positionCount = points.Count;
			lineRenderer.SetPositions(points.ToArray());
		}
	}

	public void ResetPoints()
	{
		points.Clear();
		pointTimestamps.Clear();
		lineRenderer.positionCount = 0;

		if (cursorManager != null) cursorManager.SetDefaultCursor();
	}
}