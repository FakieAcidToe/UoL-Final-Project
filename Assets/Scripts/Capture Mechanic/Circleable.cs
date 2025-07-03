using UnityEngine;
using UnityEngine.Events;

public class Circleable : MonoBehaviour
{
	[SerializeField] LineDrawer lineDrawer;

	[SerializeField] UnityEvent onFullCircle;

	float totalAngle = 0;

	void Awake()
	{
		if (lineDrawer == null)
		{
			GameObject gameManager = GameObject.FindWithTag("GameController");
			if (gameManager != null)
				lineDrawer = gameManager.GetComponent<LineDrawer>();
		}
	}
	/*
	void OnDrawGizmos()
	{
		if (lineDrawer != null && lineDrawer.points.Count > 1)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, lineDrawer.points[lineDrawer.points.Count - 2]);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, lineDrawer.points[lineDrawer.points.Count-1]);
		}
	}*/

	void Update()
	{
		if (lineDrawer != null && lineDrawer.points.Count > 1)
		{
			if (lineDrawer.hasUpdatedPointsThisFrame)
			{
				// add total angle
				totalAngle += Vector2.SignedAngle(transform.position - lineDrawer.points[lineDrawer.points.Count - 1], transform.position - lineDrawer.points[lineDrawer.points.Count - 2]);

				// full circle performed
				if (Mathf.Abs(totalAngle) >= 360)
				{
					totalAngle -= Mathf.Sign(totalAngle)*360;
					onFullCircle.Invoke();
				}
			}
		}
		else // reset
			totalAngle = 0;
	}
}