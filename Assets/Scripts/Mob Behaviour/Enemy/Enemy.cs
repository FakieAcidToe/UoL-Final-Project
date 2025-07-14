using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyAnimSetLoader))]
public class Enemy : MonoBehaviour
{
	enum EnemyState
	{
		idle,
		chase,
		spared
	}

	[Header("Animation")]
	[SerializeField] EnemyAnimSetLoader animLoader;
	EnemyState state = EnemyState.idle;

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
	float colliderSize;
	Vector2Int lastTargetPosition;
	Vector2Int lastPosition;

	[Header("Map Reference")]
	public BoundsInt homeRoom;
	public HashSet<Vector2Int> tiles;
	public Vector2 mapOffset;
	public Dictionary<Vector2Int, List<Vector2Int>> neighborCache;

	Circleable circle;
	Rigidbody2D rb;

	void Awake()
	{
		circle = GetComponent<Circleable>();
		rb = GetComponent<Rigidbody2D>();

		hp = maxHp;

		colliderSize = GetComponent<Collider2D>().bounds.size.x;
	}

	void Start()
	{
		// random flipx
		animLoader.SetFlipX(Vector2.right * Random.Range(-1f, 1f));
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
		animLoader.SetFlipX(movement);

		CheckIfShouldRecalculate();
	}

	void OnDrawGizmosSelected()
	{
		// homeroom
		if (state == EnemyState.idle && homeRoom != null && homeRoom.size != Vector3Int.zero)
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
		else if (state == EnemyState.chase)
		{
			ThickLinecast.DrawThickLineGizmo(transform.position, target.transform.position, colliderSize, Color.green);
		}
	}

	void PathfindToTarget()
	{
		movement = Vector2.zero;
		if (target == null) return;

		Vector2 targetPosition = target.transform.position;

		if (state == EnemyState.idle &&
			(homeRoom == null || // only pathfind in homeroom
			homeRoom.size == Vector3Int.zero ||
			(targetPosition.x >= homeRoom.x && targetPosition.x <= homeRoom.xMax && targetPosition.y >= homeRoom.y && targetPosition.y <= homeRoom.yMax))
			)
			ChangeState(EnemyState.chase);

		if (state == EnemyState.chase)
		{
			RaycastHit2D[] hits = ThickLinecast.ThickLinecast2D(transform.position, targetPosition, colliderSize, wallLayerMask);
			if (hits.Length > 0) // dungeon wall in the way
			{
				if (shouldRecalculate) // if player or enemy moved to a new tile
				{
					waypoints = AStarPathfinding.FindPath(Vector2Int.FloorToInt(transform.position), lastTargetPosition, tiles, neighborCache);
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
		int lastIndexToKeep = waypoints.Count - 1;

		for (int i = waypoints.Count - 1; i >= 0; --i)
		{
			RaycastHit2D[] hits = ThickLinecast.ThickLinecast2D(transform.position, waypoints[i] + mapOffset, colliderSize, wallLayerMask);
			if (hits.Length <= 0)
			{
				lastIndexToKeep = i;
				break;
			}
		}

		if (lastIndexToKeep > 0)
			waypoints.RemoveRange(0, lastIndexToKeep);
	}

	void CheckIfShouldRecalculate()
	{
		if (shouldRecalculate) return;

		Vector2Int currentTargetPosition = Vector2Int.FloorToInt(target.transform.position);
		Vector2Int currentPosition = Vector2Int.FloorToInt(transform.position);
		if (lastTargetPosition != currentTargetPosition || lastPosition != currentPosition || currentPosition == currentTile)
		{
			lastTargetPosition = currentTargetPosition;
			lastPosition = currentPosition;
			shouldRecalculate = true;
		}
	}

	void OnFullCircle()
	{
		if (++circlesDrawn >= numOfCirclesToCapture)
		{
			ChangeState(EnemyState.spared);
		}
	}

	void OnCircleCollide()
	{
		circlesDrawn = 0;
	}

	void ChangeState(EnemyState newState)
	{
		if (state != newState && animLoader != null)
		{
			state = newState;
			switch (state)
			{
				default:
				case EnemyState.idle:
					animLoader.ChangeState(EnemyAnimSetLoader.EnemyAnimState.idle);
					break;
				case EnemyState.chase:
					animLoader.ChangeState(EnemyAnimSetLoader.EnemyAnimState.run);
					break;
				case EnemyState.spared:
					animLoader.ChangeState(EnemyAnimSetLoader.EnemyAnimState.spare);
					break;
			}
		}
	}
}