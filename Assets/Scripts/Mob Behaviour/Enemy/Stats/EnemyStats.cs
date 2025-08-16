using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Stats", menuName = "Stats/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
	[Header("Name")]
	public string enemyName = "Enemy";
	public int id = 0;

	[Header("Animations")]
	public EnemyAnimationSet animationSet;

	[Header("Movement")]
	[Min(0)] public float moveSpeed = 2f;
	[Min(0)] public float playerMoveSpeed = 4f;

	[Header("Health")]
	[Min(0)] public int maxHp = 10;
	[Min(0)] public int hpScaling = 5;
	[Range(0, 1), Tooltip("How much of maxHp to heal on circle draw")] public float healPercent = 0.1f;

	[Header("Knockback/Hitstun Multipliers")]
	[Min(0)] public float knockbackAdj = 1f;
	[Min(0)] public float hitstunAdj = 1f;

	[Header("Capture")]
	[Min(0)] public int numOfCirclesToCapture = 5;
	[Min(0), Tooltip("How long enemy stays spared for")] public float spareTime = 3f;

	[Header("XP Drops")]
	[Min(0)] public int xpDropAmount = 1;

	[Header("Attacks")]
	public EnemyAttackGrid[] attackGrid;

	[Header("Boss Variant")]
	public EnemyStats bossVariant;
}