using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerAnimations), typeof(ItemUser))]
public class PlayerMovement : MonoBehaviour
{
	enum PlayerState
	{
		idle,
		run,
		hurt,
		dead
	}

	PlayerState state = PlayerState.idle;
	PlayerAnimations playerAnimation;
	PlayerInputActions controls;

	[Header("Movement")]
	[SerializeField] float moveSpeed = 2f;
	public PlayerUpgradeStats playerStats;

	[Header("HP")]
	[SerializeField] int maxHp = 20;
	[SerializeField] int hpScaling = 5; // per level
	int hp;
	[HideInInspector] public HealthbarUI healthbar;

	[Header("XP")]
	int maxXp = 20;
	int xp = 0;
	[HideInInspector] public HealthbarUI xpbar;
	[SerializeField] XPCollector xpCollector;
	[SerializeField] string lvTextBeforeNumber = "Lv";
	[HideInInspector] public Text lvText;
	public int level { get; private set; }
	[SerializeField] int[] xpPerLevel; // length is max level
	[SerializeField] AudioClip lvUpSFX;
	[SerializeField] AudioClip xpSFX;

	[Header("Name")]
	[SerializeField] string playerName = "Charmer";
	[HideInInspector] public Text nameText;

	[Header("Events")]
	public UnityEvent onDeath;

	Rigidbody2D rb;

	// movement
	Vector2 movement;
	float hitstun = 0; // time left in hitstun (cant move)
	public float hitpause { private set; get; }
	Vector2 knockback; // knockback to apply after hitpause

	public Enemy controllingEnemy = null;
	public ItemUser itemUser { private set; get; }

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		playerAnimation = GetComponent<PlayerAnimations>();
		itemUser = GetComponent<ItemUser>();
		controls = KeybindLoader.GetNewInputActions();

		hp = maxHp;
		level = 1;
		if (xpPerLevel.Length > level - 1)
			maxXp = xpPerLevel[level - 1];
	}

	void OnEnable()
	{
		controls.Gameplay.Enable();
		if (xpCollector != null) xpCollector.OnCollectOrb.AddListener(GainXP);
	}

	void OnDisable()
	{
		controls.Gameplay.Disable();
		if (xpCollector != null) xpCollector.OnCollectOrb.RemoveListener(GainXP);
	}

	void Start()
	{
		healthbar.SetHealth(hp, false);
		healthbar.SetMaxHealth(hp, false);

		xpbar.SetHealth(xp, false);
		xpbar.SetMaxHealth(maxXp, false);
		UpdateLvText();
		SetName();
	}

	void Update()
	{
		if (Time.timeScale <= 0) return;

		if (hitpause > 0)
		{
			hitpause -= Time.deltaTime;
			if (hitpause <= 0)
			{
				hitpause = 0;
				if (knockback != Vector2.zero)
				{
					rb.AddForce(knockback, ForceMode2D.Impulse);
					knockback = Vector2.zero;
				}
			}
		}

		if (hitpause <= 0)
		{
			switch (state)
			{
				case PlayerState.idle:
				case PlayerState.run:
					movement = controls.Gameplay.Move.ReadValue<Vector2>();

					// Normalize diagonal movement
					if (movement.sqrMagnitude > 1) movement.Normalize();

					// set animation if moving
					ChangeState(movement.sqrMagnitude > 0 ? PlayerState.run : PlayerState.idle);
					playerAnimation.SetFlipX(movement);
					break;
				case PlayerState.hurt:
					movement = Vector2.zero;
					hitstun -= Time.deltaTime;

					if (hitstun <= 0)
					{
						ChangeState(PlayerState.idle);
						hitstun = 0;
					}
					break;
				case PlayerState.dead:
					movement = Vector2.zero;
					break;
			}
		}
	}

	void FixedUpdate()
	{
		if (state != PlayerState.hurt && state != PlayerState.dead)
		{
			// move the player using physics
			rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
		}
	}

	public void TakeDamage(int damage)
	{
		hp = Mathf.Clamp(hp - damage, 0, maxHp);
		healthbar.SetHealth(hp);
		// dead when hp 0
		if (hp <= 0 && state != PlayerState.dead)
		{
			ChangeState(PlayerState.dead);
			onDeath.Invoke();
		}
	}

	public void ReceiveKnockback(Vector2 _force, float _hitstun, float _hitpause)
	{
		hitstun = _hitstun;
		if (hitstun > 0)
		{
			if (state != PlayerState.dead)
				ChangeState(PlayerState.hurt);
			playerAnimation.SetFlipX(_force * -1);
		}

		knockback = _force;
		if (_hitpause <= 0)
			rb.AddForce(knockback, ForceMode2D.Impulse);
		else
			ApplyHitpause(_hitpause);
	}

	public void ApplyHitpause(float _hitpause)
	{
		hitpause = Mathf.Max(_hitpause, hitpause);
		rb.velocity = Vector2.zero;
	}

	void ChangeState(PlayerState newState)
	{
		if (state != newState && playerAnimation != null)
		{
			state = newState;
			switch (state)
			{
				default:
				case PlayerState.idle:
					playerAnimation.ChangeState(PlayerAnimations.PlayerAnimState.idle);
					break;
				case PlayerState.run:
					playerAnimation.ChangeState(PlayerAnimations.PlayerAnimState.run);
					break;
				case PlayerState.hurt:
				case PlayerState.dead:
					playerAnimation.ChangeState(PlayerAnimations.PlayerAnimState.hurt);
					break;
			}
		}
	}

	public void GainXP(int xpIncrease)
	{
		TakeDamage(-xpIncrease);
		PlaySFX(xpSFX);

		if (level >= xpPerLevel.Length) return;

		xp = Mathf.Max(xp + xpIncrease, 0);
		xpbar.SetHealth(xp);
		CheckXP();
	}

	void CheckXP()
	{
		if (xp >= maxXp && level < xpPerLevel.Length) // xpPerLevel.Length is max level
			xpbar.SetHealth(0, 1f, false, LevelUp); // wait for 1 second, then set xp bar to 0 and call LevelUp()
	}

	void LevelUp()
	{
		if (xp >= maxXp)
		{
			xp -= maxXp;
			level = Mathf.Min(level + 1, xpPerLevel.Length);
			UpdateLvText();
			PlaySFX(lvUpSFX);

			if (controllingEnemy != null) controllingEnemy.UpdateLvToController();

			maxXp = xpPerLevel[level - 1];
			xpbar.SetMaxHealth(maxXp, false);

			// gain maxhp and hp on levelup
			maxHp += hpScaling;
			healthbar.SetMaxHealth(maxHp);
			TakeDamage(-hpScaling);

			if (level >= xpPerLevel.Length) // if max level, also set xp to max
			{
				xp = maxXp;
				xpbar.SetHealth(xp, false);
			}
		}

		xpbar.SetHealth(xp);
		CheckXP(); // in case player can level up again after levelling up
	}

	void UpdateLvText()
	{
		lvText.text = lvTextBeforeNumber + ' ' + (level >= xpPerLevel.Length ? "MAX" : level.ToString());
	}

	public void SetName(string name = null)
	{
		if (nameText != null)
			nameText.text = name ?? playerName;
	}

	public void PlaySFX(AudioClip _clip)
	{
		SoundManager.Instance.Play(_clip);
	}

	public bool IsDead()
	{
		return state == PlayerState.dead;
	}

	public void SetItemIcon(Image itemIcon, Text itemControlsText, Text itemNameText)
	{
		itemUser.itemIcon = itemIcon;
		itemUser.itemControlsText = itemControlsText;
		itemUser.itemNameText = itemNameText;
	}
}