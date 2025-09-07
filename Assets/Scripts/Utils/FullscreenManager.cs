using UnityEngine;
using UnityEngine.UI;

public class FullscreenManager : MonoBehaviour
{
	[SerializeField] Vector2Int windowedResolution = new Vector2Int(1024, 768);
	[SerializeField] Toggle toggle;
	[SerializeField] bool setSavedFullscreenModeOnStart = false;

	void Start()
	{
		if (setSavedFullscreenModeOnStart)
			SetFullscreenMode();

		if (toggle != null)
			toggle.isOn = SaveManager.Instance.CurrentMiscData.isFullscreen;
	}

	void OnEnable()
	{
		if (toggle != null)
			toggle.onValueChanged.AddListener(SetFullscreenMode);
	}

	void OnDisable()
	{
		if (toggle != null)
			toggle.onValueChanged.RemoveListener(SetFullscreenMode);
	}

	public void SetFullscreenMode()
	{
		SetFullscreenMode(SaveManager.Instance.CurrentMiscData.isFullscreen);
	}

	public void SetFullscreenMode(bool isFullscreen)
	{
		SetFullscreenMode(isFullscreen ? FullScreenMode.MaximizedWindow : FullScreenMode.Windowed);
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
				if (Screen.fullScreenMode == FullScreenMode.Windowed)
				{
					width = Screen.width;
					height = Screen.height;
				}
				else
				{
					width = windowedResolution.x;
					height = windowedResolution.y;
				}
				break;
		}

		Screen.SetResolution(width, height, fullscreenMode);
		SaveManager.Instance.CurrentMiscData.isFullscreen = fullscreenMode != FullScreenMode.Windowed;
		if (toggle != null)
			toggle.isOn = SaveManager.Instance.CurrentMiscData.isFullscreen;
	}
}