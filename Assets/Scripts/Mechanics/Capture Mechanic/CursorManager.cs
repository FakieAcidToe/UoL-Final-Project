using UnityEngine;

public class CursorManager : MonoBehaviour
{
	[SerializeField] Texture2D customCursor;

	void Start()
	{
		SetDefaultCursor();
	}

	public void SetCustomCursor(Texture2D cursorTexture = null)
	{
		Cursor.SetCursor(cursorTexture == null ? customCursor : cursorTexture, Vector2.zero, CursorMode.Auto);
	}

	// set the cursor back to the default system cursor
	public void SetDefaultCursor()
	{
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}
}