using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] float moveSpeed = 2f;
	Vector2 movement;

	[Header("Health")]
	[SerializeField, Min(0)] int maxHp = 10;
	int hp = 10;

	[Header("Capture")]
	[SerializeField, Min(0)] int numOfCirclesToCapture = 3;
	int circlesDrawn = 0;

	[Header("Pathfinding")]
	public GameObject target;
	List<Vector2Int> waypoints;
	bool shouldRecalculate = true;
	[SerializeField] LayerMask wallLayerMask;
	[SerializeField, Min(0)] float distanceBeforeNextTile = 0.2f;
	Vector2Int currentTile;
	bool isChasing = false;

	[Header("Map Reference")]
	public BoundsInt homeRoom;
	public HashSet<Vector2Int> tiles;
	public Vector2 mapOffset;

	Circleable circle;
	Rigidbody2D rb;

	void Awake()
	{
		circle = GetComponent<Circleable>();
		rb = GetComponent<Rigidbody2D>();

		hp = maxHp;
	}

	void OnEnable()
	{
		if (circle != null)
		{
			circle.onFullCircle.AddListener(OnFullCircle);
			circle.onCircleCollide.AddListener(OnCircleCollide);
		}
	}

	void OnDisable()
	{
		if (circle != null)
		{
			circle.onFullCircle.RemoveListener(OnFullCircle);
			circle.onCircleCollide.RemoveListener(OnCircleCollide);
		}
	}

	void Update()
	{
		PathfindToTarget();
	}

	void FixedUpdate()
	{
		// move the player using physics
		rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
		shouldRecalculate = true;
	}

	void OnDrawGizmosSelected()
	{
		// homeroom
		if (!isChasing && homeRoom != null && homeRoom.size != Vector3Int.zero)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(homeRoom.center, homeRoom.size);
		}

		// waypoints
		if (waypoints != null && waypoints.Count > 1)
		{
			Gizmos.color = new Color(0, 1, 0, 0.3f);
			Gizmos.DrawCube(currentTile + mapOffset, Vector2.one);

			Gizmos.color = Color.green;
			for (int i = 0; i < waypoints.Count - 1; ++i)
			{
				Vector3 start = (Vector3Int)waypoints[i];
				Vector3 end = (Vector3Int)waypoints[i + 1];
				Gizmos.DrawLine(start + (Vector3)mapOffset, end + (Vector3)mapOffset);
			}
		}
		else if (isChasing)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(transform.position, target.transform.position);
		}
	}

	void PathfindToTarget()
	{
		movement = Vector2.zero;
		if (target == null) return;

		Vector2 targetPosition = target.transform.position;

		if (!isChasing &&
			(homeRoom == null || // only pathfind in homeroom
			homeRoom.size == Vector3Int.zero ||
			(targetPosition.x >= homeRoom.x && targetPosition.x <= homeRoom.xMax && targetPosition.y >= homeRoom.y && targetPosition.y <= homeRoom.yMax))
			)
			isChasing = true;

		if (isChasing)
		{
			RaycastHit2D hit = Physics2D.Linecast(transform.position, targetPosition, wallLayerMask);
			if (hit.collider != null) // dungeon wall in the way
			{
				if (shouldRecalculate)
				{
					bool shouldResetCurrentTile = waypoints == null || waypoints.Count <= 0;

					waypoints = AStarPathfinding.FindPath(Vector2Int.FloorToInt(transform.position), Vector2Int.FloorToInt(targetPosition), tiles);
					shouldRecalculate = false;

					if (shouldResetCurrentTile && waypoints.Count > 0)
						currentTile = waypoints[0];
					else if (waypoints.Count > 1)
					{
						if ((waypoints[0] + mapOffset - (Vector2)transform.position).magnitude <= distanceBeforeNextTile)
							currentTile = waypoints[1];
					}
					else
						currentTile = Vector2Int.FloorToInt(transform.position);
				}

				movement = currentTile + mapOffset - (Vector2)transform.position;
			}
			else // no walls in the way
			{
				if (waypoints != null)
					waypoints.Clear();
				movement = targetPosition - (Vector2)transform.position;
			}

			if (movement.sqrMagnitude > 0) movement.Normalize();
		}
	}

	void OnFullCircle()
	{
		if (++circlesDrawn >= numOfCirclesToCapture)
		{
			// captured!
			gameObject.SetActive(false);
		}
	}

	void OnCircleCollide()
	{
		circlesDrawn = 0;
	}
}
