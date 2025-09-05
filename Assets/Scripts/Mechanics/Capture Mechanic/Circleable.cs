using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class Circleable : MonoBehaviour
{
	[SerializeField] LayerMask hurtboxLayer;

	[SerializeField, Tooltip("Prefab to spawn when line abruptly ends")]
	SpriteRenderer xPrefab;

	public UnityEvent onFullCircle;
	public UnityEvent onCircleCollide;
	[SerializeField] bool checkForClicks = false;
	public UnityEvent onClick;

	float totalAngle = 0;
	Collider2D hurtboxCollider;

	Camera mainCamera;

	void Awake()
	{
		hurtboxCollider = GetComponent<Collider2D>();
		if (checkForClicks) mainCamera = Camera.main;
	}
	
	void OnDrawGizmosSelected()
	{
		LineDrawer lineDrawer = LineDrawer.Instance;
		if (lineDrawer != null && lineDrawer.points.Count > 1)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, lineDrawer.points[lineDrawer.points.Count - 2]);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, lineDrawer.points[lineDrawer.points.Count-1]);
		}
	}

	void Update()
	{
		// check for left mouse button click
		if (checkForClicks && Mouse.current.leftButton.wasPressedThisFrame)
		{
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()), Vector2.zero);

			if (hit.collider != null && hit.collider.gameObject == gameObject)
			{
				SpawnX(hit.point);
				onClick.Invoke();
			}
		}

		// check if line collides with collider (or hitbox?)
		LineDrawer lineDrawer = LineDrawer.Instance;
		if (lineDrawer != null && lineDrawer.points.Count > 1 && hurtboxCollider != null)
		{
			for (int i = 0; i < lineDrawer.points.Count - 1; ++i)
			{
				RaycastHit2D hit = Physics2D.Linecast(lineDrawer.points[i], lineDrawer.points[i + 1], hurtboxLayer);

				if (hit.collider != null && hit.collider == hurtboxCollider)
				{
					if (i > 0)
					{
						SpawnX(hit.point);
						onCircleCollide.Invoke();
					}
					ResetCircle();
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
		if (LineDrawer.Instance != null)
			LineDrawer.Instance.ResetPoints();
	}

	void SpawnX(Vector2 pos)
	{
		if (xPrefab != null)
		{
			SpriteRenderer x = Instantiate(xPrefab, pos, Quaternion.identity);
			Destroy(x.gameObject, 0.3f);
		}
	}
}