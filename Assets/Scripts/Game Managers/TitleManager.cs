using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
	[Header("Scene References")]
	[SerializeField, Tooltip("Auto set if null")] SceneChanger sceneChanger;
	[SerializeField] UIFader fadeInScreen;

	[Header("Change Scene Properties")]
	[SerializeField] int gameplaySceneIndex = 1;
	[SerializeField] float fadeTime = 0.5f;

	void Awake()
	{
		if (sceneChanger == null) sceneChanger = GetComponent<SceneChanger>();
	}

	public void PlayButton()
	{
		StartCoroutine(PlayButtonCoroutine());
	}

	IEnumerator PlayButtonCoroutine()
	{
		if (fadeInScreen.GetCurrentAlpha() < 1f) fadeInScreen.FadeInCoroutine(fadeTime);
		while (fadeInScreen.GetCurrentAlpha() < 1f)
			yield return null;

		sceneChanger.LoadSceneByIndex(gameplaySceneIndex);
	}
}
