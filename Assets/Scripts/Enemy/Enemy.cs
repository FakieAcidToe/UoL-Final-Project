using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
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
	Vector2Int currentTile;
	bool isChasing = false;
	float colliderSize;
	Vector2Int lastTargetPosition;

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

		colliderSize = GetComponent<Collider2D>().bounds.size.x;
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
		CheckIfShouldRecalculate();
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

			ThickLinecast.DrawThickLineGizmo(transform.position, currentTile + mapOffset, colliderSize, Color.green);
			for (int i = 0; i < waypoints.Count - 1; ++i)
			{
				Vector3 start = (Vector3Int)waypoints[i];
				Vector3 end = (Vector3Int)waypoints[i + 1];
				Gizmos.DrawLine(start + (Vector3)mapOffset, end + (Vector3)mapOffset);
			}
		}
		else if (isChasing)
		{
			ThickLinecast.DrawThickLineGizmo(transform.position, target.transform.position, colliderSize, Color.green);
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
			RaycastHit2D[] hits = ThickLinecast.ThickLinecast2D(transform.position, targetPosition, colliderSize, wallLayerMask);
			if (hits.Length > 0) // dungeon wall in the way
			{
				if (shouldRecalculate) // if player or enemy moved to a new tile
				{
					waypoints = AStarPathfinding.FindPath(Vector2Int.FloorToInt(transform.position), lastTargetPosition, tiles);
					SimplifyWaypoints();
					currentTile = (waypoints.Count > 0) ? waypoints[0] : Vector2Int.FloorToInt(transform.position);
					shouldRecalculate = false;
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

	void SimplifyWaypoints()
	{
		bool foundStraightPath = false;
		for (int i = waypoints.Count - 1; i >= 0; --i)
		{
			if (foundStraightPath)
				waypoints.RemoveAt(i);
			else
			{
				RaycastHit2D[] hits = ThickLinecast.ThickLinecast2D(transform.position, waypoints[i] + mapOffset, colliderSize, wallLayerMask);
				if (hits.Length <= 0) foundStraightPath = true;
			}
		}
	}

	void CheckIfShouldRecalculate()
	{
		if (shouldRecalculate) return;

		Vector2Int currentTargetPosition = Vector2Int.FloorToInt(target.transform.position);
		if (lastTargetPosition != currentTargetPosition || currentTile == Vector2Int.FloorToInt(transform.position))
		{
			lastTargetPosition = currentTargetPosition;
			shouldRecalculate = true;
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