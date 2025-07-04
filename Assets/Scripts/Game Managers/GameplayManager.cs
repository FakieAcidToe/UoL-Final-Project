using UnityEngine;

public class GameplayManager : MonoBehaviour
{
	[Header("Scene References")]
	[SerializeField] AbstractDungeonGenerator dungeonGenerator;
	[SerializeField] CameraFollow2D cameraObj;

	[Header("Prefabs")]
	[SerializeField] GameObject playerPrefab;
	[SerializeField] GameObject enemyPrefab;

	GameObject playerObj;

	public void GenerateLevel()
	{
		dungeonGenerator.GenerateDungeon();

		Vector3 spawnLocation = dungeonGenerator.GetSpawnLocation();
		if (playerObj == null)
			playerObj = Instantiate(playerPrefab, spawnLocation, Quaternion.identity);
		else
			playerObj.transform.position = spawnLocation;
		cameraObj.target = playerObj.transform;
		cameraObj.SetPositionToTarget();
	}

	void Start()
	{
		GenerateLevel();
	}

	void Update()
	{
		
	}
}