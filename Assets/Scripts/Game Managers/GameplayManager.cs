using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
	[Header("Scene References")]
	[SerializeField] AbstractDungeonGenerator dungeonGenerator;
	[SerializeField] CameraFollow2D cameraObj;

	[Header("Prefabs")]
	[SerializeField] GameObject playerPrefab;
	[SerializeField] GameObject enemyPrefab;

	// scene obj references
	GameObject playerObj;
	List<GameObject> enemyObjs;

	void Awake()
	{
		enemyObjs = new List<GameObject>();
	}

	void Start()
	{
		GenerateLevel();
	}

	public void GenerateLevel()
	{
		DespawnEnemies();

		dungeonGenerator.GenerateDungeon();
		SpawnPlayer();
		SpawnEnemies();
	}

	public void SpawnPlayer()
	{
		SpawnPlayer(dungeonGenerator.GetSpawnLocation());
	}

	void SpawnPlayer(Vector2 location)
	{
		if (playerObj == null) playerObj = Instantiate(playerPrefab, location, Quaternion.identity);
		else playerObj.transform.position = location;

		cameraObj.target = playerObj.transform;
		cameraObj.SetPositionToTarget();
	}

	public void SpawnEnemies()
	{
		RoomFirstDungeonGenerator dungeonWithRooms = dungeonGenerator.GetComponent<RoomFirstDungeonGenerator>();
		if (dungeonWithRooms == null) // no rooms
		{
			SpawnEnemies(dungeonGenerator.floorPositions);
		}
		else if (dungeonWithRooms.roomsList != null) // has rooms
		{
			foreach (BoundsInt room in dungeonWithRooms.roomsList)
			{
				SpawnEnemies(dungeonGenerator.floorPositions, room);
			}
		}
	}

	void SpawnEnemies(HashSet<Vector2Int> floorTiles, BoundsInt room)
	{
		// spawn in room only
		HashSet<Vector2Int> tilesInRoom = new HashSet<Vector2Int>();
		foreach (Vector2Int tile in floorTiles)
			if (tile.x >= room.xMin && tile.x < room.xMax && tile.y >= room.yMin && tile.y < room.yMax)
				tilesInRoom.Add(tile);

		SpawnEnemies(tilesInRoom);
	}

	void SpawnEnemies(HashSet<Vector2Int> floorTiles)
	{
		// random tile in floorTiles
		int index = Random.Range(0, floorTiles.Count);
		int i = 0;
		Vector2Int randomPosition = new Vector2Int();
		foreach (Vector2Int pos in floorTiles)
		{
			if (i == index)
			{
				randomPosition = pos;
				break;
			}
			++i;
		}

		SpawnEnemy(randomPosition + dungeonGenerator.GetTilemapOfset());
	}

	void SpawnEnemy(Vector2 location)
	{
		GameObject enemy = Instantiate(enemyPrefab, location, Quaternion.identity);
		enemyObjs.Add(enemy);
	}

	public void DespawnEnemies()
	{
		if (enemyObjs == null) return;
		else
		{
			foreach (GameObject enemy in enemyObjs)
				Destroy(enemy);
			enemyObjs.Clear();
		}
	}
}