using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyHP : MonoBehaviour
{
	[SerializeField] HealthbarUI healthbar; // healthbar above head
	UIFader uiFader;
	public int hp { get; private set; }
	[HideInInspector] public HealthbarUI healthbarUIMonster; // top left healthbar ui
	[HideInInspector] public HealthbarUI healthbarUIPlayer;

	Enemy enemy;

	void Awake()
	{
		enemy = GetComponent<Enemy>();
		uiFader = healthbar.GetComponent<UIFader>();
	}

	void Start()
	{
		hp = enemy.stats.maxHp;
		healthbar.SetHealth(hp, false);
		healthbar.SetMaxHealth(hp, false);
	}

	public void OnStartControlling()
	{
		// fade out
		if (uiFader.GetCurrentAlpha() > 0)
			uiFader.FadeOutCoroutine();

		// set monster's health
		healthbarUIMonster.SetMaxHealth(enemy.stats.maxHp, false);
		healthbarUIMonster.SetHealth(hp);

		healthbarUIPlayer.SetPortrait(enemy.animations.GetAnimations().portrait);
	}

	public void OnStopControlling()
	{
		// fade in
		if (hp < enemy.stats.maxHp && uiFader.GetCurrentAlpha() == 0)
			uiFader.FadeInCoroutine();

		// remove monster's health
		healthbarUIMonster.SetHealth(0);

		healthbarUIPlayer.SetPortrait();
	}

	public int OnTakeDamage(int damage) // returns overflow damage
	{
		if (!enemy.IsBeingControlledByPlayer())
		{
			hp = Mathf.Clamp(hp - damage, 0, enemy.stats.maxHp);
			healthbar.SetHealth(hp);

			if (hp >= enemy.stats.maxHp && uiFader.GetCurrentAlpha() > 0)
				uiFader.FadeOutCoroutine();
			else if (hp < enemy.stats.maxHp && uiFader.GetCurrentAlpha() == 0)
				uiFader.FadeInCoroutine();
		}
		else
		{
			int overflowDamage = Mathf.Max(damage - hp, -1);
			hp = Mathf.Clamp(hp - damage, 1, enemy.stats.maxHp); // leave at 1hp
			healthbar.SetHealth(hp, false);
			healthbarUIMonster.SetHealth(hp);

			if (overflowDamage >= 0) // eject on overflow
				return overflowDamage;
		}
		return 0;
	}
}