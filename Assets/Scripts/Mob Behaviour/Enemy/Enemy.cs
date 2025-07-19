using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Circleable)),
 RequireComponent(typeof(EnemyAnimations), typeof(EnemyHP), typeof(EnemyPathfinding)),
 RequireComponent(typeof(EnemyAttack))]
public class Enemy : MonoBehaviour
{
	public enum EnemyState
	{
		idle,
		chase,
		spared,
		attack
	}
	public EnemyState state { private set; get; }

	[Header("CPU behaviour")]
	public GameObject target;

	[Header("Stats")]
	[SerializeField] EnemyStats stats;
	Vector2 movement;

	[Header("Circles / Sparing")]
	[SerializeField] Image captureCircleUI;
	[SerializeField] ParticleSystem particleStars;
	PlayerMovement controllingPlayer; // not null if being controlled by player
	bool canCapture = true;
	int circlesDrawn = 0;
	bool shouldLerpCircles = false;
	float circlesDrawnLerp = 0;
	[SerializeField, Min(0)] float circleLerpSpeed = 15f;
	float spareTimer = 0;

	// generic components
	Circleable circle;
	Rigidbody2D rb;
	Collider2D enemyCollider;

	// enemy components
	public EnemyAnimations animations { private set; get; }
	public EnemyHP health { private set; get; }
	public EnemyPathfinding pathfinding { private set; get; }
	public EnemyAttack attack { private set; get; }

	void Awake()
	{
		state = EnemyState.idle;

		circle = GetComponent<Circleable>();
		rb = GetComponent<Rigidbody2D>();
		enemyCollider = GetComponent<Collider2D>();

		animations = GetComponent<EnemyAnimations>();
		health = GetComponent<EnemyHP>();
		pathfinding = GetComponent<EnemyPathfinding>();
		attack = GetComponent<EnemyAttack>();

		animations.SetAnimations(stats.animationSet);

		captureCircleUI.fillAmount = 0;
	}

	void Start()
	{
		// random flipx
		animations.SetFlipX(Vector2.right * Random.Range(-1f, 1f));
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
				if (controllingPlayer == null) EnemyMovement();
				else PlayerMovement();
				break;
			case EnemyState.spared:
				if (controllingPlayer != null)
					ChangeState(EnemyState.idle);
				UnSpare();
				break;
			case EnemyState.attack:
				if (controllingPlayer == null)
					Attack();
				break;
		}

		UpdateCaptureCircle();
	}

	void FixedUpdate()
	{
		// move the player using physics
		rb.MovePosition(rb.position + movement * (controllingPlayer == null ? stats.moveSpeed : stats.playerMoveSpeed) * Time.fixedDeltaTime);
		animations.SetFlipX(movement);

		pathfinding.CheckIfShouldRecalculate();
	}

	void EnemyMovement()
	{
		movement = Vector2.zero;

		// start chasing when necessary
		if (state == EnemyState.idle && pathfinding.ShouldStartChasing())
			ChangeState(EnemyState.chase);

		// chase with pathfinding
		if (state == EnemyState.chase)
			movement = pathfinding.PathfindToTarget();
	}

	void PlayerMovement()
	{
		movement.x = Input.GetAxisRaw("Horizontal");
		movement.y = Input.GetAxisRaw("Vertical");

		// Normalize diagonal movement
		if (movement.sqrMagnitude > 1) movement.Normalize();

		// set animation if moving
		ChangeState(movement.sqrMagnitude > 0 ? EnemyState.chase : EnemyState.idle);
		animations.SetFlipX(movement);

		if (Input.GetMouseButtonDown(2) || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Q))
		{
			StopControlling();
		}
	}

	void Attack()
	{
		
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
			SetCirclesDrawn(0, alsoSetLerp: true);
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

		health.OnStartControlling();

		SetCirclesDrawn(0, alsoSetLerp: true);
	}

	public void StopControlling()
	{
		ChangeState(EnemyState.spared);

		controllingPlayer.transform.SetParent(null);
		controllingPlayer.gameObject.SetActive(true);
		controllingPlayer = null;

		canCapture = false;
		circle.EnableLineDrawer();

		health.OnStopControlling();
	}

	public void TakeDamage(int damage)
	{
		health.OnTakeDamage(damage);
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

	int SetCirclesDrawn(int amount, bool relative = false, bool alsoSetLerp = false)
	{
		if (stats.numOfCirclesToCapture > 0)
		{
			if (relative) circlesDrawn += amount;
			else circlesDrawn = amount;
			if (alsoSetLerp)
			{
				circlesDrawnLerp = circlesDrawn;
				captureCircleUI.fillAmount = circlesDrawnLerp / stats.numOfCirclesToCapture;
			}
			else
				shouldLerpCircles = true;

			return circlesDrawn;
		}
		else return circlesDrawn;
	}

	void UpdateCaptureCircle()
	{
		if (shouldLerpCircles && stats.numOfCirclesToCapture > 0)
		{
			if (Mathf.Abs(circlesDrawnLerp - circlesDrawn) < 0.01f)
			{
				circlesDrawnLerp = circlesDrawn;
				shouldLerpCircles = false;
			}
			else
			{
				circlesDrawnLerp = Mathf.Lerp(circlesDrawnLerp, circlesDrawn, 1f - Mathf.Exp(-circleLerpSpeed * Time.deltaTime));
			}
			captureCircleUI.fillAmount = circlesDrawnLerp / stats.numOfCirclesToCapture;
		}
	}

	void OnFullCircle()
	{
		canCapture = true;

		if (state == EnemyState.spared)
		{
			// heal enemy on circle draw
			TakeDamage(-Mathf.CeilToInt(stats.maxHp * stats.healPercent));

			spareTimer = 0; // respare
			SetCirclesDrawn(stats.numOfCirclesToCapture);
		}
		else
		{
			// increase circles ui
			if (stats.numOfCirclesToCapture > 0)
			{
				particleStars.Play();
				if (SetCirclesDrawn(1, true) >= stats.numOfCirclesToCapture)
					ChangeState(EnemyState.spared);
			}
			
			if (state == EnemyState.idle) // awaken enemy
				ChangeState(EnemyState.chase);
		}
	}

	void OnCircleCollide()
	{
		if (state != EnemyState.spared)
			SetCirclesDrawn(0);
		if (state == EnemyState.idle)
			ChangeState(EnemyState.chase);

		TakeDamage(1);
	}

	void ChangeState(EnemyState newState)
	{
		if (state != newState && animations != null)
		{
			state = newState;

			enemyCollider.isTrigger = false;
			switch (state)
			{
				default:
				case EnemyState.idle:
					animations.ChangeState(EnemyAnimations.EnemyAnimState.idle);
					break;
				case EnemyState.chase:
					animations.ChangeState(EnemyAnimations.EnemyAnimState.run);
					break;
				case EnemyState.spared:
					enemyCollider.isTrigger = true;
					animations.ChangeState(EnemyAnimations.EnemyAnimState.spare);
					break;
				case EnemyState.attack:
					enemyCollider.isTrigger = true;
					animations.ChangeState(EnemyAnimations.EnemyAnimState.attack);
					break;
			}
		}
	}

	public bool IsBeingControlledByPlayer()
	{
		return controllingPlayer != null;
	}

	public EnemyStats GetStats()
	{
		return stats;
	}
}