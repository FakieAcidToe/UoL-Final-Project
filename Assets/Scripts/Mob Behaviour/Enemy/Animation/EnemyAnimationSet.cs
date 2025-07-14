using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Animation Set", menuName = "Animations/Enemy Animation Set")]
public class EnemyAnimationSet : ScriptableObject
{
	[Header("Sprite Properties")]
	public bool isFacingRight = true;

	[Header("Idle")]
	public Sprite[] idle;
	public float idleSpeed = 0.06f;

	[Header("Run")]
	public Sprite[] run;
	public float runSpeed = 0.06f;

	[Header("Sparable")]
	public Sprite[] sparable;
	public float sparableSpeed = 0.06f;
}