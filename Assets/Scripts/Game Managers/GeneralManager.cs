using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SceneChanger))]
public class GeneralManager : MonoBehaviour
{
	[Header("General Manager Properties")]
	[SerializeField, Tooltip("Auto set if null")] SceneChanger sceneChanger;
	[SerializeField] protected UIFader screenTransitionFader;
	[SerializeField] protected float transitionFadeTime = 0.3f;
	bool awaitingChangeScene;

	[Header("BGM")]
	[SerializeField] AudioClip bgmIntro = null;
	[SerializeField] AudioClip bgmLoop = null;

	protected virtual void Awake()
	{
		awaitingChangeScene = false;
		if (sceneChanger == null) sceneChanger = GetComponent<SceneChanger>();
		if (screenTransitionFader != null && screenTransitionFader.GetCurrentAlpha() > 0f)
			screenTransitionFader.FadeOutCoroutine(transitionFadeTime);
	}

	void Start()
	{
		if (bgmLoop != null)
			SoundManager.Instance.PlayMusic(bgmLoop, bgmIntro);
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

	public void ChangeScene(int _sceneIndex)
	{
		StartCoroutine(ChangeSceneCoroutine(_sceneIndex));
	}

	public void Save()
	{
		SaveManager.Instance.Save();
	}

	public void Quit()
	{
		Application.Quit();
	}

	// cannot reference sound manager directly in inspector since it's singleton
	public void PlaySFX(AudioClip _clip)
	{
		SoundManager.Instance.Play(_clip);
	}
}