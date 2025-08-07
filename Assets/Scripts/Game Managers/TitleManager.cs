using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
	[Header("Scene References")]
	[SerializeField, Tooltip("Auto set if null")] SceneChanger sceneChanger;
	[SerializeField] UIFader screenTransitionFader;
	[SerializeField] UIFader dungeonFader;
	[SerializeField] RoomFirstDungeonGenerator dungeonGenerator;
	[SerializeField] Grid gridVisualiser;

	[Header("Change Scene Properties")]
	[SerializeField] int gameplaySceneIndex = 1;
	[SerializeField] int optionsSceneIndex = 2;
	[SerializeField] float transitionFadeTime = 0.5f;

	[Header("Dungeon Generation Properties")]
	[SerializeField] float dungeonFadeTime = 0.3f;
	[SerializeField] float regenerateEverySeconds = 5f;
	[SerializeField] List<DungeonParamsSO> dungeonTypes;
	float generateTimer;
	Vector2 startPos;
	Vector2 endPos;
	Coroutine generationCoroutine;

	void Awake()
	{
		if (sceneChanger == null) sceneChanger = GetComponent<SceneChanger>();
		if (screenTransitionFader.GetCurrentAlpha() > 0f) screenTransitionFader.FadeOutCoroutine(transitionFadeTime);

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
		gridVisualiser.transform.position = Vector3.zero;

		dungeonGenerator.dungeonParams = dungeonTypes[(int)(dungeonTypes.Count * Random.value)];
		dungeonGenerator.GenerateDungeon();

		startPos = -dungeonGenerator.GetSpawnLocation();
		endPos = -dungeonGenerator.GetExitLocation();
		gridVisualiser.transform.position = startPos;

		generateTimer = 0f;
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

	public void PlayButton()
	{
		StartCoroutine(ChangeSceneCoroutine(gameplaySceneIndex));
	}

	public void OptionsButton()
	{
		StartCoroutine(ChangeSceneCoroutine(optionsSceneIndex));
	}

	IEnumerator ChangeSceneCoroutine(int sceneIndex)
	{
		if (screenTransitionFader.GetCurrentAlpha() < 1f) screenTransitionFader.FadeInCoroutine(transitionFadeTime);
		while (screenTransitionFader.GetCurrentAlpha() < 1f)
			yield return null;

		sceneChanger.LoadSceneByIndex(sceneIndex);
	}
}
