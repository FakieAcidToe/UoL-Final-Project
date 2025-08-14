using UnityEngine;
using UnityEngine.UI;

public class FullscreenManager : MonoBehaviour
{
	[SerializeField] Vector2Int windowedResolution = new Vector2Int(1024, 768);
	[SerializeField] Dropdown dropdown;
	[SerializeField] bool setSavedFullscreenModeOnStart = false;

	void Start()
	{
		if (setSavedFullscreenModeOnStart)
			SetFullscreenMode();

		if (dropdown != null)
			dropdown.value = SaveManager.Instance.CurrentSaveData.windowType;
	}

	void OnEnable()
	{
		if (dropdown != null)
			dropdown.onValueChanged.AddListener(SetFullscreenMode);
	}

	void OnDisable()
	{
		if (dropdown != null)
			dropdown.onValueChanged.RemoveListener(SetFullscreenMode);
	}

	public void SetFullscreenMode()
	{
		SetFullscreenMode(SaveManager.Instance.CurrentSaveData.windowType);
	}

	public void SetFullscreenMode(int fullscreenMode)
	{
		SetFullscreenMode((FullScreenMode)fullscreenMode);
	}

	public void SetFullscreenMode(FullScreenMode fullscreenMode)
	{
		int width;
		int height;

		switch (fullscreenMode)
		{
			case FullScreenMode.ExclusiveFullScreen:
			case FullScreenMode.FullScreenWindow:
			case FullScreenMode.MaximizedWindow:
				width = Display.main.systemWidth;
				height = Display.main.systemHeight;
				break;
			default:
			case FullScreenMode.Windowed:
				width = windowedResolution.x;
				height = windowedResolution.y;
				break;
		}

		Screen.SetResolution(width, height, fullscreenMode);
		SaveManager.Instance.CurrentSaveData.windowType = (int)fullscreenMode;
		if (dropdown != null)
			dropdown.value = SaveManager.Instance.CurrentSaveData.windowType;
	}
}
