using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Animation Set", menuName = "Animation/Enemy Animation Set")]
public class EnemyAnimationSet : ScriptableObject
{
	public Sprite[] idle;
	public float idleSpeed = 0.06f;
}