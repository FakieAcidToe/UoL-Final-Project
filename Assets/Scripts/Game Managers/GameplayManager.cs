using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
	[Header("Scene References")]
	[SerializeField] AbstractDungeonGenerator dungeonGenerator;
	[SerializeField] CameraFollow2D cameraObj;

	[Header("Prefabs")]
	[SerializeField] GameObject playerPrefab;
	[SerializeField] Enemy enemyPrefab;

	// scene obj references
	GameObject playerObj;
	List<Enemy> enemyObjs;

	void Awake()
	{
		enemyObjs = new List<Enemy>();
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
			SpawnEnemy(dungeonGenerator.floorPositions);
		}
		else if (dungeonWithRooms.roomsList != null) // has rooms
		{
			Vector2 playerPos = playerObj.transform.position;
			bool skippedPlayer = false;

			foreach (BoundsInt room in dungeonWithRooms.roomsList)
				if (skippedPlayer || playerPos.x < room.xMin || playerPos.x > room.xMax || playerPos.y < room.yMin || playerPos.y > room.yMax) // don't spawn in player room
				{
					SpawnEnemy(dungeonGenerator.floorPositions, room);
				}
				else
					skippedPlayer = true;
		}
	}

	Enemy SpawnEnemy(HashSet<Vector2Int> floorTiles, BoundsInt room)
	{
		// spawn in room only
		HashSet<Vector2Int> tilesInRoom = new HashSet<Vector2Int>();
		foreach (Vector2Int tile in floorTiles)
			if (tile.x >= room.xMin && tile.x < room.xMax && tile.y >= room.yMin && tile.y < room.yMax)
				tilesInRoom.Add(tile);

		Enemy enemy = SpawnEnemy(tilesInRoom);
		enemy.homeRoom = room;
		return enemy;
	}

	Enemy SpawnEnemy(HashSet<Vector2Int> floorTiles)
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

		Enemy enemy = SpawnEnemy(randomPosition + dungeonGenerator.GetTilemapOfset());
		return enemy;
	}

	Enemy SpawnEnemy(Vector2 location)
	{
		Enemy enemy = Instantiate(enemyPrefab, location, Quaternion.identity);
		enemyObjs.Add(enemy);
		enemy.target = playerObj;
		enemy.tiles = dungeonGenerator.floorPositions;
		enemy.mapOffset = dungeonGenerator.GetTilemapOfset();
		enemy.neighborCache = dungeonGenerator.neighborCache;
		return enemy;
	}

	public void DespawnEnemies()
	{
		if (enemyObjs == null) return;
		else
		{
			for (int i = enemyObjs.Count-1; i >= 0; --i)
			{
				Enemy enemy = enemyObjs[i];
				Destroy(enemy.gameObject);
			}

			enemyObjs.Clear();
		}
	}
}