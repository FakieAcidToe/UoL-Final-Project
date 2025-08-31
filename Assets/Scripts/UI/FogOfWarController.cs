using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FogOfWarController : MonoBehaviour
{
	[SerializeField] RawImage fogImage;
	[SerializeField, Min(0)] float sizeIncreaseSpeed = 1;

	[System.Serializable]
	struct Target
	{
		public Target(Vector3 _position, float _targetRadius)
		{
			movingTarget = null;
			position = _position;
			targetRadius = _targetRadius;
			actualRadius = 0;
		}

		public Target(Transform _transform, float _targetRadius)
		{
			movingTarget = _transform;
			position = movingTarget.position;
			targetRadius = _targetRadius;
			actualRadius = 0;
		}

		public void Update(float sizeIncreaseSpeed)
		{
			if (movingTarget != null)
				position = movingTarget.position;

			if (actualRadius < targetRadius)
			{
				actualRadius += sizeIncreaseSpeed * Time.deltaTime;
				if (actualRadius > targetRadius)
					actualRadius = targetRadius;
			}
		}

		public Transform movingTarget;
		public Vector3 position;
		public float targetRadius;
		public float actualRadius;
	}
	List<Target> targets;

	Material _mat;
	const int maxCircles = 50;
	Vector4[] circlePositions;
	float[] circleRadii;

	void Awake()
	{
		_mat = Instantiate(fogImage.material);
		fogImage.material = _mat;

		targets = new List<Target>();
		circlePositions = new Vector4[maxCircles];
		circleRadii = new float[maxCircles];
	}

	void Update()
	{
		int count = Mathf.Min(targets.Count, maxCircles);
		for (int i = 0; i < count; ++i)
		{
			Target t = targets[i];
			t.Update(sizeIncreaseSpeed);
			targets[i] = t;

			Vector3 screenPos = Camera.main.WorldToViewportPoint(t.position);
			circlePositions[i] = new Vector4(screenPos.x, screenPos.y, 0, 0);
			circleRadii[i] = (i < targets.Count) ? t.actualRadius : 0.2f;
		}

		_mat.SetInt("_CircleCount", count);
		_mat.SetVectorArray("_CirclePositions", circlePositions);
		_mat.SetFloatArray("_CircleRadii", circleRadii);
		_mat.SetFloat("_CircleScale", 12f / (Camera.main.orthographicSize * Camera.main.aspect));
	}

	public void AddRevealTarget(Vector3 target, float targetSize)
	{
		targets.Add(new Target(target, targetSize));
	}

	public void AddRevealTarget(Transform target, float targetSize)
	{
		targets.Add(new Target(target, targetSize));
	}

	public void DeleteTargets()
	{
		targets.Clear();
	}
}