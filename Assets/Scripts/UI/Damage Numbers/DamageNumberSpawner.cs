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

	void Start()
	{
		inflationMultiplier = SaveManager.Instance.CurrentSaveData.damageInflation;
	}

	public void SpawnDamageNumbers(int damageNumber, Vector2 worldPosition)
	{
		int finalNumber = Mathf.FloorToInt(damageNumber * inflationMultiplier);
		if (finalNumber > 0)
		{
			DamageNumberCanvas damageNum = Instantiate(damageNumberPrefab, worldPosition, Quaternion.identity, transform);
			damageNum.SetDamageNumberText(finalNumber);
		}
	}
}