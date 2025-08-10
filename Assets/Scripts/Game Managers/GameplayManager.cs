using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayManager : GeneralManager
{
	[Header("Scene References")]
	[SerializeField] AbstractDungeonGenerator dungeonGenerator;
	[SerializeField] CameraFollow2D cameraObj;
	[SerializeField] HealthbarUI healthbarMonster;
	[SerializeField] HealthbarUI healthbarPlayer;
	[SerializeField] HealthbarUI xpbar;
	[SerializeField] UIFader fadeOutScreen; // UI that has floor number
	[SerializeField] Text transitionText;
	[SerializeField] Text lvText;
	[SerializeField] Text nameText;

	[Header("Prefabs")]
	[SerializeField] PlayerMovement playerPrefab;
	[SerializeField] Enemy enemyPrefab;
	[SerializeField] PlayerPressurePlate exitPrefab;
	[SerializeField] PlayerPressurePlate keyPrefab;

	[Header("Dungeon Generation")]
	[SerializeField] DungeonParamsSO[] dungeonTypes;
	DungeonParamsSO currentDungeonParam;

	[Header("Sprites")]
	[SerializeField] Sprite exitLocked;
	[SerializeField] Sprite exitUnlocked;

	[Header("Screen Transition Settings")]
	[SerializeField] float transitionTextTime = 2f;
	[SerializeField] string textBeforeNumber = "Floor";

	[Header("Enemy Settings")]
	[SerializeField, Min(1)] int enemyStaggerMultiplier = 1;
	[SerializeField] EnemyStats[] enemyTypes;

	[Header("Change Scene Properties")]
	[SerializeField] int titleSceneIndex = 0;

	// scene obj references
	PlayerMovement playerObj;
	Enemy capturedEnemy; // the captured enemy object between dungeons
	List<Enemy> enemyObjs; // list of enemies in current dungeon
	PlayerPressurePlate dungeonExit;
	PlayerPressurePlate dungeonKey;

	bool collectedKey = false;
	int floorNumber = 0;

	protected override void Awake()
	{
		base.Awake();

		enemyObjs = new List<Enemy>();

		healthbarMonster.SetHealth(0, false);
	}

	void Start()
	{
		GenerateLevel();
	}

	public void GenerateLevel()
	{
		StartCoroutine(GenerateLevelCoroutine());
	}

	public IEnumerator GenerateLevelCoroutine()
	{
		++floorNumber;
		transitionText.text = textBeforeNumber + ' ' + floorNumber.ToString();

		foreach (Enemy enemy in enemyObjs)
			enemy.ScreenTransitionState();

		if (fadeOutScreen.GetCurrentAlpha() < 1f) fadeOutScreen.FadeInCoroutine();
		while (fadeOutScreen.GetCurrentAlpha() < 1f) yield return null;

		DespawnEnemies();

		currentDungeonParam = dungeonTypes[floorNumber - 1 < dungeonTypes.Length ? floorNumber - 1 : (int)(dungeonTypes.Length * Random.value)];
		RoomFirstDungeonGenerator dungeonWithRooms = dungeonGenerator.GetComponent<RoomFirstDungeonGenerator>();
		if (dungeonWithRooms != null)
			dungeonWithRooms.dungeonParams = currentDungeonParam;

		dungeonGenerator.GenerateDungeon();
		SpawnDungeonExit();

		yield return new WaitForSeconds(transitionTextTime);

		SpawnPlayer();
		SpawnEnemies();
		RecalcEnemiesStagger();

		LineDrawer.Instance.ResetPoints();

		fadeOutScreen.FadeOutCoroutine();
	}

	void SpawnDungeonExit()
	{
		collectedKey = false;
		RoomFirstDungeonGenerator keyGenerator = dungeonGenerator.GetComponent<RoomFirstDungeonGenerator>();
		if (keyGenerator != null)
		{
			Vector2 keyLocation = keyGenerator.GetTreasureLocation();
			if (dungeonKey == null)
			{
				dungeonKey = Instantiate(keyPrefab, keyLocation, Quaternion.identity);
				dungeonKey.OnPlayerEnter.AddListener(CollectedKey);
			}
			else
			{
				dungeonKey.transform.position = keyLocation;
				dungeonKey.gameObject.SetActive(true);
			}

			collectedKey = false;
		}
		else
			collectedKey = true;

		// generate exit
		Vector2 exitLocation = dungeonGenerator.GetExitLocation();
		if (dungeonExit == null) dungeonExit = Instantiate(exitPrefab, exitLocation, Quaternion.identity);
		else dungeonExit.transform.position = exitLocation;

		dungeonExit.OnPlayerEnter.RemoveAllListeners();
		if (collectedKey)
		{
			dungeonExit.OnPlayerEnter.AddListener(GenerateLevel);
			dungeonExit.GetComponent<SpriteRenderer>().sprite = exitUnlocked;
		}
		else
			dungeonExit.GetComponent<SpriteRenderer>().sprite = exitLocked;
	}

	public void CollectedKey()
	{
		collectedKey = true;

		if (dungeonKey != null) // remove key
			dungeonKey.gameObject.SetActive(false);

		if (dungeonExit != null) // unlock exit
		{
			dungeonExit.OnPlayerEnter.AddListener(GenerateLevel);
			dungeonExit.GetComponent<SpriteRenderer>().sprite = exitUnlocked;
		}
	}

	public void SpawnPlayer()
	{
		SpawnPlayer(dungeonGenerator.GetSpawnLocation());
	}

	void SpawnPlayer(Vector2 location)
	{
		bool justSpawnedCapturedEnemy = false;
		if (playerObj == null)
		{
			playerObj = Instantiate(playerPrefab, location, Quaternion.identity);
			playerObj.healthbar = healthbarPlayer;
			playerObj.xpbar = xpbar;
			playerObj.lvText = lvText;
			playerObj.nameText = nameText;

			// has selected character
			if (SaveManager.Instance.CurrentMiscData.selectedCharacter > 0 && capturedEnemy == null)
			{
				capturedEnemy = SpawnEnemy(location);
				capturedEnemy.stats = currentDungeonParam.enemyTypes[SaveManager.Instance.CurrentMiscData.selectedCharacter - 1];
				capturedEnemy.StartControllingAfterLoad(playerObj);
				justSpawnedCapturedEnemy = true;
			}
		}
		else playerObj.transform.position = location;

		if (capturedEnemy != null)
		{
			enemyObjs.Add(capturedEnemy);
			if (!justSpawnedCapturedEnemy)
			{
				capturedEnemy.transform.position = location;
				playerObj.transform.localPosition = Vector2.zero;
			}
		}

		cameraObj.target = playerObj.transform;
		cameraObj.SetPositionToTarget();

		CameraDragController2D dragController = cameraObj.GetComponent<CameraDragController2D>();
		if (dragController != null) dragController.SetDragOrigin();
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
					for (int i = 0; i < floorNumber; ++i)
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
		enemy.pathfinding.homeRoom = room;
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

		EnemyStats[] roulette = currentDungeonParam.enemyTypes;
		if (roulette.Length <= 0) roulette = enemyTypes;
		enemy.stats = roulette[Random.Range(0, roulette.Length)];

		enemy.target = playerObj.gameObject;

		enemy.level = floorNumber;

		enemy.pathfinding.tiles = dungeonGenerator.floorPositions;
		enemy.pathfinding.mapOffset = dungeonGenerator.GetTilemapOfset();
		enemy.pathfinding.neighborCache = dungeonGenerator.neighborCache;

		enemy.health.healthbarUIMonster = healthbarMonster;
		enemy.health.healthbarUIPlayer = healthbarPlayer;

		return enemy;
	}

	public void DespawnEnemies()
	{
		if (enemyObjs == null) return;
		else
		{
			capturedEnemy = null;
			for (int i = enemyObjs.Count-1; i >= 0; --i)
			{
				Enemy enemy = enemyObjs[i];

				if (enemy.IsBeingControlledByPlayer()) capturedEnemy = enemy;
				else Destroy(enemy.gameObject);
			}

			enemyObjs.Clear();
		}
	}

	public void RecalcEnemiesStagger()
	{
		int enemyCount = enemyObjs.Count;
		for (int i = 0; i < enemyCount; ++i)
		{
			enemyObjs[i].pathfinding.staggerPer = enemyCount * enemyStaggerMultiplier;
			enemyObjs[i].pathfinding.staggerIndex = i;
		}
	}

	public void PauseButton()
	{
		StartCoroutine(ChangeSceneCoroutine(titleSceneIndex));
	}
}