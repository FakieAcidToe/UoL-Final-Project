using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameplayManager : GeneralManager
{
	[Header("Scene References")]
	[SerializeField] AbstractDungeonGenerator dungeonGenerator;
	[SerializeField] CircleDungeonGenerator bossDungeonGenerator;
	[SerializeField] CameraFollow2D cameraObj;
	[SerializeField] HealthbarUI healthbarMonster;
	[SerializeField] HealthbarUI healthbarPlayer;
	[SerializeField] HealthbarUI xpbar;
	[SerializeField] UIFader fadeOutScreen; // UI that has floor number
	[SerializeField] Text transitionText;
	[SerializeField] Text lvText;
	[SerializeField] Text nameText;
	[SerializeField] Image itemIcon;
	[SerializeField] Text itemControlsText;
	[SerializeField] FogOfWarController fogController;

	[Header("Prefabs")]
	[SerializeField] PlayerMovement playerPrefab;
	[SerializeField] Enemy enemyPrefab;
	[SerializeField] PlayerPressurePlate exitPrefab;
	[SerializeField] PlayerPressurePlate keyPrefab;
	[SerializeField] TutArrow arrowPrefab;
	[SerializeField] ItemPrefab itemPrefab;

	[Header("Stats")]
	[SerializeField] PlayerUpgradeStats playerStats;

	[Header("Dungeon Generation")]
	[SerializeField] DungeonParamsSO[] dungeonTypes;
	DungeonParamsSO currentDungeonParam;

	[Header("Boss Dungeon Generation")]
	[SerializeField, Min(1)] int bossFloor = 5;
	[SerializeField] string bossFloorName = "Final Floor";
	[SerializeField] TilemapPalette bossTilemapPalette;
	[SerializeField] Vector2Int bossRoomSize = new Vector2Int(20, 20);
	[SerializeField] RoomDecoSO bossRoomDecorations;
	[SerializeField] EnemyStats bossStats;
	[SerializeField] Vector2 bossStartPosition;

	[Header("Items")]
	[SerializeField] PowerUpItem[] items;

	[Header("Sprites")]
	[SerializeField] Sprite exitLocked;
	[SerializeField] Sprite exitUnlocked;

	[Header("Audio")]
	[SerializeField] AudioClip keySFX;
	[SerializeField] AudioClip pickupSfx;

	[Header("Screen Transition Settings")]
	[SerializeField] float transitionTextTime = 2f;
	[SerializeField] string textBeforeNumber = "Floor";

	[Header("Enemy Settings")]
	[SerializeField, Min(1)] int enemyStaggerMultiplier = 1;
	[SerializeField] EnemyStats[] enemyTypes;
	[SerializeField, Min(0)] float enemyScalingNumPerRoom = 0.5f;

	[Header("Change Scene Properties")]
	[SerializeField] int titleSceneIndex = 0;
	[SerializeField] int gameoverSceneIndex = 4;

	[Header("Pause Menu")]
	[SerializeField] GameObject pauseMenuUI;
	bool isPaused = false;
	bool canPause = true;
	public PlayerInputActions controls { private set; get; }
	[SerializeField] AudioClip uiSound;
	[SerializeField] Button settingsBackButton;
	[SerializeField] Button pauseButton;

	// scene obj references
	PlayerMovement playerObj;
	Enemy capturedEnemy; // the captured enemy object between dungeons
	List<Enemy> enemyObjs; // list of enemies in current dungeon
	List<GameObject> decoObjs;
	PlayerPressurePlate dungeonExit;
	PlayerPressurePlate dungeonKey;
	TutArrow arrowObj;
	List<ItemPrefab> itemObjs;
	List<BoundsInt> roomList;
	List<bool> roomIsLit;

	bool collectedKey = false;
	int floorNumber = 0;

	protected override void Awake()
	{
		base.Awake();

		Time.timeScale = 1f;

		controls = KeybindLoader.GetNewInputActions();
		OnChangeBindings();

		enemyObjs = new List<Enemy>();
		decoObjs = new List<GameObject>();
		itemObjs = new List<ItemPrefab>();

		roomList = new List<BoundsInt>();
		roomIsLit = new List<bool>();

		playerStats.ResetStats();

		healthbarMonster.SetHealth(0, false);
	}

	protected override void Start()
	{
		base.Start();
		SaveManager.Instance.ResetPlayData();

		GenerateLevel();
	}

	void OnEnable()
	{
		controls.UI.Enable();
		controls.UI.Pause.performed += OnPause;
		SaveManager.Instance.onChangeBindings.AddListener(OnChangeBindings);
	}

	void OnDisable()
	{
		controls.UI.Pause.performed -= OnPause;
		controls.UI.Disable();
		SaveManager.Instance.onChangeBindings.RemoveListener(OnChangeBindings);
	}

	void OnChangeBindings()
	{
		itemControlsText.text = controls.Gameplay.Item.GetBindingDisplayString(0);
	}

	void OnPause(InputAction.CallbackContext context)
	{
		if (canPause)
		{
			if (settingsBackButton.IsActive())
				settingsBackButton.onClick.Invoke();
			else if (isPaused)
			{
				PlaySFX(uiSound);
				Resume();
			}
			else
			{
				PlaySFX(uiSound);
				Pause();
			}
		}
	}

	public void Resume()
	{
		if (canPause)
		{
			pauseMenuUI.SetActive(false);
			Time.timeScale = 1f;
			isPaused = false;
			pauseButton.gameObject.SetActive(true);
		}
	}

	public void Pause()
	{
		if (canPause)
		{
			pauseMenuUI.SetActive(true);
			Time.timeScale = 0f;
			isPaused = true;
			pauseButton.gameObject.SetActive(false);
		}
	}

	void OnPlayerDeath()
	{
		StartCoroutine(GameoverCoroutine());
	}

	public IEnumerator GameoverCoroutine()
	{
		LineDrawer.Instance.enabled = false;
		Resume();
		canPause = false;
		transitionText.text = "Game Over...";
		Save();

		if (fadeOutScreen.GetCurrentAlpha() < 1f) fadeOutScreen.FadeInCoroutine(3f);
		while (fadeOutScreen.GetCurrentAlpha() < 1f) yield return null;

		DespawnEnemies();
		SaveManager.Instance.CurrentMiscData.win = false;
		SaveManager.Instance.CurrentMiscData.currentPlayCharacter = 0;
		SaveManager.Instance.CurrentMiscData.levelsCleared = floorNumber - 1;

		yield return new WaitForSeconds(transitionTextTime);

		StartCoroutine(ChangeSceneCoroutine(gameoverSceneIndex));
	}

	void OnBossDefeat()
	{
		StartCoroutine(OnBossDefeatCoroutine());
	}

	IEnumerator OnBossDefeatCoroutine()
	{
		LineDrawer.Instance.enabled = false;
		Resume();
		canPause = false;
		transitionText.text = "YOU WIN!!";
		Save();

		if (fadeOutScreen.GetCurrentAlpha() < 1f) fadeOutScreen.FadeInCoroutine(3f);
		while (fadeOutScreen.GetCurrentAlpha() < 1f) yield return null;

		DespawnEnemies();
		SaveManager.Instance.CurrentMiscData.win = true;
		SaveManager.Instance.CurrentMiscData.currentPlayCharacter = playerObj.controllingEnemy == null ? 0 : playerObj.controllingEnemy.stats.id + 1;
		SaveManager.Instance.CurrentMiscData.levelsCleared = floorNumber;

		yield return new WaitForSeconds(transitionTextTime);

		StartCoroutine(ChangeSceneCoroutine(gameoverSceneIndex));
	}

	void Update()
	{
		if (floorNumber == 1)
			PlaceArrowOnSpareEnemy();
		UpdateRoomFog();
	}

	public void GenerateLevel()
	{
		StartCoroutine(GenerateLevelCoroutine());
	}

	public IEnumerator GenerateLevelCoroutine()
	{
		++floorNumber;
		if (floorNumber == bossFloor)
			transitionText.text = bossFloorName;
		else
			transitionText.text = textBeforeNumber + ' ' + floorNumber.ToString();

		foreach (Enemy enemy in enemyObjs)
			enemy.ScreenTransitionState();

		if (fadeOutScreen.GetCurrentAlpha() < 1f) fadeOutScreen.FadeInCoroutine();
		while (fadeOutScreen.GetCurrentAlpha() < 1f) yield return null;

		DespawnEnemies();

		if (arrowObj != null)
		{
			Destroy(arrowObj.gameObject);
			arrowObj = null;
		}

		DespawnItems();
		DespawnDecos();
		DespawnXPOrbs();

		fogController.DeleteTargets();
		roomList.Clear();
		roomIsLit.Clear();

		if (floorNumber == bossFloor)
		{
			bossDungeonGenerator.SetTilemapPalette(bossTilemapPalette);
			bossDungeonGenerator.SetDungeonSize(bossRoomSize);
			bossDungeonGenerator.GenerateDungeon();

			yield return new WaitForSeconds(transitionTextTime);

			SpawnBossDecoration();

			SpawnPlayer(bossDungeonGenerator.GetSpawnLocation());

			roomList.Add(ProceduralGenerationAlgorithms.GetBounds(bossDungeonGenerator.floorPositions));
			roomIsLit.Add(false);

			Enemy boss = SpawnEnemy(bossStartPosition + bossDungeonGenerator.GetTilemapOfset());
			boss.stats = bossStats;
			boss.onDefeat.AddListener(OnBossDefeat);
			RecalcEnemiesStagger();
		}
		else
		{
			currentDungeonParam = dungeonTypes[floorNumber - 1 < dungeonTypes.Length ? floorNumber - 1 : (int)(dungeonTypes.Length * Random.value)];
			RoomFirstDungeonGenerator dungeonWithRooms = dungeonGenerator.GetComponent<RoomFirstDungeonGenerator>();
			if (dungeonWithRooms != null)
				dungeonWithRooms.dungeonParams = currentDungeonParam;

			dungeonGenerator.GenerateDungeon();
			SpawnDungeonExit();

			yield return new WaitForSeconds(transitionTextTime);

			SpawnDecoration();

			SpawnPlayer();

			if (dungeonWithRooms != null) // must be after player spawn
			{
				roomList.AddRange(dungeonWithRooms.roomsList);
				roomIsLit = Enumerable.Repeat(false, roomList.Count).ToList();
			}
			else
			{
				roomList.Add(ProceduralGenerationAlgorithms.GetBounds(dungeonGenerator.floorPositions));
				roomIsLit.Add(false);
			}

			SpawnEnemies();
			RecalcEnemiesStagger();

			if (currentDungeonParam.hasPowerUpItem)
				SpawnItem(dungeonGenerator.floorPositions);
		}

		LineDrawer.Instance.ResetPoints();

		fadeOutScreen.FadeOutCoroutine();
	}

	void SpawnDungeonExit()
	{
		collectedKey = false;
		RoomFirstDungeonGenerator keyGenerator = dungeonGenerator.GetComponent<RoomFirstDungeonGenerator>();
		if (currentDungeonParam.hasKey && keyGenerator != null)
		{
			Vector2 keyLocation = keyGenerator.GetTreasureLocation();
			if (dungeonKey == null)
			{
				dungeonKey = Instantiate(keyPrefab, keyLocation, Quaternion.identity);
				dungeonKey.OnPlayerEnter.AddListener((go) => CollectedKey());
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
			dungeonExit.OnPlayerEnter.AddListener((go) => GenerateLevel());
			dungeonExit.GetComponent<SpriteRenderer>().sprite = exitUnlocked;
		}
		else
			dungeonExit.GetComponent<SpriteRenderer>().sprite = exitLocked;
	}

	public void CollectedKey()
	{
		SoundManager.Instance.Play(keySFX);

		collectedKey = true;

		if (dungeonKey != null) // remove key
			dungeonKey.gameObject.SetActive(false);

		if (dungeonExit != null) // unlock exit
		{
			dungeonExit.OnPlayerEnter.AddListener((go) => GenerateLevel());
			dungeonExit.GetComponent<SpriteRenderer>().sprite = exitUnlocked;
		}
	}

	void SpawnDecoration()
	{
		RoomFirstDungeonGenerator dungeonWithRooms = dungeonGenerator.GetComponent<RoomFirstDungeonGenerator>();
		// if has rooms and has decorations
		if (dungeonWithRooms.roomsList != null && dungeonWithRooms.dungeonParams != null && dungeonWithRooms.dungeonParams.roomDecoration.Length > 0)
		{
			foreach (BoundsInt room in dungeonWithRooms.roomsList)
			{
				RoomDecoSO decoration = dungeonWithRooms.dungeonParams.roomDecoration[Mathf.FloorToInt(Random.value * dungeonWithRooms.dungeonParams.roomDecoration.Length)];
				if (decoration != null)
					decoObjs.AddRange(
						decoration.PlaceDecorations(
							ProceduralGenerationAlgorithms.GetTilesInRoom(dungeonWithRooms.floorPositions, room),
							dungeonWithRooms.GetTilemapVisualizer().transform
						)
					);
			}
		}
	}

	void SpawnBossDecoration()
	{
		if (bossRoomDecorations != null)
		{
			decoObjs.AddRange(
				bossRoomDecorations.PlaceDecorations(
					bossDungeonGenerator.floorPositions,
					bossDungeonGenerator.GetTilemapVisualizer().transform
				)
			);
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
			playerObj.onDeath.AddListener(OnPlayerDeath);
			playerObj.SetItemIcon(itemIcon, itemControlsText);
			playerObj.playerStats = playerStats;

			// has selected item
			if (SaveManager.Instance.CurrentMiscData.selectedUpgrade > 0)
			{
				playerObj.itemUser.PickUpItem(items[SaveManager.Instance.CurrentMiscData.selectedUpgrade - 1]);
			}

			// has selected character
			if (SaveManager.Instance.CurrentMiscData.selectedCharacter > 0 && capturedEnemy == null)
			{
				capturedEnemy = SpawnEnemy(location);
				capturedEnemy.stats = enemyTypes[SaveManager.Instance.CurrentMiscData.selectedCharacter - 1];
				capturedEnemy.ChangeState(Enemy.EnemyState.spared);
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

		fogController.AddRevealTarget(playerObj.transform, 0.3f);

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
			bool skippedExit = false;

			foreach (BoundsInt room in dungeonWithRooms.roomsList)
				if (skippedPlayer || playerPos.x < room.xMin || playerPos.x > room.xMax || playerPos.y < room.yMin || playerPos.y > room.yMax) // don't spawn in player room
				{
					Enemy enemy = null;
					int numEnemies = 1 + Mathf.FloorToInt(floorNumber * enemyScalingNumPerRoom);
					for (int i = 0; i < numEnemies; ++i)
						enemy = SpawnEnemy(dungeonGenerator.floorPositions, room);

					Vector2 exitLocation = dungeonGenerator.GetExitLocation();
					if (enemy != null && currentDungeonParam.minibossEnemy != null && !skippedExit && exitLocation.x >= room.xMin && exitLocation.x <= room.xMax && exitLocation.y >= room.yMin && exitLocation.y <= room.yMax) // in exit room
					{
						enemy.stats = currentDungeonParam.minibossEnemy;
						enemy.Bossify();
						skippedExit = true;
					}
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

		if (currentDungeonParam != null)
		{
			EnemyStats[] roulette = currentDungeonParam.enemyTypes;
			if (roulette.Length <= 0) roulette = enemyTypes;
			enemy.stats = roulette[Random.Range(0, roulette.Length)];
		}

		enemy.playerStats = playerStats;
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
			for (int i = enemyObjs.Count - 1; i >= 0; --i)
			{
				Enemy enemy = enemyObjs[i];

				if (enemy.IsBeingControlledByPlayer()) capturedEnemy = enemy;
				else
				{
					enemy.onDefeat.RemoveAllListeners();
					Destroy(enemy.gameObject);
				}
			}

			enemyObjs.Clear();
		}
	}

	ItemPrefab SpawnItem(HashSet<Vector2Int> floorTiles)
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

		ItemPrefab item = SpawnItem(randomPosition + dungeonGenerator.GetTilemapOfset());
		return item;
	}

	ItemPrefab SpawnItem(Vector2 position)
	{
		return SpawnItem(position, items[Random.Range(0, items.Length)]); // random item
	}

	ItemPrefab SpawnItem(Vector2 position, PowerUpItem itemUpgrade)
	{
		ItemPrefab item = Instantiate(itemPrefab, position, Quaternion.identity);
		item.itemSO = itemUpgrade;
		itemObjs.Add(item);
		return item;
	}

	void DespawnItems()
	{
		for (int i = itemObjs.Count - 1; i >= 0; --i)
		{
			ItemPrefab item = itemObjs[i];
			if (item != null && item.gameObject != null)
				Destroy(item.gameObject);
		}

		itemObjs.Clear();
	}

	void DespawnDecos()
	{
		for (int i = decoObjs.Count - 1; i >= 0; --i)
		{
			GameObject deco = decoObjs[i];
			if (deco != null)
				Destroy(deco);
		}

		decoObjs.Clear();
	}

	void DespawnXPOrbs()
	{
		GameObject[] allObjects = FindObjectsOfType<GameObject>();
		foreach (GameObject obj in allObjects)
			if (obj.layer == 9) // xp orb layer is 9
				Destroy(obj);
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

	public void ReturnToTitleButton()
	{
		Save();
		Time.timeScale = 1f;
		StartCoroutine(ChangeSceneCoroutine(titleSceneIndex));
	}

	void PlaceArrowOnSpareEnemy() // only apply on floor 1. somewhat expensive.
	{
		Enemy closestSparedEnemy = null;

		if (playerObj != null && playerObj.gameObject.activeSelf)
		{
			float dist = float.MaxValue;
			foreach (Enemy enemy in enemyObjs)
				if (enemy.state == Enemy.EnemyState.spared)
				{
					float currentDist = (enemy.transform.position - playerObj.transform.position).magnitude;
					if (currentDist < dist)
					{
						dist = currentDist;
						closestSparedEnemy = enemy;
					}
				}
		}

		if (closestSparedEnemy == null)
		{
			if (arrowObj != null)
			{
				arrowObj.SetOpacity(0f);
				if (arrowObj.GetOpacity() < 0.01f)
				{
					Destroy(arrowObj.gameObject);
					arrowObj = null;
				}
			}
		}
		else
		{
			Vector2 dir = (playerObj.transform.position - closestSparedEnemy.transform.position).normalized;

			if (arrowObj == null)
			{
				arrowObj = Instantiate(arrowPrefab);
				arrowObj.SetOpacity(0f, true);
				arrowObj.SetPosition(playerObj.transform.position, true);
			}
			arrowObj.SetOpacity(1f);
			arrowObj.SetPosition((Vector2)closestSparedEnemy.transform.position + dir * 1.2f);
			arrowObj.SetRotation(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
		}
	}

	public void OnIemBoxClick()
	{
		if (playerObj == null) return;

		ItemUser user = null;

		if (playerObj.controllingEnemy != null)
			user = playerObj.controllingEnemy.GetComponent<ItemUser>();
		else if(playerObj.isActiveAndEnabled)
			user = playerObj.GetComponent<ItemUser>();

		if (user != null && user.currentItem != null)
		{
			SpawnItem(user.transform.position, user.currentItem);
			user.DropItem();

			SoundManager.Instance.Play(pickupSfx);

			// trim item list
			itemObjs = itemObjs
				.Where(item => item != null && item.gameObject != null)
				.ToList();
		}
	}

	void UpdateRoomFog()
	{
		if (roomList != null && playerObj != null)
		{
			Vector2 playerPos = playerObj.transform.position;

			for (int i = 0; i < roomList.Count; i++)
				if (!roomIsLit[i])
				{
					BoundsInt room = roomList[i];
					if (playerPos.x >= room.xMin && playerPos.x <= room.xMax && playerPos.y >= room.yMin && playerPos.y <= room.yMax) // if player in room
					{
						roomIsLit[i] = true;
						fogController.AddRevealTarget(room.center, room.size.magnitude/32);
					}
				}
		}
	}
}