using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
	// Example data structure
	[System.Serializable]
	public class SaveData
	{
		public float musicVolume = 100f;
		public float sfxVolume = 100f;
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