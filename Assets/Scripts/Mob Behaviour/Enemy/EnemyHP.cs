using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyHP : MonoBehaviour
{
	[SerializeField] HealthbarUI healthbar; // healthbar above head
	UIFader uiFader;
	public int hp { get; private set; }
	[HideInInspector] public HealthbarUI healthbarUIPlayer; // top left healthbar ui
	[HideInInspector] public HealthbarUI healthbarUIMonster;

	Enemy enemy;

	void Awake()
	{
		enemy = GetComponent<Enemy>();
		hp = enemy.GetStats().maxHp;
		healthbar.SetHealth(hp, false);
		healthbar.SetMaxHealth(hp, false);
		uiFader = healthbar.GetComponent<UIFader>();
	}

	public void OnStartControlling()
	{
		// fade out
		if (uiFader.GetCurrentAlpha() > 0)
			uiFader.FadeOutCoroutine();

		// set monster's health
		healthbarUIMonster.SetMaxHealth(enemy.GetStats().maxHp, false);
		healthbarUIMonster.SetHealth(hp);
	}

	public void OnStopControlling()
	{
		// fade in
		if (hp < enemy.GetStats().maxHp && uiFader.GetCurrentAlpha() == 0)
			uiFader.FadeInCoroutine();

		// remove monster's health
		healthbarUIMonster.SetHealth(0);
	}

	public void OnTakeDamage(int damage)
	{
		if (!enemy.IsBeingControlledByPlayer())
		{
			hp = Mathf.Clamp(hp - damage, 0, enemy.GetStats().maxHp);
			healthbar.SetHealth(hp);

			if (hp >= enemy.GetStats().maxHp && uiFader.GetCurrentAlpha() > 0)
				uiFader.FadeOutCoroutine();
			else if (hp < enemy.GetStats().maxHp && uiFader.GetCurrentAlpha() == 0)
				uiFader.FadeInCoroutine();
		}
		else
		{
			int overflowDamage = Mathf.Max(damage - hp, -1);
			hp = Mathf.Clamp(hp - damage, 1, enemy.GetStats().maxHp); // leave at 1hp
			healthbar.SetHealth(hp, false);
			healthbarUIMonster.SetHealth(hp);

			if (overflowDamage >= 0) // eject on overflow
			{
				healthbarUIPlayer.SetHealthRelative(-overflowDamage);
				enemy.StopControlling();
			}
		}
	}
}