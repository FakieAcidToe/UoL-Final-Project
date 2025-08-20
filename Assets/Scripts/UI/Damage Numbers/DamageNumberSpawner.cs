using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
	public static DamageNumberSpawner Instance { private set; get; }
	[SerializeField] DamageNumberCanvas damageNumberPrefab;
	[SerializeField] string sortingLayerName = "Capture Line";

	void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(this);
	}

	public void SpawnDamageNumbers(int damageNumber, Vector2 worldPosition)
	{
		int finalNumber = Mathf.FloorToInt(damageNumber * SaveManager.Instance.CurrentSaveData.damageInflation);
		if (finalNumber > 0)
		{
			DamageNumberCanvas damageNum = Instantiate(damageNumberPrefab, worldPosition, Quaternion.identity, transform);
			damageNum.SetDamageNumberText(finalNumber);
			damageNum.GetComponent<Canvas>().sortingLayerName = sortingLayerName;
		}
	}
}