using System.Collections;
using UnityEngine;

public class OptionsManager : MonoBehaviour
{
	[Header("Scene References")]
	[SerializeField, Tooltip("Auto set if null")] SceneChanger sceneChanger;
	[SerializeField] UIFader screenTransitionFader;

	[Header("Change Scene Properties")]
	[SerializeField] int titleSceneIndex = 0;
	[SerializeField] float transitionFadeTime = 0.5f;

	void Awake()
	{
		if (sceneChanger == null) sceneChanger = GetComponent<SceneChanger>();
		if (screenTransitionFader.GetCurrentAlpha() > 0f) screenTransitionFader.FadeOutCoroutine(transitionFadeTime);
	}

	public void BackButton()
	{
		SaveManager.Instance.Save();
		StartCoroutine(ChangeSceneCoroutine(titleSceneIndex));
	}

	IEnumerator ChangeSceneCoroutine(int sceneIndex)
	{
		if (screenTransitionFader.GetCurrentAlpha() < 1f) screenTransitionFader.FadeInCoroutine(transitionFadeTime);
		while (screenTransitionFader.GetCurrentAlpha() < 1f)
			yield return null;

		sceneChanger.LoadSceneByIndex(sceneIndex);
	}
}
