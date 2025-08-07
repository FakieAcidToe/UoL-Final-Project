using UnityEngine;

public class OptionsManager : GeneralManager
{
	[Header("Change Scene Properties")]
	[SerializeField] int titleSceneIndex = 0;

	public void BackButton()
	{
		SaveManager.Instance.Save();
		StartCoroutine(ChangeSceneCoroutine(titleSceneIndex));
	}
}