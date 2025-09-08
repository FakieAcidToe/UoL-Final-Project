using UnityEngine;
using UnityEngine.UI;

public class FullscreenManager : MonoBehaviour
{
	[SerializeField] Vector2Int windowedResolution = new Vector2Int(1280, 720);
	[SerializeField] Button button;
	[SerializeField] Sprite fullscreenSprite;
	[SerializeField] Sprite unfullscreenSprite;

	void Start()
	{
		UpdateButtonSprite();
	}

	void Update()
	{
		UpdateButtonSprite();
	}

	void UpdateButtonSprite()
	{
		if (button != null)
			button.image.sprite = Screen.fullScreenMode == FullScreenMode.Windowed ? fullscreenSprite : unfullscreenSprite;
	}

	void OnEnable()
	{
		if (button != null)
			button.onClick.AddListener(ToggleFullscreenMode);
	}

	void OnDisable()
	{
		if (button != null)
			button.onClick.RemoveListener(ToggleFullscreenMode);
	}

	public void ToggleFullscreenMode()
	{
		SetFullscreenMode(Screen.fullScreenMode == FullScreenMode.Windowed);
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
		UpdateButtonSprite();
	}
}