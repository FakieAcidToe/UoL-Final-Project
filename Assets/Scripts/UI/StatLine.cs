using UnityEngine;
using UnityEngine.UI;

public class StatLine : MonoBehaviour
{
	[SerializeField] Text text;
	[SerializeField] StatValue statValue = StatValue.numEnemiesCaptured;

	enum StatValue
	{
		numEnemiesCaptured,
		numEnemiesKilled,
		levelsCleared
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
				return "Captured " +
					(SaveManager.Instance == null ? "0" : SaveManager.Instance.CurrentMiscData.numEnemiesCaptured.ToString()) +
					" Monster" + ((SaveManager.Instance == null || SaveManager.Instance.CurrentMiscData.numEnemiesCaptured != 1) ? "s" : "");
			case StatValue.numEnemiesKilled:
				return "Defeated " +
					(SaveManager.Instance == null ? "0" : SaveManager.Instance.CurrentMiscData.numEnemiesKilled.ToString()) +
					" Monster" + ((SaveManager.Instance == null || SaveManager.Instance.CurrentMiscData.numEnemiesKilled != 1) ? "s" : "");
			case StatValue.levelsCleared:
				return "Cleared " +
					(SaveManager.Instance == null ? "0" : SaveManager.Instance.CurrentMiscData.levelsCleared.ToString()) +
					" Level" + ((SaveManager.Instance == null || SaveManager.Instance.CurrentMiscData.levelsCleared != 1) ? "s" : "");
		}
	}

	void OnValidate()
	{
		Start();
	}
}
