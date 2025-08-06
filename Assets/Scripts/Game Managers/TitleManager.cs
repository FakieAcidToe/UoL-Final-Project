using System.Collections;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
	[Header("Scene References")]
	[SerializeField, Tooltip("Auto set if null")] SceneChanger sceneChanger;
	[SerializeField] UIFader screenTransitionFader;
	[SerializeField] UIFader dungeonFader;
	[SerializeField] AbstractDungeonGenerator dungeonGenerator;
	[SerializeField] Grid gridVisualiser;

	[Header("Change Scene Properties")]
	[SerializeField] int gameplaySceneIndex = 1;
	[SerializeField] float transitionFadeTime = 0.5f;

	[Header("Dungeon Generation Properties")]
	[SerializeField] float dungeonFadeTime = 0.3f;
	[SerializeField] float regenerateEverySeconds = 5f;
	float generateTimer;

	void Awake()
	{
		if (sceneChanger == null) sceneChanger = GetComponent<SceneChanger>();

		gridVisualiser.transform.position = Vector3.zero;
		dungeonGenerator.GenerateDungeon();
		gridVisualiser.transform.position = -dungeonGenerator.GetSpawnLocation();
		generateTimer = 0f;

		if (dungeonFader.GetCurrentAlpha() > 0f) dungeonFader.FadeOutCoroutine(dungeonFadeTime);
	}

	void Update()
	{
		generateTimer += Time.deltaTime;
		if (generateTimer > regenerateEverySeconds)
		{
			generateTimer = 0f;
			StartCoroutine(GenerateDungeon());
		}
	}

	IEnumerator GenerateDungeon()
	{
		if (dungeonFader.GetCurrentAlpha() < 1f) dungeonFader.FadeInCoroutine(dungeonFadeTime);
		while (dungeonFader.GetCurrentAlpha() < 1f) yield return null;

		gridVisualiser.transform.position = Vector3.zero;
		dungeonGenerator.GenerateDungeon();
		gridVisualiser.transform.position = -dungeonGenerator.GetSpawnLocation();

		if (dungeonFader.GetCurrentAlpha() > 0f) dungeonFader.FadeOutCoroutine(dungeonFadeTime);
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
		StartCoroutine(PlayButtonCoroutine());
	}

	IEnumerator PlayButtonCoroutine()
	{
		if (screenTransitionFader.GetCurrentAlpha() < 1f) screenTransitionFader.FadeInCoroutine(transitionFadeTime);
		while (screenTransitionFader.GetCurrentAlpha() < 1f)
			yield return null;

		sceneChanger.LoadSceneByIndex(gameplaySceneIndex);
	}
}
