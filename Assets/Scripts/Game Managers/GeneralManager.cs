using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SceneChanger))]
public class GeneralManager : MonoBehaviour
{
	[Header("General Manager Properties")]
	[SerializeField, Tooltip("Auto set if null")] SceneChanger sceneChanger;
	[SerializeField] UIFader screenTransitionFader;
	[SerializeField] float transitionFadeTime = 0.3f;
	bool awaitingChangeScene;

	protected virtual void Awake()
	{
		awaitingChangeScene = false;
		if (sceneChanger == null) sceneChanger = GetComponent<SceneChanger>();
		if (screenTransitionFader != null && screenTransitionFader.GetCurrentAlpha() > 0f)
			screenTransitionFader.FadeOutCoroutine(transitionFadeTime);
	}

	protected IEnumerator ChangeSceneCoroutine(int sceneIndex)
	{
		if (awaitingChangeScene) yield break;

		awaitingChangeScene = true;
		if (screenTransitionFader != null && screenTransitionFader.GetCurrentAlpha() < 1f)
			screenTransitionFader.FadeInCoroutine(transitionFadeTime);
		while (screenTransitionFader != null && screenTransitionFader.GetCurrentAlpha() < 1f)
			yield return null;

		sceneChanger.LoadSceneByIndex(sceneIndex);
	}
}