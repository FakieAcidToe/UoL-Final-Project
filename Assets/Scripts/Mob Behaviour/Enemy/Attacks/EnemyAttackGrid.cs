using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Attack", menuName = "Attack/Enemy Attacks")]
public class EnemyAttackGrid : ScriptableObject
{
	[Header("Attack Windows")]
	public Window[] windows;

	[System.Serializable]
	public struct Window
	{
		public string name;
		public Sprite[] sprites;
		[Min(0)] public float animSpeed;
		public bool chargeable;
	}
}