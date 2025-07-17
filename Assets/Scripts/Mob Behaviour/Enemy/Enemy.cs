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
	EnemyState state = EnemyState.idle;

	[Header("Animation")]
	[SerializeField] EnemyAnimSetLoader animLoader;

	[Header("Stats")]
	[SerializeField] EnemyStats stats;
	Vector2 movement;
	[SerializeField] HealthbarUI healthbar;
	UIFader uiFader;
	int hp = 10;
	int circlesDrawn = 0;
	float spareTimer = 0;
	PlayerMovement controllingPlayer; // not null if being controlled by player
	bool canCapture = true;

	[Header("Pathfinding")]
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

	//[Header("Map Reference")]
	[HideInInspector] public BoundsInt homeRoom;
	[HideInInspector] public HashSet<Vector2Int> tiles;
	[HideInInspector] public Vector2 mapOffset;
	[HideInInspector] public Dictionary<Vector2Int, List<Vector2Int>> neighborCache;

	Circleable circle;
	Rigidbody2D rb;
	Collider2D enemyCollider;

	void Awake()
	{
		circle = GetComponent<Circleable>();
		rb = GetComponent<Rigidbody2D>();

		hp = stats.maxHp;
		healthbar.SetHealth(hp, false);
		healthbar.SetMaxHealth(hp, false);
		uiFader = healthbar.GetComponent<UIFader>();

		enemyCollider = GetComponent<Collider2D>();
		colliderSize = enemyCollider.bounds.size.x;

		animLoader.SetAnimations(stats.animationSet);
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
		switch (state)
		{
			default:
			case EnemyState.idle:
			case EnemyState.chase:
				if (controllingPlayer == null)
					PathfindToTarget();
				else
					PlayerMovement();
				break;
			case EnemyState.spared:
				if (controllingPlayer != null)
					ChangeState(EnemyState.idle);
				UnSpare();
				break;
		}
	}

	void FixedUpdate()
	{
		// move the player using physics
		rb.MovePosition(rb.position + movement * (controllingPlayer == null ? stats.moveSpeed : stats.playerMoveSpeed) * Time.fixedDeltaTime);
		animLoader.SetFlipX(movement);

		CheckIfShouldRecalculate();
	}

	void OnDrawGizmosSelected()
	{
		if (controllingPlayer == null)
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
	}

	void PlayerMovement()
	{
		movement.x = Input.GetAxisRaw("Horizontal");
		movement.y = Input.GetAxisRaw("Vertical");

		// Normalize diagonal movement
		if (movement.sqrMagnitude > 1) movement.Normalize();

		// set animation if moving
		ChangeState(movement.sqrMagnitude > 0 ? EnemyState.chase : EnemyState.idle);
		animLoader.SetFlipX(movement);

		if (Input.GetMouseButtonDown(2) || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Q))
		{
			StopControlling();
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

	void UnSpare()
	{
		movement = Vector2.zero;

		spareTimer += Time.deltaTime;
		if (spareTimer > stats.spareTime)
		{
			spareTimer = 0;
			canCapture = true;
			ChangeState(EnemyState.chase);
		}
	}

	public void StartControlling(PlayerMovement player)
	{
		if (!player.gameObject.activeSelf) return;

		ChangeState(EnemyState.idle);

		controllingPlayer = player;
		controllingPlayer.transform.SetParent(transform);
		controllingPlayer.transform.localPosition = Vector2.zero;
		controllingPlayer.gameObject.SetActive(false);

		circle.DisableLineDrawer();

		if (uiFader.GetCurrentAlpha() > 0)
			uiFader.FadeOutCoroutine();
	}

	public void StopControlling()
	{
		ChangeState(EnemyState.spared);

		controllingPlayer.transform.SetParent(null);
		controllingPlayer.gameObject.SetActive(true);
		controllingPlayer = null;

		canCapture = false;
		circle.EnableLineDrawer();

		if (hp < stats.maxHp && uiFader.GetCurrentAlpha() == 0)
			uiFader.FadeInCoroutine();
	}

	public void TakeDamage(int damage)
	{
		hp = Mathf.Clamp(hp - damage, 0, stats.maxHp);
		healthbar.SetHealth(hp);

		if (!IsBeingControlledByPlayer())
		{
			if (hp >= stats.maxHp && uiFader.GetCurrentAlpha() > 0)
				uiFader.FadeOutCoroutine();
			else if (hp < stats.maxHp && uiFader.GetCurrentAlpha() == 0)
				uiFader.FadeInCoroutine();
		}
	}

	void OnTriggerStay2D(Collider2D collision)
	{
		if (state == EnemyState.spared && canCapture)
		{
			PlayerMovement potentialPlayer = collision.gameObject.GetComponent<PlayerMovement>();
			if (controllingPlayer == null && potentialPlayer != null)
				StartControlling(potentialPlayer);
		}
	}

	void OnFullCircle()
	{
		canCapture = true;

		if (state == EnemyState.idle) // awaken enemy
			ChangeState(EnemyState.chase);
		else if (state == EnemyState.spared) // respare
			spareTimer = 0;

		if (++circlesDrawn >= stats.numOfCirclesToCapture)
		{
			circlesDrawn = 0;
			ChangeState(EnemyState.spared);
		}

		TakeDamage(1);
	}

	void OnCircleCollide()
	{
		circlesDrawn = 0;

		if (state == EnemyState.idle)
			ChangeState(EnemyState.chase);

		TakeDamage(-1);
	}

	void ChangeState(EnemyState newState)
	{
		if (state != newState && animLoader != null)
		{
			state = newState;

			enemyCollider.isTrigger = false;
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
					enemyCollider.isTrigger = true;
					animLoader.ChangeState(EnemyAnimSetLoader.EnemyAnimState.spare);
					break;
			}
		}
	}

	public bool IsBeingControlledByPlayer()
	{
		return controllingPlayer != null;
	}
}