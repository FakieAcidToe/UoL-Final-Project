using UnityEngine;

[CreateAssetMenu(fileName = "Stat Upgrade Item", menuName = "Items/Stat Upgrade Item")]
public class UpgradeStatItem : PowerUpItem
{
	[SerializeField, Min(0)] float attackMult = 1;
	[SerializeField, Min(0)] float reveiveAttackMult = 1;
	[SerializeField, Min(0)] int catchMult = 1;

	public override void PickUpItem(ItemUser self)
	{
		PlayerUpgradeStats stats = GetStatsFromItemUser(self);
		if (stats != null)
		{
			stats.attackMult = attackMult;
			stats.reveiveAttackMult = reveiveAttackMult;
			stats.catchMult = catchMult;
		}
	}

	public override void DropItem(ItemUser self)
	{
		PlayerUpgradeStats stats = GetStatsFromItemUser(self);

		if (stats != null)
			stats.ResetStats();
	}

	PlayerUpgradeStats GetStatsFromItemUser(ItemUser self)
	{
		PlayerUpgradeStats stats = null;

		// grab playerstats from itemuser
		Enemy enemy = self.GetComponent<Enemy>();
		if (enemy != null)
			stats = enemy.playerStats;
		else
		{
			PlayerMovement player = self.GetComponent<PlayerMovement>();
			if (player != null)
				stats = player.playerStats;
		}

		return stats;
	}
}
