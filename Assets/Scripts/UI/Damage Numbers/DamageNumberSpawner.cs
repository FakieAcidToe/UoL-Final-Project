using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
	public static DamageNumberSpawner Instance { private set; get; }
	[SerializeField] DamageNumberCanvas damageNumberPrefab;
	[SerializeField] float inflationMultiplier = 15;

	void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(this);
	}

	public void SpawnDamageNumbers(int damageNumber, Vector2 worldPosition)
	{
		DamageNumberCanvas damageNum = Instantiate(damageNumberPrefab, worldPosition, Quaternion.identity, transform);
		damageNum.SetDamageNumberText(Mathf.FloorToInt(damageNumber * inflationMultiplier));
	}
}