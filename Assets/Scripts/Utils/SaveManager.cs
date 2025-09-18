using UnityEngine;
using UnityEngine.Events;

public class SaveManager : MonoBehaviour
{
	[System.Serializable]
	public class SaveData // save data that should last through sessions
	{
		// settings
		public float musicVolume = 15f;
		public float sfxVolume = 100f;
		public float feedbackDuration = 1f;
		public float screenshake = 1f;
		public float damageInflation = 100f;

		// gameplay save
		public bool[] unlockedMonsters = new bool[4];
		public bool[] unlockedItems = new bool[6];
	}

	[System.Serializable]
	public class MiscData // won't be saved, but can be smuggled through scenes
	{
		// charselect data
		public int selectedCharacter = 0;
		public int selectedUpgrade = 0;
		public int difficulty = 3;

		// play data
		public bool win = false;
		public int numEnemiesCaptured = 0;
		public int numEnemiesKilled = 0;
		public int currentPlayCharacter = 0;
		public int levelsCleared = 0;
	}

	public static SaveManager Instance { get; private set; }
	const string SaveKey = "save_data";
	public UnityEvent onChangeBindings; // invoked when key bindings are updated

	public SaveData CurrentSaveData { get; private set; } = new SaveData();
	public MiscData CurrentMiscData { get; private set; } = new MiscData();

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		Load();
	}

	public void SceneChanged()
	{
		onChangeBindings.RemoveAllListeners();
	}

	public void Save()
	{
		string json = JsonUtility.ToJson(CurrentSaveData, true);
		PlayerPrefs.SetString(SaveKey, json);
		PlayerPrefs.Save();
	}

	public void Load()
	{
		if (PlayerPrefs.HasKey(SaveKey))
		{
			string json = PlayerPrefs.GetString(SaveKey);
			CurrentSaveData = JsonUtility.FromJson<SaveData>(json);
		}
		else
		{
			CurrentSaveData = new SaveData();
		}
	}

	public void ResetData()
	{
		CurrentSaveData = new SaveData();
		PlayerPrefs.DeleteKey(SaveKey);
		PlayerPrefs.Save();
	}

	// default settings
	public void ResetSettings()
	{
		CurrentSaveData.musicVolume = 15f;
		CurrentSaveData.sfxVolume = 100f;
		CurrentSaveData.feedbackDuration = 1f;
		CurrentSaveData.screenshake = 1f;
		CurrentSaveData.damageInflation = 100f;
	}

	public void ResetPlayData()
	{
		CurrentMiscData.win = false;
		CurrentMiscData.numEnemiesCaptured = 0;
		CurrentMiscData.numEnemiesKilled = 0;
		CurrentMiscData.currentPlayCharacter = 0;
		CurrentMiscData.levelsCleared = 0;
	}

	public void UnlockEverything()
	{
		for (int i = 0; i < CurrentSaveData.unlockedMonsters.Length; ++i)
			CurrentSaveData.unlockedMonsters[i] = true;

		for (int i = 0; i < CurrentSaveData.unlockedItems.Length; ++i)
			CurrentSaveData.unlockedItems[i] = true;
	}
}