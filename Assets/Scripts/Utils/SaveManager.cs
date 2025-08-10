using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
	[System.Serializable]
	public class SaveData // save data that should last through sessions
	{
		// settings
		public float musicVolume = 100f;
		public float sfxVolume = 100f;
		public float feedbackDuration = 1f;
		public float screenshake = 1f;
		public float damageInflation = 1f;

		// gameplay save
		public bool[] unlockedMonsters = new bool[3];
	}
	[System.Serializable]
	public class MiscData // won't be saved, but can be smuggled through scenes
	{
		public int selectedCharacter = 0;
	}

	public static SaveManager Instance { get; private set; }
	string savePath;

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		savePath = Path.Combine(Application.persistentDataPath, "save.json");
		Load();
	}

	public SaveData CurrentSaveData { get; private set; } = new SaveData();
	public MiscData CurrentMiscData { get; private set; } = new MiscData();

	public void Save()
	{
		string json = JsonUtility.ToJson(CurrentSaveData, true);
		File.WriteAllText(savePath, json);
		//Debug.Log("Data saved to: " + savePath);
	}

	public void Load()
	{
		if (File.Exists(savePath))
		{
			string json = File.ReadAllText(savePath);
			CurrentSaveData = JsonUtility.FromJson<SaveData>(json);
			//Debug.Log("Data loaded from: " + savePath);
		}
		else
		{
			//Debug.Log("No save file found. Using default values.");
			CurrentSaveData = new SaveData();
		}
	}

	public void ResetData()
	{
		CurrentSaveData = new SaveData();
		if (File.Exists(savePath)) File.Delete(savePath);
	}
}