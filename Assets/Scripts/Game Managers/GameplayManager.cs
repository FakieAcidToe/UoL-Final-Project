using UnityEngine;

public class GameplayManager : MonoBehaviour
{
	[SerializeField] GameObject playerObj;
	[SerializeField] AbstractDungeonGenerator dungeonGenerator;
	[SerializeField] CameraFollow2D cameraObj;

	public void GenerateLevel()
	{
		dungeonGenerator.GenerateDungeon();

		playerObj.transform.position = (Vector3)dungeonGenerator.GetSpawnLocation();
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