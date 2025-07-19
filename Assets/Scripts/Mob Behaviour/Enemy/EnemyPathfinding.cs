using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Enemy;

[RequireComponent(typeof(Enemy), typeof(Collider2D))]
public class EnemyPathfinding : MonoBehaviour
{
	public GameObject target;
	List<Vector2Int> waypoints;
	bool shouldRecalculate = true;
	[SerializeField] LayerMask wallLayerMask;
	Vector2Int currentTile;
	float colliderSize;
	Vector2Int lastTargetPosition;
	Vector2Int lastPosition;
	[Tooltip("Calculate pathfinding every x fixed updates")] public int staggerPer = 4;
	[Tooltip("Offset index for staggering")] public int staggerIndex = 0;
	int staggerCurrent = 0;

	// map references
	[HideInInspector] public BoundsInt homeRoom;
	[HideInInspector] public HashSet<Vector2Int> tiles;
	[HideInInspector] public Vector2 mapOffset;
	[HideInInspector] public Dictionary<Vector2Int, List<Vector2Int>> neighborCache;

	Enemy enemy;

	void Awake()
	{
		enemy = GetComponent<Enemy>();
		colliderSize = GetComponent<Collider2D>().bounds.size.x;
	}

	void OnDrawGizmosSelected()
	{
		if (enemy != null && !enemy.IsBeingControlledByPlayer())
		{
			// homeroom
			if (enemy.state == EnemyState.idle && homeRoom != null && homeRoom.size != Vector3Int.zero)
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
			else if (enemy.state == EnemyState.chase)
			{
				ThickLinecast.DrawThickLineGizmo(transform.position, target.transform.position, colliderSize, Color.green);
			}
		}
	}

	public bool ShouldStartChasing()
	{
		Vector2 targetPosition = target.transform.position;
		return	homeRoom == null || // only pathfind in homeroom
				homeRoom.size == Vector3Int.zero ||
				(targetPosition.x >= homeRoom.x && targetPosition.x <= homeRoom.xMax && targetPosition.y >= homeRoom.y && targetPosition.y <= homeRoom.yMax);
	}

	public Vector2 PathfindToTarget()
	{
		Vector2 movement = Vector2.zero;
		if (target == null) return movement;

		Vector2 targetPosition = target.transform.position;
		RaycastHit2D[] hits = ThickLinecast.ThickLinecast2D(transform.position, targetPosition, colliderSize, wallLayerMask);
		if (hits.Length > 0) // dungeon wall in the way
		{
			if (shouldRecalculate) // if player or enemy moved to a new tile
			{
				waypoints = AStarPathfinding.FindPath(Vector2Int.FloorToInt(transform.position), Vector2Int.FloorToInt(target.transform.position), tiles, neighborCache);
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

		return movement;
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

	public void CheckIfShouldRecalculate()
	{
		Vector2Int currentPosition = Vector2Int.FloorToInt(transform.position);

		staggerCurrent = (staggerCurrent + 1) % staggerPer;
		if (staggerCurrent != staggerIndex && // recalculate on stagger frame
			currentPosition != currentTile && // on target tile = force recalculate
			(waypoints != null && waypoints.Count > 0)) // targeted directly last frame = force recalculate
			return;

		if (shouldRecalculate) return;

		Vector2Int currentTargetPosition = Vector2Int.FloorToInt(target.transform.position);
		if (lastTargetPosition != currentTargetPosition || lastPosition != currentPosition || currentPosition == currentTile)
		{
			lastTargetPosition = currentTargetPosition;
			lastPosition = currentPosition;
			shouldRecalculate = true;
		}
	}
}
