using UnityEngine;

[CreateAssetMenu(fileName = "Player Animation Set", menuName = "Animations/Player Animation Set")]
public class PlayerAnimationSet : ScriptableObject
{
	[Header("Sprite Properties")]
	public bool isFacingRight = true;

	[Header("Idle")]
	public Sprite[] idle;
	public float idleSpeed = 0.06f;

	[Header("Run")]
	public Sprite[] run;
	public float runSpeed = 0.06f;

	[Header("Hurt")]
	public Sprite[] hurt;
	public float hurtSpeed = 0.06f;
}