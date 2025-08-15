using UnityEngine;

[CreateAssetMenu(fileName = "Player Upgrade Stats", menuName = "Stats/Player Upgrade Stats")]
public class PlayerUpgradeStats : ScriptableObject
{
	[Min(0)] public float attackMult = 1;
	[Min(0)] public float reveiveAttackMult = 1;
	[Min(0)] public int catchMult = 1;

	public void ResetStats()
	{
		attackMult = 1;
		reveiveAttackMult = 1;
		catchMult = 1;
	}
}