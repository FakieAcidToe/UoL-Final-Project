using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Attack", menuName = "Attack/Abstract Enemy Attack")]
public class EnemyAttackGrid : ScriptableObject
{
	[Header("Attack Windows")]
	public Window[] windows;

	[System.Serializable]
	public struct Window
	{
		public string name;
		public Sprite[] sprites;
		[Min(0)] public float windowLength;
		public bool chargeable;
	}

	// runs when starting an attack
	public virtual void AttackStart(Enemy self) { }

	// runs every frame of the attack
	public virtual void AttackUpdate(Enemy self, int window, float windowTimer, float chargeTimer) { }
}