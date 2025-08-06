using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
	public void LoadSceneByName(string sceneName)
	{
		SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
	}

	public void LoadSceneByIndex(int sceneIndex)
	{
		SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
	}

	public void ReloadCurrentScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void QuitGame()
	{
		Debug.Log("Quit Game");
		Application.Quit();
	}
}