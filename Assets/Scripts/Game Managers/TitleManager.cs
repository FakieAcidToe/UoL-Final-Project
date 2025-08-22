using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : GeneralManager
{
	[Header("Scene References")]
	[SerializeField] UIFader dungeonFader;
	[SerializeField] RoomFirstDungeonGenerator dungeonGenerator;
	[SerializeField] Grid gridVisualiser;

	[Header("Dungeon Generation Properties")]
	[SerializeField] float dungeonFadeTime = 0.3f;
	[SerializeField] float regenerateEverySeconds = 5f;
	[SerializeField] List<DungeonParamsSO> dungeonTypes;
	float generateTimer;
	Vector2 startPos;
	Vector2 endPos;
	Coroutine generationCoroutine;

	bool exiting = false;
	List<GameObject> decoObjs;

	protected override void Awake()
	{
		base.Awake();

		decoObjs = new List<GameObject>();
		GenerateDungeon();

		if (dungeonFader.GetCurrentAlpha() > 0f) dungeonFader.FadeOutCoroutine(dungeonFadeTime);
	}

	void Update()
	{
		generateTimer += Time.deltaTime;
		if (generateTimer > regenerateEverySeconds && generationCoroutine == null)
			generationCoroutine = StartCoroutine(GenerateDungeonCoroutine());

		float t = Mathf.Clamp(generateTimer / regenerateEverySeconds, 0, 1);
		gridVisualiser.transform.position = new Vector2(
			EaseUtils.Interpolate(t, startPos.x, endPos.x, EaseUtils.EaseInOutSine),
			EaseUtils.Interpolate(t, startPos.y, endPos.y, EaseUtils.EaseInOutSine));
	}

	IEnumerator GenerateDungeonCoroutine()
	{
		if (dungeonFader.GetCurrentAlpha() < 1f) dungeonFader.FadeInCoroutine(dungeonFadeTime);
		while (dungeonFader.GetCurrentAlpha() < 1f) yield return null;

		GenerateDungeon();

		if (dungeonFader.GetCurrentAlpha() > 0f) dungeonFader.FadeOutCoroutine(dungeonFadeTime);
		generationCoroutine = null;
	}

	void GenerateDungeon()
	{
		DespawnDecos();
		gridVisualiser.transform.position = Vector3.zero;

		dungeonGenerator.dungeonParams = dungeonTypes[(int)(dungeonTypes.Count * Random.value)];
		dungeonGenerator.GenerateDungeon();

		startPos = -dungeonGenerator.GetSpawnLocation();
		endPos = -dungeonGenerator.GetExitLocation();

		SpawnDecoration();
		gridVisualiser.transform.position = startPos;

		generateTimer = 0f;
	}

	void SpawnDecoration()
	{
		if (dungeonGenerator.roomsList != null && dungeonGenerator.dungeonParams != null && dungeonGenerator.dungeonParams.roomDecoration.Length > 0)
		{
			foreach (BoundsInt room in dungeonGenerator.roomsList)
			{
				RoomDecoSO decoration = dungeonGenerator.dungeonParams.roomDecoration[Mathf.FloorToInt(Random.value * dungeonGenerator.dungeonParams.roomDecoration.Length)];
				if (decoration != null)
					decoObjs.AddRange(
						decoration.PlaceDecorations(
							ProceduralGenerationAlgorithms.GetTilesInRoom(dungeonGenerator.floorPositions, room),
							dungeonGenerator.GetTilemapVisualizer().transform
						)
					);
			}
		}
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

	public void FadeOutQuit()
	{
		StartCoroutine(FadeOutQuitCoroutine());
	}

	IEnumerator FadeOutQuitCoroutine()
	{
		if (exiting) yield break;
		exiting = true;

		if (screenTransitionFader != null && screenTransitionFader.GetCurrentAlpha() < 1f)
			screenTransitionFader.FadeInCoroutine(transitionFadeTime);
		while (screenTransitionFader != null && screenTransitionFader.GetCurrentAlpha() < 1f)
			yield return null;

		Quit();
	}

	/*Vector2 GetBoundingBoxCenter(HashSet<Vector2Int> tiles)
	{
		if (tiles == null || tiles.Count == 0)
			return Vector2.zero;

		int minX = tiles.Min(t => t.x);
		int maxX = tiles.Max(t => t.x);
		int minY = tiles.Min(t => t.y);
		int maxY = tiles.Max(t => t.y);

		float centerX = (minX + maxX) / 2f;
		float centerY = (minY + maxY) / 2f;

		return new Vector2(centerX, centerY);
	}*/
}