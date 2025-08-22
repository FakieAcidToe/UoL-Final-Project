using UnityEngine;
using UnityEngine.UI;

public class StatLine : MonoBehaviour
{
	[SerializeField] Text text;
	[SerializeField] StatValue statValue = StatValue.numEnemiesCaptured;

	enum StatValue
	{
		numEnemiesCaptured,
		numEnemiesKilled
	}

	void Start()
	{
		if (text != null)
			text.text = GetStatValueString();
	}

	string GetStatValueString()
	{
		switch (statValue)
		{
			default:
				return "";
			case StatValue.numEnemiesCaptured:
				return "Captured " + (SaveManager.Instance == null ? "0" : SaveManager.Instance.CurrentMiscData.numEnemiesCaptured.ToString()) + " Monsters";
			case StatValue.numEnemiesKilled:
				return "Defeated " + (SaveManager.Instance == null ? "0" : SaveManager.Instance.CurrentMiscData.numEnemiesKilled.ToString()) + " Monsters";
		}
	}

	void OnValidate()
	{
		Start();
	}
}
