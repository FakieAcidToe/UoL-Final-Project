using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Stats", menuName = "Stats/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
	[Header("Animations")]
	public EnemyAnimationSet animationSet;

	[Header("Movement")]
	[Min(0)] public float moveSpeed = 2f;
	[Min(0)] public float playerMoveSpeed = 4f;

	[Header("Health")]
	[Min(0)] public int maxHp = 10;

	[Header("Capture")]
	[Min(0)] public int numOfCirclesToCapture = 3;
	[Min(0), Tooltip("How long enemy stays spared for")] public float spareTime = 3f;
}
