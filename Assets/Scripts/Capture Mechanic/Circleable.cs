using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Circleable : MonoBehaviour
{
	[SerializeField] LineDrawer lineDrawer;
	[SerializeField] LayerMask hurtboxLayer;

	public UnityEvent onFullCircle;
	public UnityEvent onCircleCollide;

	float totalAngle = 0;
	Collider2D hurtboxCollider;

	void Awake()
	{
		if (lineDrawer == null)
		{
			GameObject gameManager = GameObject.FindWithTag("GameController");
			if (gameManager != null)
				lineDrawer = gameManager.GetComponent<LineDrawer>();
		}

		hurtboxCollider = GetComponent<Collider2D>();
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
		// check if line collides with collider (or hitbox?)
		if (lineDrawer != null && lineDrawer.points.Count > 1 && hurtboxCollider != null)
		{
			for (int i = 0; i < lineDrawer.points.Count - 1; ++i)
			{
				RaycastHit2D hit = Physics2D.Linecast(lineDrawer.points[i], lineDrawer.points[i + 1], hurtboxLayer);

				if (hit.collider != null && hit.collider == hurtboxCollider)
				{
					ResetCircle();
					onCircleCollide.Invoke();
					break;
				}
			}
		}

		// calculate angle
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

	public void ResetCircle()
	{
		totalAngle = 0;

		// also reset line drawer points
		if (lineDrawer != null)
			lineDrawer.ResetPoints();
	}
}