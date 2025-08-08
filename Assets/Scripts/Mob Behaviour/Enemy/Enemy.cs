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
		attack,
		hurt,
		dead,
		screenTransition
	}
	public EnemyState state { private set; get; }

	[Header("CPU behaviour")]
	public GameObject target;

	[Header("Stats")]
	public EnemyStats stats;

	// movement
	public PlayerInputActions controls { private set; get; }
	Vector2 movement;
	float hitstun = 0; // time left in hitstun (cant move)
	public float hitpause { private set; get; }
	Vector2 knockback; // knockback to apply after hitpause

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
	public Collider2D enemyCollider { private set; get; }

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

		captureCircleUI.fillAmount = 0;

		controls = KeybindLoader.GetNewInputActions();
	}

	void Start()
	{
		animations.SetAnimations(stats.animationSet);
		attack.SetAttackGrid(stats.attackGrid);

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
		controls.Gameplay.Enable();
	}

	void OnDisable()
	{
		if (circle != null)
		{
			circle.onFullCircle.RemoveListener(OnFullCircle);
			circle.onCircleCollide.RemoveListener(OnCircleCollide);
		}
		controls.Gameplay.Disable();
	}

	void Update()
	{
		if (hitpause > 0)
		{
			hitpause -= Time.deltaTime;
			if (hitpause <= 0)
			{
				hitpause = 0;
				if (knockback != Vector2.zero)
				{
					enemyCollider.isTrigger = false;
					rb.AddForce(knockback, ForceMode2D.Impulse);
					knockback = Vector2.zero;
				}
			}
		}

		if (hitpause <= 0)
		{
			switch (state)
			{
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
					Attack();
					break;
				case EnemyState.hurt:
					movement = Vector2.zero;
					hitstun -= Time.deltaTime;

					if (health.hp <= 0)
						ChangeState(EnemyState.dead);
					else if (hitstun <= 0)
					{
						if (controllingPlayer == null)
							ChangeState(EnemyState.chase);
						else
							ChangeState(EnemyState.idle);
						hitstun = 0;
					}
					break;
				case EnemyState.screenTransition:
					movement = Vector2.zero;
					break;
			}
		}

		UpdateCaptureCircle();
	}

	void FixedUpdate()
	{
		if (hitpause <= 0 && state != EnemyState.hurt && state != EnemyState.dead)
		{
			// move the player using physics
			rb.MovePosition(rb.position + movement * (controllingPlayer == null ? stats.moveSpeed : stats.playerMoveSpeed) * Time.fixedDeltaTime);

			if (state == EnemyState.idle || state == EnemyState.chase)
				pathfinding.CheckIfShouldRecalculate();
		}
	}

	void EnemyMovement()
	{
		movement = Vector2.zero;

		// start chasing when necessary
		if (state == EnemyState.idle && pathfinding.ShouldStartChasing())
			ChangeState(EnemyState.chase);

		// chase with pathfinding
		if (state == EnemyState.chase)
		{
			movement = pathfinding.PathfindToTarget();
			animations.SetFlipX(movement);
		}

		if (state != EnemyState.attack)
		{
			if (attack.CPUShouldAttack()) // attack
				ChangeState(EnemyState.attack);
		}
	}

	void PlayerMovement()
	{
		movement = controls.Gameplay.Move.ReadValue<Vector2>();

		// Normalize diagonal movement
		if (movement.sqrMagnitude > 1) movement.Normalize();

		// set animation if moving
		ChangeState(movement.sqrMagnitude > 0 ? EnemyState.chase : EnemyState.idle);
		animations.SetFlipX(movement);

		if (state != EnemyState.attack)
		{
			if (controls.Gameplay.Eject.WasPressedThisFrame()) // eject
				StopControlling();
			else if (controls.Gameplay.Attack.WasPressedThisFrame()) // attack
				ChangeState(EnemyState.attack);
		}
	}

	void Attack()
	{
		movement = Vector2.zero;

		if (attack.AttackUpdate()) // true if attack end
		{
			if (controllingPlayer == null)
				ChangeState(EnemyState.chase);
			else
				ChangeState(EnemyState.idle);
		}
	}

	void UnSpare()
	{
		movement = Vector2.zero;
		enemyCollider.isTrigger = true;

		if (controls.Gameplay.Eject.WasPressedThisFrame())
			canCapture = true;

		spareTimer += Time.deltaTime;
		if (spareTimer > stats.spareTime)
		{
			spareTimer = 0;
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

		LineDrawer.Instance.enabled = false;
		LineDrawer.Instance.ResetPoints();

		health.OnStartControlling();

		SetCirclesDrawn(0, alsoSetLerp: true);
	}

	public void StopControlling()
	{
		ChangeState(EnemyState.spared);

		controllingPlayer.transform.SetParent(null);
		controllingPlayer.gameObject.SetActive(true);
		controllingPlayer = null;

		spareTimer = 0;
		canCapture = false;
		LineDrawer.Instance.enabled = true;

		health.OnStopControlling();
	}

	public void TakeDamage(int damage)
	{
		int overflowDamage = health.OnTakeDamage(damage);
		if (overflowDamage > 0)
		{
			controllingPlayer.TakeDamage(overflowDamage);
			StopControlling();
		}
	}

	public void ReceiveKnockback(Vector2 _force, float _hitstun, float _hitpause)
	{
		hitstun = _hitstun * stats.hitstunAdj;
		if (hitstun > 0)
		{
			ChangeState(EnemyState.hurt);
			animations.SetFlipX(_force * -1);
		}

		knockback = _force * stats.knockbackAdj;
		if (_hitpause <= 0)
			rb.AddForce(knockback, ForceMode2D.Impulse);
		else
			ApplyHitpause(_hitpause);

		SetCirclesDrawn(0);
	}

	public void ApplyHitpause(float _hitpause)
	{
		hitpause = Mathf.Max(_hitpause, hitpause);
		rb.velocity = Vector2.zero;
		enemyCollider.isTrigger = true;
	}

	public void Die() // gets called once enemy finishes fading out
	{
		gameObject.SetActive(false);
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
		if (state == EnemyState.spared)
		{
			if (stats.spareTime > 0)
				captureCircleUI.fillAmount = EaseUtils.EaseOutQuad(1 - spareTimer / stats.spareTime);
		}
		else if (shouldLerpCircles && stats.numOfCirclesToCapture > 0)
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
		if (state == EnemyState.spared)
		{
			// heal enemy on circle draw
			TakeDamage(-Mathf.CeilToInt(stats.maxHp * stats.healPercent));
			if (health.hp >= stats.maxHp) canCapture = true;

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
				{
					canCapture = true;
					ChangeState(EnemyState.spared);
				}
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
	}

	public void ScreenTransitionState()
	{
		if (controllingPlayer == null)
			ChangeState(EnemyState.screenTransition);
	}

	void ChangeState(EnemyState newState)
	{
		if (state != newState && animations != null)
		{
			if (state == EnemyState.attack)
				attack.AttackInterrupt();

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
					animations.ChangeState(EnemyAnimations.EnemyAnimState.custom);
					attack.AttackStart();
					break;
				case EnemyState.hurt:
					animations.ChangeState(EnemyAnimations.EnemyAnimState.hurt);
					break;
				case EnemyState.dead:
					animations.ChangeState(EnemyAnimations.EnemyAnimState.die);
					break;
				case EnemyState.screenTransition:
					break;
			}
		}
	}

	public bool IsBeingControlledByPlayer()
	{
		return controllingPlayer != null;
	}

	public PlayerMovement GetControllingPlayer()
	{
		return controllingPlayer;
	}

	public void SetMovement(Vector2 newMovement)
	{
		movement = newMovement;
	}
}